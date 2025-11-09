using System.Text;

namespace RemoteFlutterSharp.Rfw.Expressions;

internal sealed class RfwReferenceExpression : RfwExpression
{
    private readonly IReadOnlyList<string> _segments;

    public RfwReferenceExpression(IEnumerable<string> segments)
    {
        _segments = segments.ToArray();
        if (_segments.Count == 0)
        {
            throw new ArgumentException("Reference requires at least one segment.", nameof(segments));
        }
    }

    internal override void Write(RfwWriter writer)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < _segments.Count; i++)
        {
            if (i > 0)
            {
                builder.Append('.');
            }

            builder.Append(_segments[i]);
        }

        writer.Write(builder.ToString());
    }
}
