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
            return (JsonConverter) Activator.CreateInstance(typeof(ArrayItemConverter<>).MakeGenericType(typeof(TItemConverter), itemType), options, _itemConverter)!;
        }

        if (!typeToConvert.IsAbstract && !typeToConvert.IsInterface && typeToConvert.GetConstructor(Type.EmptyTypes) != null)
        {
            return (JsonConverter) Activator.CreateInstance(typeof(ConcreteCollectionItemConverter<,,>).MakeGenericType(typeof(TItemConverter), typeToConvert, typeToConvert, itemType), options, _itemConverter)!;
        }

        if (isSet)
        {
            Type setType = typeof(HashSet<>).MakeGenericType(itemType);

            if (typeToConvert.IsAssignableFrom(setType))
            {
                return (JsonConverter) Activator.CreateInstance(typeof(ConcreteCollectionItemConverter<,,>).MakeGenericType(typeof(TItemConverter), setType, typeToConvert, itemType), options, _itemConverter)!;
            }
        }
        else
        {
            Type listType = typeof(List<>).MakeGenericType(itemType);
            if (typeToConvert.IsAssignableFrom(listType))
            {
                return (JsonConverter) Activator.CreateInstance(typeof(ConcreteCollectionItemConverter<,,>).MakeGenericType(typeof(TItemConverter), listType, typeToConvert, itemType), options, _itemConverter)!;
            }
        }

        // OK it's not an array and we can't find a parameterless constructor for the type. We can serialize, but not deserialize.
        return (JsonConverter) Activator.CreateInstance(typeof(EnumerableItemConverter<,>).MakeGenericType(typeof(TItemConverter), typeToConvert, itemType), options, _itemConverter)!;
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

    private abstract class CollectionItemConverterBase<TEnumerable, TItem> : JsonConverter<TEnumerable> where TEnumerable : IEnumerable<TItem>
    {
        private readonly JsonSerializerOptions _modifiedOptions;

        protected CollectionItemConverterBase(JsonSerializerOptions options, TItemConverter converter)
        {
            // Clone the incoming options and insert the item converter at the beginning of the clone.
            // Then if converter is actually a JsonConverterFactory (e.g. JsonStringEnumConverter) then the correct JsonConverter<T> will be manufactured or fetched.
            _modifiedOptions = new JsonSerializerOptions(options);
            _modifiedOptions.Converters.Insert(0, converter);
        }

        protected TCollection BaseRead<TCollection>(ref Utf8JsonReader reader) where TCollection : ICollection<TItem>, new()
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException(); // Unexpected token type

            var list = new TCollection();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                list.Add(JsonSerializer.Deserialize<TItem>(ref reader, _modifiedOptions)!); // fail hard
            }

            return list;
        }

        public sealed override void Write(Utf8JsonWriter writer, TEnumerable value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (TItem item in value)
            {
                JsonSerializer.Serialize(writer, item, _modifiedOptions);
            }

            writer.WriteEndArray();
        }
    }

    private sealed class ArrayItemConverter<TItem> : CollectionItemConverterBase<TItem[], TItem>
    {
        public ArrayItemConverter(JsonSerializerOptions options, TItemConverter converter) : base(options, converter)
        {
        }

        public override TItem[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => BaseRead<List<TItem>>(ref reader).ToArray();
    }

    private sealed class ConcreteCollectionItemConverter<TCollection, TEnumerable, TItem> : CollectionItemConverterBase<TEnumerable, TItem>
        where TCollection : ICollection<TItem>, TEnumerable, new()
        where TEnumerable : IEnumerable<TItem>
    {
        public ConcreteCollectionItemConverter(JsonSerializerOptions options, TItemConverter converter) : base(options, converter)
        {
        }

        public override TEnumerable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => BaseRead<TCollection>(ref reader);
    }

    private sealed class EnumerableItemConverter<TEnumerable, TItem> : CollectionItemConverterBase<TEnumerable, TItem> where TEnumerable : IEnumerable<TItem>
    {
        public EnumerableItemConverter(JsonSerializerOptions options, TItemConverter converter) : base(options, converter)
        {
        }

        public override TEnumerable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException(
            $"Deserialization is not implemented for type {typeof(TEnumerable)}");
    }
}

#nullable enable