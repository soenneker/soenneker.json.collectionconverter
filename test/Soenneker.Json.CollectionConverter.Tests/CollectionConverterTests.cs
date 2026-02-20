using Newtonsoft.Json;
using Soenneker.Enums.DeployEnvironment;
using Xunit;

namespace Soenneker.Json.CollectionConverter.Tests;

public class CollectionConverterTests
{
    [Fact]
    public void Should_convert_with_systemtextjson()
    {
        var testClass = new TestClass
        {
            Environments = [DeployEnvironment.Local]
        };

        string result = System.Text.Json.JsonSerializer.Serialize(testClass);
    }

    [Fact]
    public void Should_convert_with_jsonnet()
    {
        var testClass = new TestClass
        {
            Environments = [DeployEnvironment.Local]
        };

        string result = JsonConvert.SerializeObject(testClass);
    }
}