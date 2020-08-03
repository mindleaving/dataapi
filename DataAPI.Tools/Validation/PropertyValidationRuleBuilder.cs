using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Commons.Extensions;
using Commons.Physics;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Validation;

namespace DataAPI.Tools.Validation
{
    public class PropertyValidationRuleBuilder
    {
        private readonly int maximumPathDepth;
        private readonly TypeValidationRuleBuilder typeValidationRuleBuilder;

        public PropertyValidationRuleBuilder(TypeValidationRuleBuilder typeValidationRuleBuilder, int maximumPathDepth)
        {
            this.typeValidationRuleBuilder = typeValidationRuleBuilder;
            this.maximumPathDepth = maximumPathDepth;
        }

        public List<string> Build(PropertyInfo property, string parentPath)
        {
            var propertyRules = new List<string>();
            var propertyName = property.Name;
            var propertyPath = string.IsNullOrEmpty(parentPath) ? propertyName : $"{parentPath}.{propertyName}";
            var propertyType = property.PropertyType;
            var dataTypeRule = BuildDataTypeRule(propertyPath, propertyType);
            if(dataTypeRule != null)
                propertyRules.Add(dataTypeRule);
            var validationAttributes = property.GetCustomAttributes<ValidationAttribute>().ToList();
            var isOptional = !validationAttributes.Select(x => x.GetType())
                .Intersect(
                    new[]
                    {
                        typeof(RequiredAttribute),
                        typeof(RequiredIfPropertyEqualToAttribute),
                        typeof(RequiredIfPropertyNotEqualToAttribute)
                    }).Any();
            foreach (var validationAttribute in validationAttributes)
            {
                string rule = null;
                switch (validationAttribute)
                {
                    case ItemCountAttribute itemCountAttribute:
                        rule = BuildItemCountRule(propertyPath, itemCountAttribute);
                        break;
                    case LessThanAttribute lessThanAttribute:
                        rule = BuildLessThan(propertyPath, lessThanAttribute);
                        break;
                    case NotEmptyAttribute notEmptyAttribute:
                        rule = BuildNotEmptyRule(propertyPath);
                        break;
                    case RangeOrNaNAttribute rangeOrNaNAttribute:
                        rule = BuildRangeRule(propertyPath, rangeOrNaNAttribute, allowNaN: true);
                        break;
                    case RequiredIfPropertyEqualToAttribute requiredIfPropertyHasValueAttribute:
                        rule = BuildRequiredIfPropertyHasValueRule(propertyPath, requiredIfPropertyHasValueAttribute);
                        break;
                    case RequiredIfPropertyNotEqualToAttribute requiredIfPropertyNotHasValueAttribute:
                        rule = BuildRequiredIfPropertyNotHasValueRule(propertyPath, requiredIfPropertyNotHasValueAttribute);
                        break;
                    case DataReferenceAttribute dataReferenceAttribute:
                        rule = BuildDataReferenceRule(propertyPath, dataReferenceAttribute);
                        break;
                    case GreaterThanAttribute greaterThanAttribute:
                        rule = BuildGreaterThan(propertyPath, greaterThanAttribute);
                        break;
                    case EmailAddressAttribute emailAddressAttribute:
                        rule = BuildRegularExpressionRule(propertyPath, new RegularExpressionAttribute("^[a-zA-Z0-9\\._-]+@[a-zA-Z0-9\\._-]+\\.[a-zA-Z0-9]+$"));
                        break;
                    case EnumDataTypeAttribute enumDataTypeAttribute:
                        if(enumDataTypeAttribute.CustomDataType != null)
                        {
                            propertyType = Assembly.GetAssembly(typeof(IId)).GetTypes()
                                .Where(t => t.IsEnum)
                                .SingleOrDefault(t => t.Name == enumDataTypeAttribute.CustomDataType);
                        }
                        else
                            throw new NotSupportedException();
                        break;
                    case UrlAttribute urlAttribute:
                        rule = BuildRegularExpressionRule(propertyPath, new RegularExpressionAttribute("^https?://.+$"));
                        break;
                    case DataTypeAttribute dataTypeAttribute:
                        if(dataTypeAttribute.CustomDataType != null)
                        {
                            propertyType = Assembly.GetAssembly(typeof(IId)).GetTypes()
                                .SingleOrDefault(t => t.Name == dataTypeAttribute.CustomDataType);
                        }
                        else
                            throw new NotSupportedException();
                        break;
                    case MaxLengthAttribute maxLengthAttribute:
                        if(propertyType == typeof(string))
                            rule = BuildStringLengthRule(propertyPath, new StringLengthAttribute(maxLengthAttribute.Length));
                        else if (propertyType.IsInstanceOfType(typeof(IList)))
                            rule = BuildItemCountRule(propertyPath, new ItemCountAttribute(0, maxLengthAttribute.Length));
                        else
                            throw new NotSupportedException();
                        break;
                    case MinLengthAttribute minLengthAttribute:
                        if (propertyType == typeof(string))
                            rule = BuildMinimumStringLengthRule(propertyPath, minLengthAttribute);
                        else if (propertyType.IsInstanceOfType(typeof(IList)))
                            rule = BuildMinimumItemsCountRule(propertyPath, minLengthAttribute);
                        else
                            throw new NotSupportedException();
                        break;
                    case RangeAttribute rangeAttribute:
                        rule = BuildRangeRule(propertyPath, rangeAttribute);
                        break;
                    case RegularExpressionAttribute regularExpressionAttribute:
                        rule = BuildRegularExpressionRule(propertyPath, regularExpressionAttribute);
                        break;
                    case RequiredAttribute requiredAttribute:
                        rule = BuildExistsRule(propertyPath);
                        break;
                    case StringLengthAttribute stringLengthAttribute:
                        rule = BuildStringLengthRule(propertyPath, stringLengthAttribute);
                        break;
                }

                if (rule == null)
                    continue;
                if (isOptional)
                {
                    rule += $" IF {BuildExistsRule(propertyPath)}";
                }

                propertyRules.Add(rule);
            }

            // Recurse to child-properties
            if (propertyType != null
                && !propertyType.IsPrimitive 
                && propertyPath.Count(c => c == '.') + 1 < maximumPathDepth)
            {
                var stringSerializedTypes = new [] { typeof(string), typeof(DateTime), typeof(DateTime?), typeof(TimeSpan), typeof(TimeSpan?), typeof(UnitValue) };
                if (typeof(IDictionary).IsAssignableFrom(propertyType))
                {
                    // Do nothing
                }
                else if (typeof(IList).IsAssignableFrom(propertyType))
                {
                    var itemType = propertyType.IsArray
                        ? propertyType.GetElementType()
                        : propertyType.GenericTypeArguments.SingleOrDefault();
                    if(itemType != null && !itemType.InSet(stringSerializedTypes))
                        propertyRules.AddRange(typeValidationRuleBuilder.Build(itemType, propertyPath));
                }
                else if (propertyType.InSet(stringSerializedTypes)) // These are serialized to string without further hierarchy
                {
                    // Go no deeper
                }
                else
                {
                    foreach (var childProperty in property.PropertyType.GetProperties())
                    {
                        propertyRules.AddRange(Build(childProperty, propertyPath));
                    }
                }
            }

            return propertyRules;
        }

