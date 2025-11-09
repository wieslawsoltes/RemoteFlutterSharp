namespace RemoteFlutterSharp.Rfw.Expressions;

internal sealed class RfwWidgetExpression : RfwExpression
{
    private readonly string _name;
    private readonly IReadOnlyDictionary<string, RfwExpression> _arguments;

    public RfwWidgetExpression(string name, IReadOnlyDictionary<string, RfwExpression> arguments)
    {
        _name = name;
        _arguments = arguments;
    }

    internal override void Write(RfwWriter writer)
    {
        writer.Write(_name);
        writer.WriteLine("(");
        writer.PushIndent();

        foreach (var kvp in _arguments)
        {
            writer.WriteIndent();
            writer.Write(kvp.Key);
            writer.Write(": ");
            kvp.Value.Write(writer);
            writer.Write(",");
            writer.WriteLine();
        }

        writer.PopIndent();
        writer.Write(")");
    }
}
