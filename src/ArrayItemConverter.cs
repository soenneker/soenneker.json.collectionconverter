using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soenneker.Json.CollectionConverter.Base;

namespace Soenneker.Json.CollectionConverter;

public sealed class ArrayItemConverter<TItem, TItemConverter> : CollectionItemConverterBase<TItem[], TItem, TItemConverter> where TItemConverter : JsonConverter, new()
{
    public ArrayItemConverter(JsonSerializerOptions options, TItemConverter converter) : base(options, converter)
    {
    }

    public override TItem[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => BaseRead<List<TItem>>(ref reader).ToArray();
}