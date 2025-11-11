using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RemoteFlutterSharp.Rfw;
using RemoteFlutterSharp.Rfw.Expressions;

namespace RemoteFlutterSharp.Xaml.Markup;

public interface IExpressionNode
{
    RfwExpression BuildExpression();
}

public interface IListItemNode
{
    RfwListItem BuildItem();
}

public sealed class Argument
{
    public string Name { get; set; } = string.Empty;

    [RfwContent]
    public IExpressionNode? Value { get; set; }

    public RfwValue BuildValue()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Argument Name is required.");
        }

        if (Value is null)
        {
            throw new InvalidOperationException($"Argument '{Name}' requires a nested expression.");
        }

        return Value.BuildExpression();
    }

    public RfwArgument ToArgument() => new(Name, BuildValue());
}

public sealed class WidgetCall : IExpressionNode
{
    public string Name { get; set; } = string.Empty;

    [RfwContent]
    public List<Argument> Arguments { get; } = new();

    public RfwExpression BuildExpression()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Widget expression requires a Name value.");
        }

        return RfwDsl.Widget(Name, builder =>
        {
            foreach (var argument in Arguments)
            {
                builder.Argument(argument.Name, argument.BuildValue());
            }
        });
    }
}

public sealed class StringLiteral : IExpressionNode
{
    public string Value { get; set; } = string.Empty;

    public RfwExpression BuildExpression() => RfwDsl.String(Value);
}

public sealed class BoolLiteral : IExpressionNode
{
    public bool Value { get; set; }

    public RfwExpression BuildExpression() => RfwDsl.Bool(Value);
}

public sealed class IntLiteral : IExpressionNode
{
    public int Value { get; set; }

    public RfwExpression BuildExpression() => RfwDsl.Int(Value);
}

public sealed class DoubleLiteral : IExpressionNode
{
    public double Value { get; set; }

    public RfwExpression BuildExpression() => RfwDsl.Double(Value);
}

public sealed class HexColorLiteral : IExpressionNode
{
    public string Value { get; set; } = string.Empty;

    public RfwExpression BuildExpression()
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            throw new InvalidOperationException("HexColor requires a Value.");
        }

        var token = Value.Trim();
        if (token.StartsWith("#", StringComparison.Ordinal))
        {
            token = token[1..];
        }

        if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            token = token[2..];
        }

        if (!uint.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new InvalidOperationException($"Unable to parse '{Value}' as a hexadecimal color.");
        }

        return RfwDsl.HexColor(parsed);
    }
}

public sealed class LiteralValue : IExpressionNode
{
    public string Value { get; set; } = string.Empty;

    public RfwExpression BuildExpression() => RfwDsl.Literal(Value);
}

public sealed class Reference : IExpressionNode
{
    public string Path { get; set; } = string.Empty;

    [RfwContent]
    public List<string> Segments { get; } = new();

    public RfwExpression BuildExpression()
    {
        string[] segments;
        if (Segments.Count > 0)
        {
            segments = Segments.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }
        else if (!string.IsNullOrWhiteSpace(Path))
        {
            segments = Path.Split(new[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        else
        {
            throw new InvalidOperationException("Reference requires a Path or Segments.");
        }

        if (segments.Length == 0)
        {
            throw new InvalidOperationException("Reference requires at least one segment.");
        }

        return RfwDsl.Reference(segments);
    }
}

public sealed class Map : IExpressionNode
{
    [RfwContent]
    public List<Argument> Entries { get; } = new();

    public RfwExpression BuildExpression()
    {
        var arguments = Entries.Select(entry => entry.ToArgument()).ToArray();
        return RfwDsl.Map(arguments);
    }
}

public sealed class Event : IExpressionNode
{
    public string Name { get; set; } = string.Empty;

    [RfwContent]
    public List<Argument> Arguments { get; } = new();

    public RfwExpression BuildExpression()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Event expression requires a Name value.");
        }

        return RfwDsl.Event(Name, builder =>
        {
            foreach (var argument in Arguments)
            {
                builder.Argument(argument.Name, argument.BuildValue());
            }
        });
    }
}

public sealed class List : IExpressionNode
{
    [RfwContent]
    public List<IListItemNode> Items { get; } = new();

    public RfwExpression BuildExpression()
    {
        var built = Items.Select(item => item.BuildItem()).ToArray();
        return RfwDsl.List(built);
    }
}

public sealed class Item : IListItemNode
{
    [RfwContent]
    public IExpressionNode? Value { get; set; }

    public RfwListItem BuildItem()
    {
        if (Value is null)
        {
            throw new InvalidOperationException("List item requires a nested expression.");
        }

        return RfwDsl.Item(Value.BuildExpression());
    }
}

public sealed class For : IListItemNode
{
    public string Variable { get; set; } = string.Empty;

    public IExpressionNode? Iterable { get; set; }

    [RfwContent]
    public IExpressionNode? Body { get; set; }

    public RfwListItem BuildItem()
    {
        if (string.IsNullOrWhiteSpace(Variable))
        {
            throw new InvalidOperationException("For loop requires a variable name.");
        }

        if (Iterable is null)
        {
            throw new InvalidOperationException("For loop requires an iterable expression.");
        }

        if (Body is null)
        {
            throw new InvalidOperationException("For loop requires a body expression.");
        }

        return RfwDsl.For(Variable, Iterable.BuildExpression(), Body.BuildExpression());
    }
}
