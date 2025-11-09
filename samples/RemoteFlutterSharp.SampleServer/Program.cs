using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RemoteFlutterSharp.RemoteUi;

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

var library = CatalogRemoteUi.CreateLibrary();
var libraryText = library.ToText();
var dataJson = CatalogRemoteUi.CreateDataJson();

builder.Services.AddSingleton(library);
builder.Services.AddSingleton(new RemoteWidgetDocument(libraryText, dataJson));

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Redirect("/api/rfw/library"));

app.MapGet("/api/rfw/library", ([FromServices] RemoteWidgetDocument document) =>
    Results.Text(document.LibraryText, "text/plain"));

app.MapGet("/api/rfw/data", ([FromServices] RemoteWidgetDocument document) =>
    Results.Text(document.DataJson, "application/json"));

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

    return payload.Name switch
    {
        "catalog.select" => HandleSelect(),
        "catalog.back" => Results.Ok(new { status = "catalog" }),
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
});

app.Run();

public sealed record RemoteWidgetDocument(string LibraryText, string DataJson);

public sealed record RemoteUiEvent(string Name, Dictionary<string, JsonElement> Arguments);
