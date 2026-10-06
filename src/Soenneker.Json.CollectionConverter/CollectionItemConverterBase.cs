using System.Text.Json.Serialization.Metadata;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Soenneker.Json.CollectionConverter;

internal abstract class CollectionItemConverterBase<TEnumerable, TItem> : JsonConverter<TEnumerable>
    where TEnumerable : IEnumerable<TItem>
{
    private readonly JsonSerializerOptions _modifiedOptions;

    protected CollectionItemConverterBase(JsonSerializerOptions options, JsonConverter converter)
    {
        _modifiedOptions = new JsonSerializerOptions(options);
        _modifiedOptions.Converters.Insert(0, converter);
    }

    private JsonTypeInfo<TItem> ItemContract => (JsonTypeInfo<TItem>)_modifiedOptions.GetTypeInfo(typeof(TItem));

    protected TCollection BaseRead<TCollection>(ref Utf8JsonReader reader) where TCollection : ICollection<TItem>, new()
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        var list = new TCollection();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return list;

            list.Add(JsonSerializer.Deserialize(ref reader, ItemContract)!);
        }

        throw new JsonException("The collection JSON array was not terminated.");
    }

    public override void Write(Utf8JsonWriter writer, TEnumerable value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (TItem item in value)
        {
            JsonSerializer.Serialize(writer, item, ItemContract);
        }

        writer.WriteEndArray();
    }
}