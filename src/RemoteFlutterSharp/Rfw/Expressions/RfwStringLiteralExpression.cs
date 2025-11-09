using System.Text;

namespace RemoteFlutterSharp.Rfw.Expressions;

internal sealed class RfwStringLiteralExpression : RfwExpression
{
    private readonly string _value;

    public RfwStringLiteralExpression(string value)
    {
        _value = value;
    }

    internal override void Write(RfwWriter writer)
    {
        writer.Write('"' + Escape(_value) + '"');
    }

    private static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length + 4);
        foreach (var c in value)
        {
            builder.Append(c switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ => c,
            });
        }

        return builder.ToString();
    }
}
