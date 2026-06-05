using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Soenneker.Json.CollectionConverter;

/// <summary>
/// A System.Text.Json converter for (de)serializing collections
/// </summary>
public sealed class CollectionConverter<TItemConverter> : JsonConverterFactory where TItemConverter : JsonConverter, new()
{
    private readonly TItemConverter _itemConverter = new();

    // Cache compiled factory methods for converters
    private static readonly ConcurrentDictionary<Type, Func<JsonSerializerOptions, JsonConverter>> _converterFactories = new();

    /// <summary>
    /// Executes the can convert operation.
    /// </summary>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <returns>A value indicating whether the operation succeeded.</returns>
    public override bool CanConvert(Type? typeToConvert)
    {
        if (typeToConvert == null)
            return false;

        (Type? itemType, _, _) = CollectionConverterUtils.GetItemType(typeToConvert);
        return itemType != null && _itemConverter.CanConvert(itemType);
    }

    /// <summary>
    /// Creates converter.
    /// </summary>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The options.</param>
    /// <returns>The result of the operation.</returns>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == null)
            return null;

        // Check cache first
        if (_converterFactories.TryGetValue(typeToConvert, out Func<JsonSerializerOptions, JsonConverter>? factory))
        {
            return factory(options);
        }

        // Get item type details
        (Type? itemType, bool isArray, bool isSet) = CollectionConverterUtils.GetItemType(typeToConvert);

        if (itemType == null)
            return null;

        Func<JsonSerializerOptions, JsonConverter>? factoryMethod = null;

        if (isArray)
        {
            factoryMethod = CreateFactory(
                typeof(ArrayItemConverter<>).MakeGenericType(itemType)
            );
        }
        else if (!typeToConvert.IsAbstract && !typeToConvert.IsInterface && typeToConvert.GetConstructor(Type.EmptyTypes) != null)
        {
            factoryMethod = CreateFactory(
                typeof(ConcreteCollectionItemConverter<,,>).MakeGenericType(typeToConvert, typeToConvert, itemType)
            );
        }
        else if (isSet)
        {
            Type setType = typeof(HashSet<>).MakeGenericType(itemType);
            if (typeToConvert.IsAssignableFrom(setType))
            {
                factoryMethod = CreateFactory(
                    typeof(ConcreteCollectionItemConverter<,,>).MakeGenericType(setType, typeToConvert, itemType)
                );
            }
        }
        else
        {
            Type listType = typeof(List<>).MakeGenericType(itemType);
            if (typeToConvert.IsAssignableFrom(listType))
            {
                factoryMethod = CreateFactory(
                    typeof(ConcreteCollectionItemConverter<,,>).MakeGenericType(listType, typeToConvert, itemType)
                );
            }
        }

        // Fallback for non-deserializable types
        if (factoryMethod == null)
        {
            factoryMethod = CreateFactory(
                typeof(EnumerableItemConverter<,>).MakeGenericType(typeToConvert, itemType)
            );
        }

        if (factoryMethod == null)
            return null;

        // Cache and return the converter
        _converterFactories[typeToConvert] = factoryMethod;
        return factoryMethod(options);
    }

    private Func<JsonSerializerOptions, JsonConverter> CreateFactory(Type converterType)
    {
        // Get the constructor with parameters (JsonSerializerOptions, TItemConverter)
        ConstructorInfo? constructor = converterType.GetConstructor([typeof(JsonSerializerOptions), typeof(TItemConverter)]);
        if (constructor == null)
            throw new InvalidOperationException($"No matching constructor found for {converterType}");

        // Parameters for the lambda
        ParameterExpression optionsParam = Expression.Parameter(typeof(JsonSerializerOptions), "options");
        ConstantExpression itemConverterConstant = Expression.Constant(_itemConverter, typeof(TItemConverter));

        // Constructor call
        NewExpression newExpression = Expression.New(constructor, optionsParam, itemConverterConstant);

        // Compile the expression tree into a delegate
        return Expression.Lambda<Func<JsonSerializerOptions, JsonConverter>>(newExpression, optionsParam).Compile();
    }
}