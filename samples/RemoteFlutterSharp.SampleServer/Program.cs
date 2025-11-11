using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RemoteFlutterSharp.RemoteUi;
using RemoteFlutterSharp.RemoteUi.Xaml;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials());
});

var csharpLibrary = CatalogRemoteUi.CreateLibrary();
var xamlLibrary = CatalogRemoteUiXaml.CreateLibrary();

var catalogDocuments = new RemoteWidgetCatalog(
    new RemoteWidgetDocument(csharpLibrary.ToText(), CatalogRemoteUi.CreateDataJson()),
    new RemoteWidgetDocument(xamlLibrary.ToText(), CatalogRemoteUiXaml.CreateDataJson()));

builder.Services.AddSingleton(catalogDocuments);

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Redirect("/api/rfw/library"));

app.MapGet("/api/rfw/library", (HttpContext context, [FromServices] RemoteWidgetCatalog catalog) =>
{
    var document = ResolveDocument(context, catalog);
    return Results.Text(document.LibraryText, "text/plain");
});

app.MapGet("/api/rfw/data", (HttpContext context, [FromServices] RemoteWidgetCatalog catalog) =>
{
    var document = ResolveDocument(context, catalog);
    return Results.Text(document.DataJson, "application/json");
});

app.MapGet("/api/rfw/product/{id:int}", ([FromRoute] int id) =>
    CatalogData.TryGetProductDetail(id, out var detail)
        ? Results.Json(detail)
        : Results.NotFound());

app.MapPost("/api/rfw/event", ([FromBody] RemoteUiEvent payload, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(payload.Name))
    {
        return Results.BadRequest("Event name is required.");
    }

    logger.LogInformation("Received remote event {EventName} with payload {Payload}", payload.Name, JsonSerializer.Serialize(payload.Arguments));

    int ResolveProductId()
    {
        if (!payload.Arguments.TryGetValue("id", out var element))
        {
            logger.LogWarning("Remote event payload missing id argument.");
            return -1;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var value))
        {
            return value;
        }

        if (element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out var parsed))
        {
            return parsed;
        }

        logger.LogWarning("Remote event id argument was not an integer: {ValueKind}", element.ValueKind);
        return -1;
    }

    string ResolveProductName()
    {
        if (payload.Arguments.TryGetValue("name", out var element) && element.ValueKind == JsonValueKind.String)
        {
            return element.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    string ResolveStringValue(string key, string fallback)
    {
        if (payload.Arguments.TryGetValue(key, out var element) && element.ValueKind == JsonValueKind.String)
        {
            return element.GetString() ?? fallback;
        }

        return fallback;
    }

    return payload.Name switch
    {
        "catalog.select" => HandleSelect(),
        "catalog.back" => Results.Ok(new { status = "catalog" }),
        "catalog.manage" => Results.Ok(new { status = "manager" }),
        "catalog.interest" => HandleInterest(),
        "catalog.create" => HandleCreate(),
        "catalog.update" => HandleUpdate(),
        "catalog.delete" => HandleDelete(),
        "catalog.buy" => Results.Ok(new { status = "added", product = ResolveProductName() }),
        _ => Results.Accepted()
    };

    IResult HandleSelect()
    {
        var id = ResolveProductId();
        if (id <= 0)
        {
            return Results.BadRequest(new { error = "Invalid product identifier." });
        }

        return CatalogData.TryGetProductDetail(id, out var detail)
            ? Results.Ok(detail)
            : Results.NotFound(new { error = "Product not found." });
    }

    IResult HandleInterest()
    {
        var productId = ResolveProductId();
        var notifier = ResolveStringValue("notifier", "Remote UI");
        var message = ResolveStringValue("message", "Interest logged from the Flutter host.");
        var interest = CatalogData.RecordInterest(productId > 0 ? productId : null, notifier, message);
        logger.LogInformation("Interest recorded: {InterestMessage}", interest.Message);
        return Results.Ok(new { message = interest.Message, total = interest.TotalInterests });
    }

    IResult HandleCreate()
    {
        var name = ResolveStringValue("name", "Modern Reflection");
        var category = ResolveStringValue("category", "Studio");
        var priceText = ResolveStringValue("priceText", "$549");
        var ratingText = ResolveStringValue("ratingText", "4.7");
        var description = ResolveStringValue("description", "New inspired piece added from the Flutter host.");
        var highlights = new[]
        {
            ResolveStringValue("highlight1", "Responsive craftsmanship"),
            ResolveStringValue("highlight2", "Harmonized palette"),
            "Limited drop"
        };
        var specifications = new[]
        {
            new CatalogData.Specification("Added", DateTimeOffset.UtcNow.ToString("HH:mm", CultureInfo.InvariantCulture))
        };
        var record = CatalogData.AddProduct(
            name,
            category,
            priceText,
            ratingText,
            description,
            highlights,
            specifications);
        var payload = CatalogData.BuildDataPayload();
        return Results.Ok(new { message = $"Created {record.Name}.", data = payload });
    }

    IResult HandleUpdate()
    {
        var productId = ResolveProductId();
        if (productId <= 0)
        {
            return Results.BadRequest(new { error = "Missing product identifier." });
        }

        var tag = ResolveStringValue("tag", "refreshed");
        if (!CatalogData.AppendCategoryTag(productId, tag))
        {
            return Results.NotFound(new { error = "Product not available." });
        }

        var payload = CatalogData.BuildDataPayload();
        return Results.Ok(new { message = $"Updated category for #{productId}.", data = payload });
    }

    IResult HandleDelete()
    {
        var productId = ResolveProductId();
        if (productId <= 0)
        {
            return Results.BadRequest(new { error = "Missing product identifier." });
        }

        if (!CatalogData.RemoveProduct(productId))
        {
            return Results.NotFound(new { error = "Product not available." });
        }

        var payload = CatalogData.BuildDataPayload();
        return Results.Ok(new { message = $"Deleted #{productId}.", data = payload });
    }
});

app.Run();

static RemoteWidgetDocument ResolveDocument(HttpContext context, RemoteWidgetCatalog catalog)
{
    var variant = context.Request.Query["variant"].ToString();

    if (string.Equals(variant, "csharp", StringComparison.OrdinalIgnoreCase))
    {
        return catalog.CSharp;
    }

    return catalog.Xaml;
}

public sealed record RemoteWidgetDocument(string LibraryText, string DataJson);

public sealed record RemoteWidgetCatalog(RemoteWidgetDocument CSharp, RemoteWidgetDocument Xaml);

public sealed record RemoteUiEvent(string Name, Dictionary<string, JsonElement> Arguments);
