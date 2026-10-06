using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Soenneker.Json.CollectionConverter;

/// <summary>Creates statically closed collection converters. Supply options backed by generated item metadata.</summary>
public static class CollectionConverters
{
    /// <summary>Creates an array converter applying the supplied item converter.</summary>
    public static JsonConverter<TItem[]> Array<TItem>(JsonSerializerOptions options, JsonConverter<TItem> converter) =>
        new ArrayItemConverter<TItem>(options, converter);

    /// <summary>Creates a collection converter applying the supplied item converter.</summary>
    public static JsonConverter<TCollection> Collection<TCollection, TItem>(JsonSerializerOptions options, JsonConverter<TItem> converter)
        where TCollection : ICollection<TItem>, new() =>
        new ConcreteCollectionItemConverter<TCollection, TCollection, TItem>(options, converter);
}
