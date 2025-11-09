namespace RemoteFlutterSharp.Rfw.Expressions;

internal sealed class RfwListExpression : RfwExpression
{
    private readonly IReadOnlyList<RfwListItem> _items;

    public RfwListExpression(IEnumerable<RfwListItem> items)
    {
        _items = items.ToArray();
    }

    internal override void Write(RfwWriter writer)
    {
        if (_items.Count == 0)
        {
            writer.Write("[]");
            return;
        }

        writer.WriteLine("[");
        writer.PushIndent();

        for (var index = 0; index < _items.Count; index++)
        {
            var isLast = index == _items.Count - 1;
            _items[index].Write(writer, isLast);
        }

        writer.PopIndent();
        writer.Write("]");
    }
}

internal sealed class RfwExpressionListItem : RfwListItem
{
    private readonly RfwExpression _expression;

    public RfwExpressionListItem(RfwExpression expression)
    {
        _expression = expression;
    }

    internal override void Write(RfwWriter writer, bool isLast)
    {
        writer.WriteIndent();
        _expression.Write(writer);
        writer.Write(",");
        writer.WriteLine();
    }
}

internal sealed class RfwForListItem : RfwListItem
{
    private readonly string _variable;
    private readonly RfwExpression _iterable;
    private readonly RfwExpression _body;

    public RfwForListItem(string variable, RfwExpression iterable, RfwExpression body)
    {
        _variable = variable;
        _iterable = iterable;
        _body = body;
    }

    internal override void Write(RfwWriter writer, bool isLast)
    {
        writer.WriteIndent();
        writer.Write("...for ");
        writer.Write(_variable);
        writer.Write(" in ");
        _iterable.Write(writer);
        writer.WriteLine(":");
        writer.PushIndent();
        writer.WriteIndent();
        _body.Write(writer);
        writer.Write(",");
        writer.WriteLine();
        writer.PopIndent();
    }
}
