using System.Collections.Generic;
using System.Linq;

using RemoteFlutterSharp.Dynamic;

namespace RemoteFlutterSharp.RemoteUi;

public static class CatalogData
{
    private static readonly IReadOnlyList<ProductRecord> Products =
    [
        new ProductRecord(
            Id: 1,
            Name: "Linden Desk",
            Category: "Home Office",
            PriceText: "$799",
            RatingText: "4.8",
            Highlights: new[]
            {
                "Robust FSC-certified oak – diseño escandinavo",
                "Superficie protegida con aceite natural sin VOC",
                "Garantía de 10 años"
            },
            Description: "La mesa Linden combina líneas limpias con almacenamiento oculto. Perfecta para estudios creativos y espacios híbridos.",
            Specifications: new[]
            {
                new Specification("Ancho", "140 cm"),
                new Specification("Alto", "76 cm"),
                new Specification("Profundidad", "70 cm"),
                new Specification("Peso", "32 kg"),
            }),
        new ProductRecord(
            Id: 2,
            Name: "Northwind Ergonomic Chair",
            Category: "Home Office",
            PriceText: "$389",
            RatingText: "4.6",
            Highlights: new[]
            {
                "Soporte lumbar con memoria",
                "Tejido transpirable made in Łódź",
                "Apoyabrazos 4D"
            },
            Description: "Nuestra silla Northwind equilibra ventilación y soporte continuo durante jornadas largas, con certificación BIFMA.",
            Specifications: new[]
            {
                new Specification("Altura del asiento", "45 – 55 cm"),
                new Specification("Peso máximo", "150 kg"),
                new Specification("Tapizado", "Malla reciclada"),
            }),
        new ProductRecord(
            Id: 3,
            Name: "Glass Terrarium",
            Category: "Decor",
            PriceText: "$129",
            RatingText: "4.9",
            Highlights: new[]
            {
                "Cristal templado hecho en Murano",
                "Incluye kit de musgo bosque-bañado",
                "Iluminación LED cálida integrada"
            },
            Description: "Terrario modular inspirado en bosques nórdicos; ideal para plantas de humedad media. Aroma a lluvia gracias al sustrato Kokedama.",
            Specifications: new[]
            {
                new Specification("Diámetro", "45 cm"),
                new Specification("Altura", "38 cm"),
                new Specification("Iluminación", "LED 2700K"),
            }),
        new ProductRecord(
            Id: 4,
            Name: "Espresso Machine Pro",
            Category: "Kitchen",
            PriceText: "$1199",
            RatingText: "4.7",
            Highlights: new[]
            {
                "Caldera doble en acero 316L",
                "Válvula de perfilado para crema perfecta",
                "Pantalla táctil multilingüe (Español, Polski, 日本語)"
            },
            Description: "Máquina espresso de precisión con tecnología PID y preinfusión adaptativa. Diseño artesanal desde Trieste, edición limitada.",
            Specifications: new[]
            {
                new Specification("Voltaje", "120 V"),
                new Specification("Caldera", "Doble"),
                new Specification("Depósito", "2.5 L"),
                new Specification("Peso", "18 kg"),
            }),
    ];

    public static string CreateCatalogJson()
    {
        var builder = new DynamicContentBuilder()
            .Set("catalog", new Dictionary<string, object>
            {
                ["items"] = Products.Select(product => new Dictionary<string, object>
                {
                    ["id"] = product.Id,
                    ["name"] = product.Name,
                    ["priceText"] = product.PriceText,
                    ["ratingText"] = product.RatingText,
                    ["category"] = product.Category,
                }).ToArray(),
            });

        return builder.ToJsonString();
    }

    public static bool TryGetProductDetail(int id, out Dictionary<string, object> detail)
    {
        var record = Products.FirstOrDefault(product => product.Id == id);
        if (record is null)
        {
            detail = default!;
            return false;
        }

        detail = new Dictionary<string, object>
        {
            ["id"] = record.Id,
            ["name"] = record.Name,
            ["priceText"] = record.PriceText,
            ["ratingText"] = record.RatingText,
            ["category"] = record.Category,
            ["description"] = record.Description,
            ["highlights"] = record.Highlights,
            ["specifications"] = record.Specifications.Select(specification => new Dictionary<string, object>
            {
                ["label"] = specification.Label,
                ["value"] = specification.Value,
            }).ToArray(),
        };

        return true;
    }

    private sealed record ProductRecord(
        int Id,
        string Name,
        string Category,
        string PriceText,
        string RatingText,
        IReadOnlyList<string> Highlights,
        string Description,
        IReadOnlyList<Specification> Specifications);

    private sealed record Specification(string Label, string Value);
}
