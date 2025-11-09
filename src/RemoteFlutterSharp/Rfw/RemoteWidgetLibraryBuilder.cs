using RemoteFlutterSharp.Rfw.Expressions;

namespace RemoteFlutterSharp.Rfw;

public sealed class RemoteWidgetLibraryBuilder
{
    private readonly List<string> _imports = new();
    private readonly List<RemoteWidgetDefinition> _widgets = new();

    public RemoteWidgetLibraryBuilder(string libraryName)
    {
        if (string.IsNullOrWhiteSpace(libraryName))
        {
            throw new ArgumentException("Library name is required.", nameof(libraryName));
        }

        LibraryName = libraryName;
    }

    public string LibraryName { get; }

    public RemoteWidgetLibraryBuilder AddImport(string library)
    {
        if (string.IsNullOrWhiteSpace(library))
        {
            throw new ArgumentException("Import name is required.", nameof(library));
        }

        if (!_imports.Contains(library, StringComparer.Ordinal))
        {
            _imports.Add(library);
        }

        return this;
    }

    public RemoteWidgetLibraryBuilder DefineWidget(string name, RfwExpression body, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Widget name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(body);

        _widgets.Add(new RemoteWidgetDefinition(name, null, body, description));
        return this;
    }

    public RemoteWidgetLibraryBuilder DefineWidgetWithState(string name, IReadOnlyDictionary<string, RfwExpression> initialState, RfwExpression body, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Widget name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(initialState);
        ArgumentNullException.ThrowIfNull(body);

        _widgets.Add(new RemoteWidgetDefinition(name, initialState, body, description));
        return this;
    }

    public RemoteWidgetLibrary Build()
    {
        if (_widgets.Count == 0)
        {
            throw new InvalidOperationException("At least one widget definition is required.");
        }

        return new RemoteWidgetLibrary(LibraryName, _imports.ToArray(), _widgets.ToArray());
    }
}

public sealed record RemoteWidgetDefinition(
    string Name,
    IReadOnlyDictionary<string, RfwExpression>? InitialState,
    RfwExpression Body,
    string? Description);

public sealed class RemoteWidgetLibrary
{
    public RemoteWidgetLibrary(string libraryName, IReadOnlyList<string> imports, IReadOnlyList<RemoteWidgetDefinition> widgets)
    {
        LibraryName = libraryName;
        Imports = imports;
        Widgets = widgets;
    }

    public string LibraryName { get; }

    public IReadOnlyList<string> Imports { get; }

    public IReadOnlyList<RemoteWidgetDefinition> Widgets { get; }

    public string ToText()
    {
        var writer = new RfwWriter();

        foreach (var import in Imports)
        {
            writer.WriteLine($"import {import};");
        }

        if (Imports.Count > 0)
        {
            writer.WriteLine();
        }

        for (var index = 0; index < Widgets.Count; index++)
        {
            var widget = Widgets[index];

            if (!string.IsNullOrWhiteSpace(widget.Description))
            {
                writer.WriteLine($"// {widget.Description}");
            }

            writer.Write("widget ");
            writer.Write(widget.Name);
            if (widget.InitialState is { Count: > 0 })
            {
                writer.Write(" { ");
                var stateIndex = 0;
                foreach (var stateEntry in widget.InitialState)
                {
                    if (stateIndex > 0)
                    {
                        writer.Write(", ");
                    }

                    writer.Write(stateEntry.Key);
                    writer.Write(": ");
                    stateEntry.Value.Write(writer);
                    stateIndex++;
                }

                writer.Write(" }");
            }

            writer.Write(" = ");
            widget.Body.Write(writer);
            writer.WriteLine(";");

            if (index < Widgets.Count - 1)
            {
                writer.WriteLine();
            }
        }

        return writer.ToString();
    }
}
