using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soenneker.Json.CollectionConverter.Base;

namespace Soenneker.Json.CollectionConverter;

public sealed class EnumerableItemConverter<TEnumerable, TItem, TItemConverter> : CollectionItemConverterBase<TEnumerable, TItem, TItemConverter> where TEnumerable : IEnumerable<TItem> where TItemConverter : JsonConverter, new()
{
    public EnumerableItemConverter(JsonSerializerOptions options, TItemConverter converter) : base(options, converter)
    {
    }

    public override TEnumerable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException(
        $"Deserialization is not implemented for type {typeof(TEnumerable)}");
}