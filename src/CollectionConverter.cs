using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soenneker.Extensions.Type;

namespace Soenneker.Json.CollectionConverter;

/// <summary>
/// A System.Text.Json converter for (de)serializing collections
/// </summary>
public class CollectionConverter<TItemConverter> : JsonConverterFactory where TItemConverter : JsonConverter, new()
{
    private readonly TItemConverter _itemConverter = new();

    public override bool CanConvert(Type? typeToConvert)
    {
        (Type? itemType, _, _) = GetItemType(typeToConvert);

        if (itemType == null)
            return false;

        return _itemConverter.CanConvert(itemType);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        (Type? itemType, bool isArray, bool isSet) = GetItemType(typeToConvert);

        if (itemType == null)
            return null;

        if (isArray)
        {
            return (JsonConverter) Activator.CreateInstance(typeof(ArrayItemConverter<,>).MakeGenericType(typeof(TItemConverter), itemType), options, _itemConverter)!;
        }

        if (!typeToConvert.IsAbstract && !typeToConvert.IsInterface && typeToConvert.GetConstructor(Type.EmptyTypes) != null)
        {
            return (JsonConverter) Activator.CreateInstance(typeof(ConcreteCollectionItemConverter<,,,>).MakeGenericType(typeof(TItemConverter), typeToConvert, typeToConvert, itemType), options,
                _itemConverter)!;
        }

        if (isSet)
        {
            Type setType = typeof(HashSet<>).MakeGenericType(itemType);

            if (typeToConvert.IsAssignableFrom(setType))
            {
                return (JsonConverter) Activator.CreateInstance(typeof(ConcreteCollectionItemConverter<,,,>).MakeGenericType(typeof(TItemConverter), setType, typeToConvert, itemType), options,
                    _itemConverter)!;
            }
        }
        else
        {
            Type listType = typeof(List<>).MakeGenericType(itemType);
            if (typeToConvert.IsAssignableFrom(listType))
            {
                return (JsonConverter) Activator.CreateInstance(typeof(ConcreteCollectionItemConverter<,,,>).MakeGenericType(typeof(TItemConverter), listType, typeToConvert, itemType), options,
                    _itemConverter)!;
            }
        }

        // OK it's not an array and we can't find a parameterless constructor for the type. We can serialize, but not deserialize.
        return (JsonConverter) Activator.CreateInstance(typeof(EnumerableItemConverter<,,>).MakeGenericType(typeof(TItemConverter), typeToConvert, itemType), options, _itemConverter)!;
    }

    private static (Type? Type, bool IsArray, bool isSet) GetItemType(Type? type)
    {
        if (type == null || type.IsPrimitive || type == typeof(string) || typeof(IDictionary).IsAssignableFrom(type))
            return (null, false, false);

        if (type.IsArray)
            return type.GetArrayRank() == 1 ? (type.GetElementType(), true, false) : (null, false, false);

        Type? itemType = null;
        var isSet = false;

        foreach (Type iType in type.GetInterfacesAndSelf())
        {
            if (!iType.IsGenericType)
                continue;

            Type genType = iType.GetGenericTypeDefinition();

            if (genType == typeof(ISet<>))
            {
                isSet = true;
            }
            else if (genType == typeof(IEnumerable<>))
            {
                Type thisItemType = iType.GetGenericArguments()[0];

                if (itemType != null && itemType != thisItemType)
                {
                    return (null, false, false); // type implements multiple enumerable types simultaneously. 
                }

                itemType = thisItemType;
            }
            else if (genType == typeof(IDictionary<,>))
            {
                return (null, false, false);
            }
        }

        return (itemType, false, isSet);
    }
}