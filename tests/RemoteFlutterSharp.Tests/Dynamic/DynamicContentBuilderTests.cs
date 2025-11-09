using RemoteFlutterSharp.Dynamic;

namespace RemoteFlutterSharp.Tests.Dynamic;

public sealed class DynamicContentBuilderTests
{
    [Fact]
    public void Build_NormalizesSupportedTypes()
    {
        var builder = new DynamicContentBuilder()
            .Set("payload", new Dictionary<string, object>
            {
                ["int"] = 7,
                ["float"] = 3.14f,
                ["double"] = 2.5,
                ["decimal"] = 9.9m,
                ["bool"] = true,
                ["string"] = "text",
                ["list"] = new object[] { 1, 2.0, false, "done" },
                ["map"] = new Dictionary<string, object>
                {
                    ["nested"] = 42,
                },
            });

        var root = builder.Build();
        var payload = Assert.IsType<Dictionary<string, object>>(root["payload"]);

        Assert.Equal(7L, payload["int"]);
    var floatValue = Assert.IsType<double>(payload["float"]);
    Assert.Equal(3.14d, floatValue, 4);

    var doubleValue = Assert.IsType<double>(payload["double"]);
    Assert.Equal(2.5d, doubleValue, 4);

    var decimalValue = Assert.IsType<double>(payload["decimal"]);
    Assert.Equal(9.9d, decimalValue, 4);
        Assert.True((bool)payload["bool"]);
        Assert.Equal("text", payload["string"]);

        var list = Assert.IsType<List<object>>(payload["list"]);
    Assert.Equal(new object[] { 1L, 2.0d, false, "done" }, list);

        var map = Assert.IsType<Dictionary<string, object>>(payload["map"]);
        Assert.Equal(42L, map["nested"]);
    }

    [Fact]
    public void Set_ThrowsOnUnsupportedType()
    {
        var builder = new DynamicContentBuilder();
        Assert.Throws<ArgumentException>(() => builder.Set("invalid", new object()));
    }
}
