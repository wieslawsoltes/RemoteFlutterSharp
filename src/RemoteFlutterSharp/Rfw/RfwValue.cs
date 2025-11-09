using System;
using System.Globalization;
using RemoteFlutterSharp.Rfw.Expressions;

namespace RemoteFlutterSharp.Rfw;

public readonly struct RfwValue
{
    private readonly RfwExpression? _expression;

    internal RfwExpression Expression => _expression ?? throw new InvalidOperationException("RfwValue requires a valid expression.");

    public RfwValue(RfwExpression expression)
    {
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public static implicit operator RfwValue(string value) =>
        new(new RfwStringLiteralExpression(value ?? throw new ArgumentNullException(nameof(value))));

    public static implicit operator RfwValue(bool value) =>
        new(new RfwLiteralExpression(value ? "true" : "false"));

    public static implicit operator RfwValue(int value) =>
        new(new RfwLiteralExpression(value.ToString(CultureInfo.InvariantCulture)));

    public static implicit operator RfwValue(long value) =>
        new(new RfwLiteralExpression(value.ToString(CultureInfo.InvariantCulture)));

    public static implicit operator RfwValue(float value) =>
        new(new RfwLiteralExpression(value.ToString(CultureInfo.InvariantCulture)));

    public static implicit operator RfwValue(double value) =>
        new(new RfwLiteralExpression(value.ToString(CultureInfo.InvariantCulture)));

    public static implicit operator RfwValue(RfwExpression expression) => new(expression);

    public static implicit operator RfwExpression(RfwValue value) => value.Expression;
}
