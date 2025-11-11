using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using RemoteFlutterSharp.Rfw;
using RemoteFlutterSharp.Xaml.Markup;
using XamlX;
using XamlX.Ast;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Parsers;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace RemoteFlutterSharp.Xaml;

public static class RfwXamlLoader
{
    private static readonly object s_lock = new();
    private static bool s_initialized;
    private static SreTypeSystem? s_typeSystem;
    private static AssemblyBuilder? s_assemblyBuilder;
    private static ModuleBuilder? s_moduleBuilder;
    private static XamlLanguageTypeMappings? s_languageMappings;
    private static XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult>? s_emitMappings;

    public static RemoteWidgetLibrary LoadLibrary(string xaml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xaml);
        EnsureInitialized();

        var diagnostics = new List<XamlDiagnostic>();
        var handler = new XamlDiagnosticsHandler
        {
            HandleDiagnostic = diagnostic =>
            {
                diagnostics.Add(diagnostic);
                return diagnostic.Severity;
            }
        };

        var configuration = new TransformerConfiguration(
            s_typeSystem!,
            s_typeSystem!.FindAssembly("RemoteFlutterSharp.Xaml"),
            s_languageMappings!,
            XamlXmlnsMappings.Resolve(s_typeSystem!, s_languageMappings!),
            diagnosticsHandler: handler);

        var compiler = new XamlILCompiler(configuration, s_emitMappings!, true)
        {
            EnableIlVerification = false
        };

        var document = XDocumentXamlParser.Parse(xaml);

        compiler.Transform(document);
        diagnostics.ThrowExceptionIfAnyError();

        var runtimeTypeName = "RemoteFlutterSharp_XamlRuntime_" + Guid.NewGuid().ToString("N");
        var runtimeBuilder = CreateTypeBuilder(runtimeTypeName, isPublic: true);
        var contextBuilder = CreateTypeBuilder(runtimeTypeName + "_Context", isPublic: false);

        var contextType = XamlILContextDefinition.GenerateContextClass(
            contextBuilder.XamlTypeBuilder,
            s_typeSystem!,
            s_languageMappings!,
            s_emitMappings!);

        compiler.Compile(
            document,
            runtimeBuilder.XamlTypeBuilder,
            contextType,
            "Populate",
            "Build",
            "XamlNamespaceInfo",
            null,
            null);

        contextBuilder.CreateType();
        runtimeBuilder.CreateType();
        var runtimeType = runtimeBuilder.GetRuntimeType();

        var buildMethod = runtimeType.GetMethod("Build", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("Generated XAML type is missing a Build method.");

        var libraryObject = buildMethod.Invoke(null, new object?[] { null })
            ?? throw new InvalidOperationException("XAML Build method returned null.");

        if (libraryObject is not Library library)
        {
            throw new InvalidOperationException("XAML root element must resolve to a Library instance.");
        }

        return library.Build();
    }

    public static RemoteWidgetLibrary LoadLibrary(Stream stream, bool leaveOpen = false)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: leaveOpen);
        var content = reader.ReadToEnd();
        return LoadLibrary(content);
    }

    private static void EnsureInitialized()
    {
        if (s_initialized)
        {
            return;
        }

        lock (s_lock)
        {
            if (s_initialized)
            {
                return;
            }

            // Ensure parser assembly is loaded.
            _ = typeof(XDocumentXamlParser).Assembly;

            // Prime common system assemblies that XamlX expects to locate.
            _ = typeof(Uri).Assembly;

            s_typeSystem = new SreTypeSystem();

            var assemblyName = new AssemblyName("RemoteFlutterSharp_XamlRuntimeStore");
            s_assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            s_moduleBuilder = s_assemblyBuilder.DefineDynamicModule(assemblyName.Name + ".dll");

            (s_languageMappings, s_emitMappings) = RfwXamlLanguage.Configure(s_typeSystem);

            s_initialized = true;
        }
    }

    private static RuntimeTypeBuilder CreateTypeBuilder(string name, bool isPublic)
    {
        var attributes = TypeAttributes.Class | (isPublic ? TypeAttributes.Public : TypeAttributes.NotPublic);
        var typeBuilder = s_moduleBuilder!.DefineType(name, attributes);
        var xamlTypeBuilder = ((SreTypeSystem)s_typeSystem!).CreateTypeBuilder(typeBuilder);
        return new RuntimeTypeBuilder(typeBuilder, xamlTypeBuilder);
    }

    private sealed class RuntimeTypeBuilder
    {
        private readonly TypeBuilder _typeBuilder;
        private Type? _runtimeType;

        public RuntimeTypeBuilder(TypeBuilder typeBuilder, IXamlTypeBuilder<IXamlILEmitter> xamlTypeBuilder)
        {
            _typeBuilder = typeBuilder;
            XamlTypeBuilder = xamlTypeBuilder;
        }

        public IXamlTypeBuilder<IXamlILEmitter> XamlTypeBuilder { get; }

        public void CreateType()
        {
            XamlTypeBuilder.CreateType();
        }

        public Type GetRuntimeType() => _runtimeType ??= _typeBuilder.CreateTypeInfo()!.AsType();
    }
}
