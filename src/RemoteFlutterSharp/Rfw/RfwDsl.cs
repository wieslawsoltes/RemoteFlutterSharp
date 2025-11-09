using System;
using System.Collections.Generic;
using System.Globalization;
using RemoteFlutterSharp.Rfw.Expressions;

namespace RemoteFlutterSharp.Rfw;

public static class RfwDsl
{
    public static RfwExpression Literal(string raw) => new RfwLiteralExpression(raw);

    public static RfwExpression Bool(bool value) => new RfwLiteralExpression(value ? "true" : "false");

    public static RfwExpression Int(int value) => new RfwLiteralExpression(value.ToString(CultureInfo.InvariantCulture));

    public static RfwExpression Double(double value) => new RfwLiteralExpression(value.ToString(CultureInfo.InvariantCulture));

    public static RfwExpression HexColor(uint value) => new RfwLiteralExpression($"0x{value:X8}");

    public static RfwExpression String(string value) => new RfwStringLiteralExpression(value);

    public static RfwExpression Reference(params string[] segments) => new RfwReferenceExpression(segments);

    public static RfwExpression Widget(string name, params RfwArgument[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var dict = new Dictionary<string, RfwExpression>(arguments.Length, StringComparer.Ordinal);
        foreach (var argument in arguments)
        {
            dict[argument.Name] = argument.Value.Expression;
        }

        return new RfwWidgetExpression(name, dict);
    }

    public static RfwExpression Widget(string name, Action<RfwArgumentBuilder>? configure)
    {
        var builder = new RfwArgumentBuilder();
        configure?.Invoke(builder);
        return new RfwWidgetExpression(name, builder.Build());
    }

    public static RfwExpression Map(params RfwArgument[] entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var dict = new Dictionary<string, RfwExpression>(entries.Length, StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            dict[entry.Name] = entry.Value.Expression;
        }

        return new RfwMapExpression(dict);
    }

    public static RfwExpression Map(Action<RfwArgumentBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new RfwArgumentBuilder();
        configure(builder);
        return new RfwMapExpression(builder.Build());
    }

    public static RfwExpression Event(string eventName, params RfwArgument[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var payload = Map(arguments);
        return new RfwEventExpression(eventName, payload);
    }

    public static RfwExpression Event(string eventName, Action<RfwArgumentBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new RfwArgumentBuilder();
        configure(builder);
        var entries = builder.Build();
        return new RfwEventExpression(eventName, new RfwMapExpression(entries));
    }

    public static RfwExpression List(params RfwListItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new RfwListExpression(items);
    }

    public static RfwExpression List(params RfwValue[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var items = new RfwListItem[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            items[index] = Item(values[index]);
        }

        return new RfwListExpression(items);
    }

    public static RfwListItem Item(RfwExpression expression) => new RfwExpressionListItem(expression);

    public static RfwListItem Item(RfwValue value) => Item(value.Expression);

    public static RfwListItem For(string variable, RfwExpression iterable, RfwExpression body) => new RfwForListItem(variable, iterable, body);
}
