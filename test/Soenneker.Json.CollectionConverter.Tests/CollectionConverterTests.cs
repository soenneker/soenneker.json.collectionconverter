using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using AwesomeAssertions;
using Newtonsoft.Json;
using Soenneker.Enums.DeployEnvironment;

namespace Soenneker.Json.CollectionConverter.Tests;

public class CollectionConverterTests
{
    [Test]
    public void Should_convert_with_systemtextjson()
    {
        var testClass = new TestClass
        {
            Environments = [DeployEnvironment.Local]
        };

        string result = System.Text.Json.JsonSerializer.Serialize(testClass);
    }

    [Test]
    public void Should_convert_with_jsonnet()
    {
        var testClass = new TestClass
        {
            Environments = [DeployEnvironment.Local]
        };

        string result = JsonConvert.SerializeObject(testClass);
    }

    [Test]
    public void Converter_factories_do_not_share_item_converter_instances()
    {
        var firstOptions = new JsonSerializerOptions();
        firstOptions.Converters.Add(new CollectionConverter<InstanceIntConverter>());

        var secondOptions = new JsonSerializerOptions();
        secondOptions.Converters.Add(new CollectionConverter<InstanceIntConverter>());

        string first = System.Text.Json.JsonSerializer.Serialize(new[] { 10 }, firstOptions);
        string second = System.Text.Json.JsonSerializer.Serialize(new[] { 10 }, secondOptions);

        first.Should().NotBe(second);
    }

    private sealed class InstanceIntConverter : System.Text.Json.Serialization.JsonConverter<int>
    {
        private static int _nextId;
        private readonly int _id = Interlocked.Increment(ref _nextId);

        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetInt32();

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) => writer.WriteNumberValue(_id);
    }
}
