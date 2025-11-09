using System.Text;

namespace RemoteFlutterSharp.Rfw;

internal sealed class RfwWriter
{
    private readonly StringBuilder _builder = new();
    private readonly string _indentToken;
    private int _indentLevel;
    private bool _atLineStart = true;

    public RfwWriter(string indentToken = "  ")
    {
        _indentToken = indentToken;
    }

    public void Write(string text)
    {
        if (text.Length == 0)
        {
            return;
        }

        WriteIndentIfNeeded();
        _builder.Append(text);
        _atLineStart = false;
    }

    public void WriteLine()
    {
        _builder.AppendLine();
        _atLineStart = true;
    }

    public void WriteLine(string text)
    {
        if (text.Length == 0)
        {
            WriteLine();
            return;
        }

        Write(text);
        WriteLine();
    }

    public void WriteIndent()
    {
        WriteIndentIfNeeded();
    }

    public void PushIndent() => _indentLevel++;

    public void PopIndent()
    {
        if (_indentLevel == 0)
        {
            throw new InvalidOperationException("Indent level underflow.");
        }

        _indentLevel--;
    }

    public override string ToString() => _builder.ToString();

    private void WriteIndentIfNeeded()
    {
        if (!_atLineStart)
        {
            return;
        }

        for (var i = 0; i < _indentLevel; i++)
        {
            _builder.Append(_indentToken);
        }

        _atLineStart = false;
    }
}
