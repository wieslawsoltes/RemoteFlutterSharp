using System.IO;
using System.Linq;
using RemoteFlutterSharp.Xaml;

namespace RemoteFlutterSharp.Tests;

public class XamlLoaderTests
{
    private static string ResolveRepositoryPath(params string[] segments)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        return Path.Combine(new[] { root }.Concat(segments).ToArray());
    }

    [Fact]
    public void Loader_can_parse_sample_library()
    {
        var samplePath = ResolveRepositoryPath("samples", "RemoteFlutterSharp.RemoteUi.Xaml", "CatalogRemoteUi.xaml");
        Assert.True(File.Exists(samplePath));

        var xaml = File.ReadAllText(samplePath);
        var library = RfwXamlLoader.LoadLibrary(xaml);

        Assert.Equal("catalog", library.LibraryName);
        Assert.Contains(library.Widgets, w => w.Name == "root");

        var text = library.ToText();
        Assert.Contains("widget root", text);
        Assert.Contains("catalog.select", text);
    }
}
