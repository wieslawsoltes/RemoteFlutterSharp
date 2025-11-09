using System;
using System.Collections.Generic;
using RemoteFlutterSharp.Rfw.Expressions;

namespace RemoteFlutterSharp.Rfw;

public sealed class RfwArgumentBuilder
{
    private readonly Dictionary<string, RfwExpression> _values = new(StringComparer.Ordinal);

    public RfwArgumentBuilder Argument(string name, RfwValue value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Argument name is required.", nameof(name));
        }

        _values[name] = value.Expression;
        return this;
    }

    public RfwArgumentBuilder Argument(RfwArgument argument) => Argument(argument.Name, argument.Value);

    public RfwArgumentBuilder AddRange(IEnumerable<RfwArgument> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        foreach (var argument in arguments)
        {
            Argument(argument);
        }

        return this;
    }

    public RfwArgumentBuilder Child(RfwValue child) => Argument("child", child);

    public RfwArgumentBuilder Children(params RfwValue[] children) => Argument("children", RfwDsl.List(children));

    public RfwArgumentBuilder Padding(params RfwValue[] edges) => Argument("padding", RfwDsl.List(edges));

    internal IReadOnlyDictionary<string, RfwExpression> Build() =>
        new Dictionary<string, RfwExpression>(_values, StringComparer.Ordinal);
}
