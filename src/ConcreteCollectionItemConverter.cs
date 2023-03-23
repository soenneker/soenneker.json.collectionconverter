using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soenneker.Json.CollectionConverter.Base;

namespace Soenneker.Json.CollectionConverter;

public sealed class ConcreteCollectionItemConverter<TCollection, TEnumerable, TItem, TItemConverter> : CollectionItemConverterBase<TEnumerable, TItem, TItemConverter> where TItemConverter : JsonConverter, new()
    where TCollection : ICollection<TItem>, TEnumerable, new()
    where TEnumerable : IEnumerable<TItem>
{
    public ConcreteCollectionItemConverter(JsonSerializerOptions options, TItemConverter converter) : base(options, converter)
    {
    }

    public override TEnumerable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => BaseRead<TCollection>(ref reader);
}