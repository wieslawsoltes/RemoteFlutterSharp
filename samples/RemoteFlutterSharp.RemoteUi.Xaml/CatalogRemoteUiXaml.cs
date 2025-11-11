using System;
using System.IO;
using System.Reflection;
using RemoteFlutterSharp.Rfw;
using RemoteFlutterSharp.RemoteUi;
using RemoteFlutterSharp.Xaml;

namespace RemoteFlutterSharp.RemoteUi.Xaml;

public static class CatalogRemoteUiXaml
{
    private const string ResourceName = "RemoteFlutterSharp.RemoteUi.Xaml.CatalogRemoteUi.xaml";

    private static readonly Lazy<RemoteWidgetLibrary> s_library = new(LoadLibraryFromResource);

    public static RemoteWidgetLibrary CreateLibrary() => s_library.Value;

    public static string CreateLibraryText() => CreateLibrary().ToText();

    public static string CreateDataJson() => CatalogData.CreateCatalogJson();

    private static RemoteWidgetLibrary LoadLibraryFromResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Unable to locate embedded XAML resource '{ResourceName}'.");
        }

        return RfwXamlLoader.LoadLibrary(stream);
    }
}
