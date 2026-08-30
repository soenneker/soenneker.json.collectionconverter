[![](https://img.shields.io/nuget/v/Soenneker.Json.CollectionConverter.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Json.CollectionConverter/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.json.collectionconverter/build-and-test.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.json.collectionconverter/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.json.collectionconverter/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.json.collectionconverter/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Json.CollectionConverter.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Json.CollectionConverter/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.json.collectionconverter/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.json.collectionconverter/actions/workflows/codeql.yml)

# Soenneker.Json.CollectionConverter

A `System.Text.Json` converter factory that applies an item converter to arrays and compatible generic collections.

## Install

```bash
dotnet add package Soenneker.Json.CollectionConverter
```

## Register

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using Soenneker.Json.CollectionConverter;

var options = new JsonSerializerOptions();
options.Converters.Add(
    new CollectionConverter<JsonStringEnumConverter>());
```

The generic argument must be a parameterless `JsonConverter` or `JsonConverterFactory` whose `CanConvert()` accepts the collection's item type.

## Usage

```csharp
DayOfWeek[] days =
    JsonSerializer.Deserialize<DayOfWeek[]>(
        "[\"Monday\",\"Friday\"]",
        options)!;

string json = JsonSerializer.Serialize(days, options);
// ["Monday","Friday"]
```

The factory handles one-dimensional arrays, concrete `ICollection<T>` implementations with a public parameterless constructor, compatible set interfaces through `HashSet<T>`, and compatible collection interfaces through `List<T>`.

Collections that can be enumerated but cannot be constructed are serialization-only; attempting to deserialize one throws `NotImplementedException`. Strings, dictionaries, multidimensional arrays, and types exposing conflicting `IEnumerable<T>` item types are not handled by this factory.

The item converter is inserted ahead of the copied serializer converter list for item reads and writes. Each `CollectionConverter<TItemConverter>` instance owns its item converter and compiled collection factories, so converter state is not shared between separate serializer configurations.
