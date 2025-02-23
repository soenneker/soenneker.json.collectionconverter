using System.Collections.Generic;
using System.Text.Json.Serialization;
using Soenneker.Enums.DeployEnvironment;

namespace Soenneker.Json.CollectionConverter.Tests;

public class TestClass
{
    [JsonPropertyName("daysOfWeek")]
    public List<DeployEnvironment>? Environments { get; set; }
}