using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using RemoteFlutterSharp.Dynamic;

namespace RemoteFlutterSharp.RemoteUi;

public static class CatalogData
{
private static readonly List<ProductRecord> Products = new()
{
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
    };

private static readonly List<CartItemRecord> CartItems = new()
    {
        new CartItemRecord(
            Id: Products[0].Id,
            Name: Products[0].Name,
            UnitPrice: 799m,
            Quantity: 1,
            Status: "Ready for final QC",
            Details: "Oak finish · Artisanal joinery",
            AccentColor: 0xFF81C784,
            BadgeRotation: 0.08),
        new CartItemRecord(
            Id: Products[1].Id,
            Name: Products[1].Name,
            UnitPrice: 389m,
            Quantity: 2,
            Status: "Awaiting courier pickup",
            Details: "Memory lumbar · Mesh weave",
            AccentColor: 0xFF64B5F6,
            BadgeRotation: 0.14),
        new CartItemRecord(
            Id: Products[3].Id,
            Name: Products[3].Name,
            UnitPrice: 1199m,
            Quantity: 1,
            Status: "PID precision preheated",
            Details: "Double boiler · Matte steel",
            AccentColor: 0xFFFFB74D,
            BadgeRotation: 0.05),
    };

    private static readonly List<InterestRecord> InterestLog = new();

    public static string CreateCatalogJson()
    {
        var builder = new DynamicContentBuilder();
        foreach (var entry in BuildDataPayload())
        {
            builder.Set(entry.Key, entry.Value);
        }
        return builder.ToJsonString();
    }

    private static Dictionary<string, object> BuildCartPayload()
    {
        var total = CartItems.Sum(item => item.UnitPrice * item.Quantity);
        var savings = total * 0.05m;

        return new Dictionary<string, object>
        {
            ["items"] = CartItems.Select(item => new Dictionary<string, object>
            {
                ["id"] = item.Id,
                ["name"] = item.Name,
                ["status"] = item.Status,
                ["details"] = item.Details,
                ["accentColor"] = item.AccentColor,
                ["badgeRotation"] = item.BadgeRotation,
                ["quantityText"] = item.QuantityText,
                ["lineTotalText"] = item.LineTotalText,
            }).ToArray(),
            ["totalText"] = FormatCurrency(total),
            ["savingsText"] = $"{FormatCurrency(savings)} saved",
            ["statusText"] = "Packed & ready for dispatch",
            ["promoText"] = "Free delivery on orders over $1,500",
        };
    }

    private static Dictionary<string, object> BuildCatalogDictionary()
    {
        return new Dictionary<string, object>
        {
            ["items"] = Products.Select(product => new Dictionary<string, object>
            {
                ["id"] = product.Id,
                ["name"] = product.Name,
                ["priceText"] = product.PriceText,
                ["ratingText"] = product.RatingText,
                ["category"] = product.Category,
            }).ToArray(),
        };
    }

    public static Dictionary<string, object> BuildDataPayload()
    {
        return new Dictionary<string, object>
        {
            ["catalog"] = BuildCatalogDictionary(),
            ["cart"] = BuildCartPayload(),
        };
    }

    public static ProductRecord AddProduct(
        string name,
        string category,
        string priceText,
        string ratingText,
        string description,
        IReadOnlyList<string> highlights,
        IReadOnlyList<Specification> specifications)
    {
        var nextId = Products.Count == 0 ? 1 : Products.Max(product => product.Id) + 1;
        var record = new ProductRecord(
            Id: nextId,
            Name: name,
            Category: category,
            PriceText: priceText,
            RatingText: ratingText,
            Highlights: highlights,
            Description: description,
            Specifications: specifications);
        Products.Add(record);
        return record;
    }

    public static bool AppendCategoryTag(int id, string tag)
    {
        var index = Products.FindIndex(product => product.Id == id);
        if (index < 0)
        {
            return false;
        }

        var normalizedTag = tag.Trim();
        if (normalizedTag.Length == 0)
        {
            return true;
        }

        var product = Products[index];
        var trimmedCategory = product.Category.Trim();
        var alreadyHasTag = trimmedCategory.Contains(normalizedTag, StringComparison.OrdinalIgnoreCase);
        if (alreadyHasTag)
        {
            return true;
        }

        var updatedCategory = $"{trimmedCategory} · {normalizedTag}";
        Products[index] = product with { Category = updatedCategory };
        return true;
    }

    public static bool RemoveProduct(int id)
    {
        var index = Products.FindIndex(product => product.Id == id);
        if (index < 0)
        {
            return false;
        }

        Products.RemoveAt(index);
        return true;
    }

    public static InterestResult RecordInterest(int? productId, string notifier, string note)
    {
        var productName = productId.HasValue
            ? Products.FirstOrDefault(product => product.Id == productId.Value)?.Name ?? $"#{productId}"
            : "Catalog";
        var record = new InterestRecord(productId, productName, notifier, note, DateTimeOffset.UtcNow);
        InterestLog.Add(record);
        return new InterestResult(
            Message: $"{record.ProductName} interest logged by {record.Notifier}: {record.Note}",
            TotalInterests: InterestLog.Count);
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

    private static string FormatCurrency(decimal amount) => $"${amount:N0}";

    public sealed record ProductRecord(
        int Id,
        string Name,
        string Category,
        string PriceText,
        string RatingText,
        IReadOnlyList<string> Highlights,
        string Description,
        IReadOnlyList<Specification> Specifications);

    public sealed record Specification(string Label, string Value);

    private sealed record CartItemRecord(
        int Id,
        string Name,
        decimal UnitPrice,
        int Quantity,
        string Status,
        string Details,
        uint AccentColor,
        double BadgeRotation)
    {
        public string QuantityText => Quantity == 1 ? "1 unit" : $"{Quantity} units";

        public string LineTotalText => FormatCurrency(UnitPrice * Quantity);
    }

    public sealed record InterestResult(string Message, int TotalInterests);

    private sealed record InterestRecord(
        int? ProductId,
        string ProductName,
        string Notifier,
        string Note,
        DateTimeOffset Timestamp);
}
