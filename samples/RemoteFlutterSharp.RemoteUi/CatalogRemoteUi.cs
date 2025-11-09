using RemoteFlutterSharp.Rfw;

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

        builder.DefineWidget(
            "ProductDetailScreen",
            Widget(
                "Scaffold",
                ("appBar", Widget(
                    "AppBar",
                    ("leading", Widget(
                        "GestureDetector",
                        ("onTap", Event("catalog.back")),
                        ("child", Widget(
                            "Padding",
                            ("padding", List(
                                Item(Double(8)),
                                Item(Double(8)),
                                Item(Double(8)),
                                Item(Double(8))
                            )),
                            ("child", Widget(
                                "Icon",
                                ("codePoint", Int(0xE5C4)),
                                ("fontFamily", String("MaterialIcons"))
                            ))
                        ))
                    )),
                    ("title", Widget(
                        "Text",
                        ("text", List(Item(Reference("data", "detail", "name")))),
                        ("style", Map(("fontSize", Double(18)))),
                        ("textDirection", String("ltr"))
                    ))
                )),
                ("body", Widget(
                    "Padding",
                    ("padding", List(
                        Item(Double(20)),
                        Item(Double(12)),
                        Item(Double(20)),
                        Item(Double(12))
                    )),
                    ("child", Widget(
                        "ListView",
                        ("children", List(
                            Item(Widget(
                                "Text",
                                ("text", List(Item(Reference("data", "detail", "category")))),
                                ("style", Map(
                                    ("fontSize", Double(14)),
                                    ("color", HexColor(0xFF7A7A7A))
                                )),
                                ("textDirection", String("ltr"))
                            )),
                            Item(Widget(
                                "Padding",
                                ("padding", List(
                                    Item(Double(12)),
                                    Item(Double(0)),
                                    Item(Double(12)),
                                    Item(Double(16))
                                )),
                                ("child", Widget(
                                    "Text",
                                    ("text", List(Item(Reference("data", "detail", "description")))),
                                    ("style", Map(
                                        ("fontSize", Double(16)),
                                        ("height", Double(1.4))
                                    )),
                                    ("textDirection", String("ltr"))
                                ))
                            )),
                            Item(Widget(
                                "Card",
                                ("child", Widget(
                                    "ListTile",
                                    ("title", Widget(
                                        "Text",
                                        ("text", List(Item(String("Price")))),
                                        ("style", Map(("fontWeight", String("w600")))),
                                        ("textDirection", String("ltr"))
                                    )),
                                    ("subtitle", Widget(
                                        "Text",
                                        ("text", List(Item(Reference("data", "detail", "priceText")))),
                                        ("style", Map(("fontSize", Double(18)))),
                                        ("textDirection", String("ltr"))
                                    )),
                                    ("trailing", Widget(
                                        "Card",
                                        ("color", HexColor(0xFFE1F5FE)),
                                        ("child", Widget(
                                            "Padding",
                                            ("padding", List(
                                                Item(Double(8)),
                                                Item(Double(4)),
                                                Item(Double(8)),
                                                Item(Double(4))
                                            )),
                                            ("child", Widget(
                                                "Text",
                                                ("text", List(Item(Reference("data", "detail", "ratingText")))),
                                                ("textDirection", String("ltr"))
                                            ))
                                        ))
                                    ))
                                ))
                            )),
                            Item(Widget(
                                "Padding",
                                ("padding", List(
                                    Item(Double(16)),
                                    Item(Double(8)),
                                    Item(Double(8)),
                                    Item(Double(4))
                                )),
                                ("child", Widget(
                                    "Text",
                                    ("text", List(Item(String("Highlights")))),
                                    ("style", Map(
                                        ("fontSize", Double(16)),
                                        ("fontWeight", String("w600"))
                                    )),
                                    ("textDirection", String("ltr"))
                                ))
                            )),
                            For(
                                "highlight",
                                Reference("data", "detail", "highlights"),
                                Widget(
                                    "ListTile",
                                    ("leading", Widget(
                                        "Icon",
                                        ("codePoint", Int(0xE86C)),
                                        ("fontFamily", String("MaterialIcons")),
                                        ("color", HexColor(0xFF558B2F))
                                    )),
                                    ("title", Widget(
                                        "Text",
                                        ("text", List(Item(Reference("highlight")))),
                                        ("textDirection", String("ltr"))
                                    ))
                                )
                            ),
                            Item(Widget(
                                "Padding",
                                ("padding", List(
                                    Item(Double(16)),
                                    Item(Double(16)),
                                    Item(Double(8)),
                                    Item(Double(4))
                                )),
                                ("child", Widget(
                                    "Text",
                                    ("text", List(Item(String("Specifications")))),
                                    ("style", Map(
                                        ("fontSize", Double(16)),
                                        ("fontWeight", String("w600"))
                                    )),
                                    ("textDirection", String("ltr"))
                                ))
                            )),
                            For(
                                "spec",
                                Reference("data", "detail", "specifications"),
                                Widget(
                                    "SpecificationRow",
                                    ("spec", Reference("spec"))
                                )
                            ),
                            Item(Widget(
                                "Padding",
                                ("padding", List(
                                    Item(Double(16)),
                                    Item(Double(24)),
                                    Item(Double(16)),
                                    Item(Double(36))
                                )),
                                ("child", Widget(
                                    "ElevatedButton",
                                    ("onPressed", Event(
                                        "catalog.buy",
                                        ("id", Reference("data", "detail", "id")),
                                        ("name", Reference("data", "detail", "name"))
                                    )),
                                    ("child", Widget(
                                        "Text",
                                        ("text", List(Item(String("Add to Cart")))),
                                        ("textDirection", String("ltr"))
                                    ))
                                ))
                            ))
                        ))
                    ))
                ))
            ),
            "Detailed product layout with highlights and specifications.");

        builder.DefineWidget(
            "SpecificationRow",
            Widget(
                "ListTile",
                ("title", Widget(
                    "Text",
                    ("text", List(Item(Reference("args", "spec", "label")))),
                    ("style", Map(("fontWeight", String("w500")))),
                    ("textDirection", String("ltr"))
                )),
                ("trailing", Widget(
                    "Text",
                    ("text", List(Item(Reference("args", "spec", "value")))),
                    ("style", Map(("color", HexColor(0xFF424242)))),
                    ("textDirection", String("ltr"))
                ))
            ),
            "Row displaying a specification label/value pair.");

        return builder.Build();
    }

    public static string CreateLibraryText() => CreateLibrary().ToText();

    public static string CreateDataJson()
        => CatalogData.CreateCatalogJson();
}
