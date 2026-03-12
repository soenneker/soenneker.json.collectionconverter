using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Soenneker.Json.CollectionConverter;

internal sealed class ConcreteCollectionItemConverter<TCollection, TEnumerable, TItem> :
    CollectionItemConverterBase<TEnumerable, TItem>
    where TCollection : ICollection<TItem>, TEnumerable, new()
    where TEnumerable : IEnumerable<TItem>
{
    public ConcreteCollectionItemConverter(JsonSerializerOptions options, JsonConverter converter) : base(options, converter) { }

    public override TEnumerable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        BaseRead<TCollection>(ref reader);
}