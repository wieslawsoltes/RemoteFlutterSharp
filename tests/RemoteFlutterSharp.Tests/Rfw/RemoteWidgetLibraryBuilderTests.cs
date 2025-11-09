using RemoteFlutterSharp.Rfw;

using static RemoteFlutterSharp.Rfw.RfwDsl;

namespace RemoteFlutterSharp.Tests.Rfw;

public sealed class RemoteWidgetLibraryBuilderTests
{
    [Fact]
    public void ToText_GeneratesExpectedRfwtxt()
    {
        var library = new RemoteWidgetLibraryBuilder("catalog")
            .AddImport("core.widgets")
            .DefineWidget(
                "root",
                Widget(
                    "Text",
                    ("text", List(Item(String("Hello, Remote Flutter"))))
                ))
            .Build();

        var expected = """
import core.widgets;

widget root = Text(
  text: [
    "Hello, Remote Flutter",
  ],
);
""";

        Assert.Equal(Normalize(expected), Normalize(library.ToText()));
    }

    private static IReadOnlyList<string> Normalize(string value) => value
        .Replace("\r\n", "\n")
        .TrimEnd('\n')
        .Split('\n')
        .Select(line => line.TrimEnd())
        .ToArray();
}
