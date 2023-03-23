using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Soenneker.Json.CollectionConverter.Base;

public abstract class CollectionItemConverterBase<TEnumerable, TItem, TItemConverter> : JsonConverter<TEnumerable> where TEnumerable : IEnumerable<TItem> where TItemConverter : JsonConverter, new()
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