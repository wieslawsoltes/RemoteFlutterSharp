namespace RemoteFlutterSharp.Rfw.Expressions;

internal sealed class RfwMapExpression : RfwExpression
{
    private readonly IReadOnlyDictionary<string, RfwExpression> _entries;

    public RfwMapExpression(IReadOnlyDictionary<string, RfwExpression> entries)
    {
        _entries = entries;
    }

    internal override void Write(RfwWriter writer)
    {
        if (_entries.Count == 0)
        {
            writer.Write("{}");
            return;
        }

        writer.WriteLine("{");
        writer.PushIndent();

        foreach (var entry in _entries)
        {
            writer.WriteIndent();
            writer.Write(entry.Key);
            writer.Write(": ");
            entry.Value.Write(writer);
            writer.Write(",");
            writer.WriteLine();
        }

        writer.PopIndent();
        writer.Write("}");
    }
}
