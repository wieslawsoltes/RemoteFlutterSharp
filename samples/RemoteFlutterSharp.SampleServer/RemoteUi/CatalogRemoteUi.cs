using System;
using System.Collections.Generic;

using RemoteFlutterSharp.Dynamic;
using RemoteFlutterSharp.Rfw;
using RemoteFlutterSharp.Rfw.Expressions;

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
            BuildCatalogScreen(),
            "The primary scaffold layout containing the catalog list.");

        builder.DefineWidget(
            "ProductCard",
            BuildProductCard(),
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

    private static RfwExpression BuildCatalogScreen() =>
        Widget("Scaffold", args => args
            .Argument("backgroundColor", HexColor(0xFFF5F5F5))
            .Argument("appBar", Widget("AppBar", appBar => appBar
                .Argument("title", LtrText(List("Remote Catalog")))))
            .Argument("body", Padding(Widget("ListView", list => list
                .Argument("children", List(
                    Item(Padding(
                        LtrText(List("Today"), text => text.Argument("style", Map(m => m
                            .Argument("fontSize", 20)
                            .Argument("fontWeight", "w600")))),
                        8,
                        4,
                        12,
                        12)),
                    For(
                        "product",
                        Reference("data", "catalog", "items"),
                        Widget("ProductCard", card => card.Argument("product", Reference("product")))
                    )
                ))), 16, 16, 16, 16))
        );

    private static RfwExpression BuildProductCard() =>
        Widget("Card", card => card.Argument("child", Widget("ListTile", tile => tile
            .Argument("title", LtrText(List(Reference("args", "product", "name")), text => text.Argument("style", Map(m => m.Argument("fontSize", 18)))))
            .Argument("subtitle", LtrText(
                List(
                    "Rating ",
                    Reference("args", "product", "ratingText"),
                    " • ",
                    Reference("args", "product", "category")),
                text => text.Argument("style", Map(m => m.Argument("color", HexColor(0xFF666666))))))
            .Argument("trailing", LtrText(List(Reference("args", "product", "priceText")), text => text.Argument("style", Map(m => m.Argument("fontWeight", "w600")))))
            .Argument("onTap", Event("catalog.select", args => args
                .Argument("id", Reference("args", "product", "id"))
                .Argument("name", Reference("args", "product", "name"))))
        )));

    private static RfwExpression LtrText(RfwExpression text, Action<RfwArgumentBuilder>? configure = null) =>
        Widget("Text", args =>
        {
            args.Argument("text", text);
            configure?.Invoke(args);
            args.Argument("textDirection", "ltr");
        });

    private static RfwExpression Padding(RfwExpression child, params RfwValue[] edges) =>
        Widget("Padding", args => args.Padding(edges).Argument("child", child));
}
