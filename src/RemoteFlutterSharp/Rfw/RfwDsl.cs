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

    public static RfwExpression Widget(string name, params (string Argument, RfwExpression Value)[] arguments)
    {
        var dict = new Dictionary<string, RfwExpression>(arguments.Length, StringComparer.Ordinal);
        foreach (var (argument, value) in arguments)
        {
            dict[argument] = value;
        }

        return new RfwWidgetExpression(name, dict);
    }

    public static RfwExpression Map(params (string Key, RfwExpression Value)[] entries)
    {
        var dict = new Dictionary<string, RfwExpression>(entries.Length, StringComparer.Ordinal);
        foreach (var (key, value) in entries)
        {
            dict[key] = value;
        }

        return new RfwMapExpression(dict);
    }

    public static RfwExpression Event(string eventName, params (string Key, RfwExpression Value)[] arguments)
    {
        var payload = Map(arguments);
        return new RfwEventExpression(eventName, payload);
    }

    public static RfwExpression List(params RfwListItem[] items) => new RfwListExpression(items);

    public static RfwListItem Item(RfwExpression expression) => new RfwExpressionListItem(expression);

    public static RfwListItem For(string variable, RfwExpression iterable, RfwExpression body) => new RfwForListItem(variable, iterable, body);
}
