[![](https://img.shields.io/nuget/v/Soenneker.Json.CollectionConverter.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Json.CollectionConverter/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.json.collectionconverter/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.json.collectionconverter/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Json.CollectionConverter.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Json.CollectionConverter/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.json.collectionconverter/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.json.collectionconverter/actions/workflows/codeql.yml)

# Soenneker.Json.CollectionConverter

A System.Text.Json converter for (de)serializing collections.

## Install

```bash
dotnet add package Soenneker.Json.CollectionConverter
```

## What you get

- `CollectionConverter<TItemConverter>` — A System.Text.Json converter for (de)serializing collections.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `CollectionConverter<TItemConverter>.CanConvert(typeToConvert)` | Executes the can convert operation. | A value indicating whether the operation succeeded. |
