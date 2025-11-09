namespace RemoteFlutterSharp.Rfw.Expressions;

internal sealed class RfwLiteralExpression : RfwExpression
{
    private readonly string _literal;

    public RfwLiteralExpression(string literal)
    {
        _literal = literal;
    }

    internal override void Write(RfwWriter writer)
    {
        writer.Write(_literal);
    }
}
