namespace RemoteFlutterSharp.Rfw.Expressions;

public abstract class RfwExpression
{
    internal abstract void Write(RfwWriter writer);
}

public abstract class RfwListItem
{
    internal abstract void Write(RfwWriter writer, bool isLast);
}
