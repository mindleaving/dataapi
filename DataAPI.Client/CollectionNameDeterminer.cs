using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataAPI.DataStructures.Attributes;
using DataAPI.DataStructures.DomainModels;

namespace DataAPI.Client
{
    internal static class CollectionNameDeterminer
    {
        public static readonly Dictionary<Type, string> TypeMap = new Dictionary<Type, string>();

        public static string GetCollectionName(Type type)
        {
            IEnumerable<Type> interfaces = type.GetInterfaces();
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
                interfaces = interfaces.Concat(baseTypeInterfaces);
                baseType = baseType.BaseType;
            }
            var dataStructuresAssembly = Assembly.GetAssembly(typeof(IId));
            var domainModels = interfaces
                .Where(x => x.Assembly == dataStructuresAssembly)
                .Except(new[] {typeof(IId) })
                .ToList();
            if (domainModels.Count == 0)
            {
                if (type.IsInterface)
                {
                    return RemoveGenericSuffix(type.Name).Substring(1); // Remove leading 'I'
                }
                return RemoveGenericSuffix(type.Name);
            }
            if(domainModels.Count > 1)
            {
                throw new Exception($"Cannot determine collection name for type '{type.FullName}' "
                                    + "because it implements more than one SharedDataStructures-interface. "
                                    + $"Please register the collection name using {nameof(DataApiClient)}.{nameof(DataApiClient.RegisterType)}");
            }
            var sharedDataStructuresInterface = domainModels[0];
            var interfaceName = RemoveGenericSuffix(sharedDataStructuresInterface.Name);
            return interfaceName.Substring(1); // Remove leading 'I'
        }

        private static string RemoveGenericSuffix(string typeName)
        {
            return typeName.Contains('`') 
                ? typeName.Substring(0, typeName.IndexOf('`')) 
                : typeName;
        }
    }
}