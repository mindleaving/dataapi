using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataAPI.DataStructures.Attributes;

namespace DataAPI.Client
{
    internal static class CollectionNameDeterminer
    {
        public static readonly Dictionary<Type, string> TypeMap = new Dictionary<Type, string>();

        public static string GetCollectionName(Type type)
        {
            var baseType = type;
            while (baseType != null)
            {
                if (TypeMap.ContainsKey(baseType))
                    return TypeMap[baseType];
                var dataApiCollectionAttribute = baseType.GetCustomAttribute<DataApiCollectionAttribute>();
                if (dataApiCollectionAttribute != null)
                    return dataApiCollectionAttribute.CollectionName;
                var baseTypeInterfaces = baseType.GetInterfaces();
                dataApiCollectionAttribute = baseTypeInterfaces
                    .Select(x => x.GetCustomAttribute<DataApiCollectionAttribute>())
                    .FirstOrDefault(attr => attr != null);
                if (dataApiCollectionAttribute != null)
                    return dataApiCollectionAttribute.CollectionName;
                baseType = baseType.BaseType;
            }
            if (type.IsInterface)
            {
                return RemoveGenericSuffix(type.Name).Substring(1); // Remove leading 'I'
            }
            return RemoveGenericSuffix(type.Name);
        }

        private static string RemoveGenericSuffix(string typeName)
        {
            return typeName.Contains('`') 
                ? typeName.Substring(0, typeName.IndexOf('`')) 
                : typeName;
        }
    }
}