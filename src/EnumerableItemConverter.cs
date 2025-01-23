using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Soenneker.Json.CollectionConverter;

internal sealed class EnumerableItemConverter<TEnumerable, TItem> : CollectionItemConverterBase<TEnumerable, TItem>
    where TEnumerable : IEnumerable<TItem>
{
    public EnumerableItemConverter(JsonSerializerOptions options, JsonConverter converter) : base(options, converter) { }

    public override TEnumerable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotImplementedException($"Deserialization is not implemented for type {typeof(TEnumerable)}");
}