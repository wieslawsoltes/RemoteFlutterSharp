using System.Collections.Generic;

using RemoteFlutterSharp.Dynamic;
using RemoteFlutterSharp.Rfw;

using static RemoteFlutterSharp.Rfw.RfwDsl;

namespace RemoteFlutterSharp.SampleServer.RemoteUi;

public static class CatalogRemoteUi
{
    public static RemoteWidgetLibrary CreateLibrary()
    {
        var builder = new RemoteWidgetLibraryBuilder("catalog")
            .AddImport("core.widgets")
            .AddImport("material");

        builder.DefineWidget(
            "root",
            Widget("CatalogScreen"),
            "Entry point widget used by the Flutter host application.");

        builder.DefineWidget(
            "CatalogScreen",
            Widget(
                "Scaffold",
                ("backgroundColor", HexColor(0xFFF5F5F5)),
                ("appBar", Widget(
                    "AppBar",
                    ("title", Widget(
                        "Text",
                        ("text", List(Item(String("Remote Catalog")))),
                        ("textDirection", String("ltr"))
                    ))
                )),
                ("body", Widget(
                    "Padding",
                    ("padding", List(
                        Item(Double(16)),
                        Item(Double(16)),
                        Item(Double(16)),
                        Item(Double(16))
                    )),
                    ("child", Widget(
                        "ListView",
                        ("children", List(
                            Item(Widget(
                                "Padding",
                                ("padding", List(
                                    Item(Double(8)),
                                    Item(Double(4)),
                                    Item(Double(12)),
                                    Item(Double(12))
                                )),
                                ("child", Widget(
                                    "Text",
                                    ("text", List(Item(String("Today")))),
                                    ("style", Map(
                                        ("fontSize", Double(20)),
                                        ("fontWeight", String("w600"))
                                    )),
                                    ("textDirection", String("ltr"))
                                ))
                            )),
                            For(
                                "product",
                                Reference("data", "catalog", "items"),
                                Widget(
                                    "ProductCard",
                                    ("product", Reference("product"))
                                )
                            )
                        ))
                    ))
                ))
            ),
            "The primary scaffold layout containing the catalog list.");

        builder.DefineWidget(
            "ProductCard",
            Widget(
                "Card",
                ("child", Widget(
                    "ListTile",
                    ("title", Widget(
                        "Text",
                        ("text", List(Item(Reference("args", "product", "name")))),
                        ("style", Map(("fontSize", Double(18)))),
                        ("textDirection", String("ltr"))
                    )),
                    ("subtitle", Widget(
                        "Text",
                        ("text", List(
                            Item(String("Rating ")), 
                            Item(Reference("args", "product", "ratingText")),
                            Item(String(" • ")), 
                            Item(Reference("args", "product", "category"))
                        )),
                        ("style", Map(("color", HexColor(0xFF666666)))),
                        ("textDirection", String("ltr"))
                    )),
                    ("trailing", Widget(
                        "Text",
                        ("text", List(Item(Reference("args", "product", "priceText")))),
                        ("style", Map(("fontWeight", String("w600")))),
                        ("textDirection", String("ltr"))
                    )),
                    ("onTap", Event(
                        "catalog.select",
                        ("id", Reference("args", "product", "id")),
                        ("name", Reference("args", "product", "name"))
                    ))
                ))
            ),
            "Reusable tile that emits selection events back to the host.");

        return builder.Build();
    }

    public static string CreateLibraryText() => CreateLibrary().ToText();

    public static string CreateDataJson()
    {
        var content = new DynamicContentBuilder()
            .Set("catalog", new Dictionary<string, object>
            {
                ["items"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = 1,
                        ["name"] = "Linden Desk",
                        ["priceText"] = "$799",
                        ["ratingText"] = "4.8",
                        ["category"] = "Home Office",
                    },
                    new Dictionary<string, object>
                    {
                        ["id"] = 2,
                        ["name"] = "Northwind Ergonomic Chair",
                        ["priceText"] = "$389",
                        ["ratingText"] = "4.6",
                        ["category"] = "Home Office",
                    },
                    new Dictionary<string, object>
                    {
                        ["id"] = 3,
                        ["name"] = "Glass Terrarium",
                        ["priceText"] = "$129",
                        ["ratingText"] = "4.9",
                        ["category"] = "Decor",
                    },
                    new Dictionary<string, object>
                    {
                        ["id"] = 4,
                        ["name"] = "Espresso Machine Pro",
                        ["priceText"] = "$1199",
                        ["ratingText"] = "4.7",
                        ["category"] = "Kitchen",
                    },
                }
            });

        return content.ToJsonString();
    }
}
