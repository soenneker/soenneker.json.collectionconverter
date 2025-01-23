using System;
using System.Collections;
using System.Collections.Generic;
using Soenneker.Extensions.Type;

namespace Soenneker.Json.CollectionConverter;

internal static class CollectionConverterUtils
{
    public static (Type? Type, bool IsArray, bool IsSet) GetItemType(Type? type)
    {
        // Handle invalid and primitive types early
        if (type == null || type.IsPrimitive || type == typeof(string) || typeof(IDictionary).IsAssignableFrom(type))
            return (null, false, false);

        // Handle arrays directly
        if (type.IsArray)
            return type.GetArrayRank() == 1 ? (type.GetElementType(), true, false) : (null, false, false);

        Type? itemType = null;
        var isSet = false;

        // Cache frequently used generic types
        Type iSetType = typeof(ISet<>);
        Type iEnumerableType = typeof(IEnumerable<>);
        Type iDictionaryType = typeof(IDictionary<,>);

        // Iterate over interfaces and the type itself
        foreach (Type iType in type.GetInterfacesAndSelf())
        {
            if (!iType.IsGenericType)
                continue;

            Type genericTypeDef = iType.GetGenericTypeDefinition();

            // Check if the type is a Set
            if (genericTypeDef == iSetType)
            {
                isSet = true;
            }
            // Check if the type is an IEnumerable<>
            else if (genericTypeDef == iEnumerableType)
            {
                Type thisItemType = iType.GetGenericArguments()[0];

                // Ensure only one enumerable type is supported
                if (itemType != null && itemType != thisItemType)
                    return (null, false, false);

                itemType = thisItemType;
            }
            // Check if the type is a Dictionary
            else if (genericTypeDef == iDictionaryType)
            {
                return (null, false, false);
            }

            // Short-circuit if both itemType and isSet are determined
            if (itemType != null && isSet)
                break;
        }

        return (itemType, false, isSet);
    }
}