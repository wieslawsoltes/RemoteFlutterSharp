using System;

[assembly: RemoteFlutterSharp.Xaml.Markup.RfwXmlnsDefinition("http://schemas.remotefluttersharp.dev/xaml", "RemoteFlutterSharp.Xaml.Markup")]

namespace RemoteFlutterSharp.Xaml.Markup;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class RfwContentAttribute : Attribute
{
    public RfwContentAttribute()
    {
    }

    public RfwContentAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RfwWhitespaceSignificantCollectionAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RfwTrimSurroundingWhitespaceAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class RfwXmlnsDefinitionAttribute : Attribute
{
    public RfwXmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
    {
        XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
        ClrNamespace = clrNamespace ?? throw new ArgumentNullException(nameof(clrNamespace));
    }

    public string XmlNamespace { get; }

    public string ClrNamespace { get; }
}
