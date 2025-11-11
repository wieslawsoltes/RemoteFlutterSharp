using System;
using System.Collections.Generic;
using RemoteFlutterSharp.Rfw;

namespace RemoteFlutterSharp.Xaml.Markup;

public sealed class Library
{
    public string Name { get; set; } = string.Empty;

    [RfwContent]
    public List<ILibraryEntry> Entries { get; } = new();

    public RemoteWidgetLibrary Build()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Library Name is required.");
        }

        var builder = new RemoteWidgetLibraryBuilder(Name);
        foreach (var entry in Entries)
        {
            entry.Apply(builder);
        }

        return builder.Build();
    }
}

public interface ILibraryEntry
{
    void Apply(RemoteWidgetLibraryBuilder builder);
}

public sealed class Import : ILibraryEntry
{
    public string Name { get; set; } = string.Empty;

    public void Apply(RemoteWidgetLibraryBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Import requires a Name value.");
        }

        builder.AddImport(Name);
    }
}

public sealed class WidgetDefinition : ILibraryEntry
{
    public string Name { get; set; } = string.Empty;

    public string? Summary { get; set; }

    [RfwContent]
    public IExpressionNode? Body { get; set; }

    public void Apply(RemoteWidgetLibraryBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Widget Name is required.");
        }

        if (Body is null)
        {
            throw new InvalidOperationException($"Widget '{Name}' requires a body expression.");
        }

        builder.DefineWidget(Name, Body.BuildExpression(), Summary);
    }
}