        private static string BuildDataTypeRule(string propertyPath, Type propertyType)
        {
            // NOTE: Everything's a string, so rule would be redundant
            //if (propertyType == typeof(string))
            //    return $"{propertyPath} IS STRING";
            if (propertyType.InSet(typeof(int), typeof(uint), typeof(ushort), typeof(short), typeof(byte), typeof(sbyte)))
                return $"{propertyPath} IS INT";
            if (propertyType.IsNumber())
                return $"{propertyPath} IS NUMBER";
            if (propertyType == typeof(DateTime))
                return $"{propertyPath} IS DATE";
            return null;
        }

        private static string BuildDataReferenceRule(string propertyPath, DataReferenceAttribute dataReferenceAttribute)
        {
            return $"{propertyPath} IS REFERENCE TO {dataReferenceAttribute.DataType}";
        }

        private static string BuildIsDateRule(string propertyPath)
        {
            return $"{propertyPath} IS DATE";
        }

        private static string BuildMinimumItemsCountRule(string propertyPath, MinLengthAttribute minLengthAttribute)
        {
            return $"{propertyPath} HAS MORE THAN OR EQUAL TO {minLengthAttribute.Length} ITEMS";
        }

        private static string BuildMinimumStringLengthRule(string propertyPath, MinLengthAttribute minLengthAttribute)
        {
            return $"{propertyPath} HAS LENGTH GREATER THAN OR EQUAL TO {minLengthAttribute.Length}";
        }

