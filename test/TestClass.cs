using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Soenneker.Enums.DeployEnvironment;

namespace Soenneker.Json.CollectionConverter.Tests;

public class TestClass
{
    [JsonPropertyName("daysOfWeek")]
    [System.Text.Json.Serialization.JsonConverter(typeof(CollectionConverter<SmartEnumNameConverter<DeployEnvironment, int>>))]
    public List<DeployEnvironment>? Environments { get; set; }
}