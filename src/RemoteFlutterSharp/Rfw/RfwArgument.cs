using System;

namespace RemoteFlutterSharp.Rfw;

public readonly struct RfwArgument
{
    public string Name { get; }
    public RfwValue Value { get; }

    public RfwArgument(string name, RfwValue value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Argument name is required.", nameof(name));
        }

        Name = name;
        Value = value;
    }

    public static implicit operator RfwArgument((string Name, RfwValue Value) entry) => new(entry.Name, entry.Value);
}
