namespace RemoteFlutterSharp.Rfw.Expressions;

internal sealed class RfwEventExpression : RfwExpression
{
    private readonly string _eventName;
    private readonly RfwExpression? _arguments;

    public RfwEventExpression(string eventName, RfwExpression? arguments = null)
    {
        _eventName = eventName;
        _arguments = arguments;
    }

    internal override void Write(RfwWriter writer)
    {
        writer.Write("event \"");
        writer.Write(_eventName);
        writer.Write("\"");
        if (_arguments != null)
        {
            writer.Write(" ");
            _arguments.Write(writer);
        }
    }
}
