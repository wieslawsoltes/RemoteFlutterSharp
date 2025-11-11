using System;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace RemoteFlutterSharp.Xaml;

internal static class RfwXamlLanguage
{
    public static (XamlLanguageTypeMappings Language, XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult> Emit) Configure(IXamlTypeSystem typeSystem)
    {
        ArgumentNullException.ThrowIfNull(typeSystem);

        var contentAttribute = typeSystem.GetType("RemoteFlutterSharp.Xaml.Markup.RfwContentAttribute")
            ?? throw new InvalidOperationException("Unable to resolve RfwContentAttribute type.");
        var xmlnsAttribute = typeSystem.GetType("RemoteFlutterSharp.Xaml.Markup.RfwXmlnsDefinitionAttribute")
            ?? throw new InvalidOperationException("Unable to resolve RfwXmlnsDefinitionAttribute type.");
        var whitespaceAttribute = typeSystem.GetType("RemoteFlutterSharp.Xaml.Markup.RfwWhitespaceSignificantCollectionAttribute")
            ?? throw new InvalidOperationException("Unable to resolve RfwWhitespaceSignificantCollectionAttribute type.");
        var trimAttribute = typeSystem.GetType("RemoteFlutterSharp.Xaml.Markup.RfwTrimSurroundingWhitespaceAttribute")
            ?? throw new InvalidOperationException("Unable to resolve RfwTrimSurroundingWhitespaceAttribute type.");

        var language = new XamlLanguageTypeMappings(typeSystem)
        {
            XmlnsAttributes = { xmlnsAttribute },
            ContentAttributes = { contentAttribute },
            WhitespaceSignificantCollectionAttributes = { whitespaceAttribute },
            TrimSurroundingWhitespaceAttributes = { trimAttribute }
        };

        var emit = new XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult>();
        return (language, emit);
    }
}
