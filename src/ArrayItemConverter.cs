using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Soenneker.Json.CollectionConverter;

internal sealed class ArrayItemConverter<TItem> : CollectionItemConverterBase<TItem[], TItem>
{
    public ArrayItemConverter(JsonSerializerOptions options, JsonConverter converter) : base(options, converter) { }

    public override TItem[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        BaseRead<List<TItem>>(ref reader).ToArray();
}