        private static string BuildRangeRule(string propertyPath, RangeAttribute rangeAttribute, bool allowNaN = false)
        {
            var rule = $"{propertyPath} IS BETWEEN {rangeAttribute.Minimum} AND {rangeAttribute.Maximum}";
            if (allowNaN)
                rule += " OR IS NaN";
            return rule;
        }

        private static string BuildItemCountRule(string propertyPath, ItemCountAttribute itemCountAttribute)
        {
            if (itemCountAttribute.MinimumCount == itemCountAttribute.MaximumCount)
                return $"{propertyPath} HAS {itemCountAttribute.MinimumCount} ITEMS";
            return $"{propertyPath} HAS BETWEEN {itemCountAttribute.MinimumCount} AND {itemCountAttribute.MaximumCount} ITEMS";
        }

        private static string BuildNotEmptyRule(string propertyPath)
        {
            return $"{propertyPath} IS NOT EMPTY";
        }

        private static string BuildRequiredIfPropertyHasValueRule(string propertyPath, RequiredIfPropertyEqualToAttribute requiredIfPropertyEqualToAttribute)
        {
            if (requiredIfPropertyEqualToAttribute.Value == null)
            {
                return $"{propertyPath} IS NOT NULL IF {requiredIfPropertyEqualToAttribute.PropertyName} IS EMPTY";
            }
            var conditionalValue = requiredIfPropertyEqualToAttribute.Value.IsNumber() && !requiredIfPropertyEqualToAttribute.Value.GetType().IsEnum
                ? requiredIfPropertyEqualToAttribute.Value.ToString()
                : $"'{requiredIfPropertyEqualToAttribute.Value}'";
            return $"{propertyPath} IS NOT NULL IF {requiredIfPropertyEqualToAttribute.PropertyName} IS EQUAL TO {conditionalValue}";
        }

        private static string BuildRequiredIfPropertyNotHasValueRule(string propertyPath, RequiredIfPropertyNotEqualToAttribute requiredIfPropertyNotEqualToAttribute)
        {
            if (requiredIfPropertyNotEqualToAttribute.Value == null)
            {
                return $"{propertyPath} IS NOT NULL IF {requiredIfPropertyNotEqualToAttribute.PropertyName} IS NOT EMPTY";
            }
            var conditionalValue = requiredIfPropertyNotEqualToAttribute.Value.IsNumber() && !requiredIfPropertyNotEqualToAttribute.Value.GetType().IsEnum
                ? requiredIfPropertyNotEqualToAttribute.Value.ToString()
                : $"'{requiredIfPropertyNotEqualToAttribute.Value}'";
            return $"{propertyPath} IS NOT NULL IF {requiredIfPropertyNotEqualToAttribute.PropertyName} IS NOT EQUAL TO {conditionalValue}";
        }

        private static string BuildLessThan(string propertyPath, LessThanAttribute lessThanAttribute)
        {
            switch (lessThanAttribute.ReferenceType)
            {
                case ComparisonReferenceType.Static:
                    return $"{propertyPath} IS LESS THAN {lessThanAttribute.UpperLimit}";
                case ComparisonReferenceType.OtherProperty:
                    return $"{propertyPath} IS LESS THAN {lessThanAttribute.PropertyName}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string BuildGreaterThan(string propertyPath, GreaterThanAttribute greaterThanAttribute)
        {
            switch (greaterThanAttribute.ReferenceType)
            {
                case ComparisonReferenceType.Static:
                    return $"{propertyPath} IS GREATER THAN {greaterThanAttribute.LowerLimit}";
                case ComparisonReferenceType.OtherProperty:
                    return $"{propertyPath} IS GREATER THAN {greaterThanAttribute.PropertyName}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string BuildRegularExpressionRule(string propertyPath, RegularExpressionAttribute regularExpressionAttribute)
        {
            return $"{propertyPath} MATCHES {regularExpressionAttribute.Pattern}";
        }

        private static string BuildStringLengthRule(string propertyPath, StringLengthAttribute stringLengthAttribute)
        {
            return $"{propertyPath} IS STRING;" +
                   $"{propertyPath} HAS LENGTH BETWEEN {stringLengthAttribute.MinimumLength} AND {stringLengthAttribute.MaximumLength}";
        }

        private static string BuildExistsRule(string propertyPath)
        {
            return $"{propertyPath} EXISTS";
        }
    }
}
