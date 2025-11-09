using System;
using RemoteFlutterSharp.Rfw;
using RemoteFlutterSharp.Rfw.Expressions;

using static RemoteFlutterSharp.Rfw.RfwDsl;

namespace RemoteFlutterSharp.RemoteUi;

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

        builder.DefineWidget(
            "ProductDetailScreen",
            BuildProductDetailScreen(),
            "Detailed product layout with highlights and specifications.");

        builder.DefineWidget(
            "SpecificationRow",
            BuildSpecificationRow(),
            "Row displaying a specification label/value pair.");

        return builder.Build();
    }

    public static string CreateLibraryText() => CreateLibrary().ToText();

    public static string CreateDataJson()
        => CatalogData.CreateCatalogJson();

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

    private static RfwExpression BuildProductDetailScreen()
    {
        static RfwExpression BuildAppBar() =>
            Widget("AppBar", bar => bar
                .Argument("leading", Widget("GestureDetector", detector => detector
                    .Argument("onTap", Event("catalog.back"))
                    .Argument("child", Padding(Widget("Icon", icon => icon
                        .Argument("codePoint", 0xE5C4)
                        .Argument("fontFamily", "MaterialIcons")), 8, 8, 8, 8))))
                .Argument("title", LtrText(List(Reference("data", "detail", "name")), title => title.Argument("style", Map(m => m.Argument("fontSize", 18))))));

        static RfwExpression BuildBody()
        {
        static RfwExpression PriceTile()
        {
            var ratingBadge = Padding(LtrText(List(Reference("data", "detail", "ratingText"))), 8, 4, 8, 4);
            var trailingCard = Widget("Card", trailing => trailing
                .Argument("color", HexColor(0xFFE1F5FE))
                .Argument("child", ratingBadge));

            return Widget("Card", priceCard => priceCard.Argument("child", Widget("ListTile", tile => tile
                .Argument("title", LtrText(List("Price"), title => title.Argument("style", Map(m => m.Argument("fontWeight", "w600")))))
                .Argument("subtitle", LtrText(List(Reference("data", "detail", "priceText")), text => text.Argument("style", Map(m => m.Argument("fontSize", 18)))))
                .Argument("trailing", trailingCard))));
        }

            var items = List(
                Item(LtrText(List(Reference("data", "detail", "category")), text => text.Argument("style", Map(m => m
                    .Argument("fontSize", 14)
                    .Argument("color", HexColor(0xFF7A7A7A)))))),
                Item(Padding(
                    LtrText(List(Reference("data", "detail", "description")), text => text.Argument("style", Map(m => m
                        .Argument("fontSize", 16)
                        .Argument("height", 1.4)))),
                    12,
                    0,
                    12,
                    16)),
                Item(PriceTile()),
                Item(Padding(LtrText(List("Highlights"), heading => heading.Argument("style", Map(m => m
                    .Argument("fontSize", 16)
                    .Argument("fontWeight", "w600")))), 16, 8, 8, 4)),
                For(
                    "highlight",
                    Reference("data", "detail", "highlights"),
                    Widget("ListTile", highlight => highlight
                        .Argument("leading", Widget("Icon", icon => icon
                            .Argument("codePoint", 0xE86C)
                            .Argument("fontFamily", "MaterialIcons")
                            .Argument("color", HexColor(0xFF558B2F))))
                        .Argument("title", LtrText(List(Reference("highlight")))))),
                Item(Padding(LtrText(List("Specifications"), heading => heading.Argument("style", Map(m => m
                    .Argument("fontSize", 16)
                    .Argument("fontWeight", "w600")))), 16, 16, 8, 4)),
                For(
                    "spec",
                    Reference("data", "detail", "specifications"),
                    Widget("SpecificationRow", spec => spec.Argument("spec", Reference("spec")))),
                Item(Padding(Widget("ElevatedButton", button => button
                    .Argument("onPressed", Event("catalog.buy", args => args
                        .Argument("id", Reference("data", "detail", "id"))
                        .Argument("name", Reference("data", "detail", "name"))))
                    .Argument("child", LtrText(List("Add to Cart")))), 16, 24, 16, 36))
            );

            return Padding(Widget("ListView", list => list.Argument("children", items)), 20, 12, 20, 12);
        }

        return Widget("Scaffold", args => args
            .Argument("appBar", BuildAppBar())
            .Argument("body", BuildBody()));
    }

    private static RfwExpression BuildSpecificationRow() =>
        Widget("ListTile", tile => tile
            .Argument("title", LtrText(List(Reference("args", "spec", "label")), text => text.Argument("style", Map(m => m.Argument("fontWeight", "w500")))))
            .Argument("trailing", LtrText(List(Reference("args", "spec", "value")), text => text.Argument("style", Map(m => m.Argument("color", HexColor(0xFF424242)))))));

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
