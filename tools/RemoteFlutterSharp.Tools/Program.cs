using RemoteFlutterSharp.RemoteUi;

var options = ParseArguments(args);

var outputDirectory = Path.GetFullPath(options.OutputDirectory ?? Path.Combine(Environment.CurrentDirectory, "artifacts", "remote-ui"));
Directory.CreateDirectory(outputDirectory);

var libraryText = CatalogRemoteUi.CreateLibraryText();
var dataJson = CatalogRemoteUi.CreateDataJson();

var libraryPath = Path.Combine(outputDirectory, "catalog.rfwtxt");
var dataPath = Path.Combine(outputDirectory, "catalog.json");

await File.WriteAllTextAsync(libraryPath, libraryText);
await File.WriteAllTextAsync(dataPath, dataJson);

Console.WriteLine($"✅ Remote UI assets generated in {outputDirectory}");
Console.WriteLine($" - Library: {libraryPath}");
Console.WriteLine($" - Data:    {dataPath}");

static ToolOptions ParseArguments(string[] arguments)
{
	if (arguments.Length == 0)
	{
		return new ToolOptions();
	}

	string? outputPath = null;

	for (var index = 0; index < arguments.Length; index++)
	{
		var current = arguments[index];
		switch (current)
		{
			case "-h" or "--help":
				PrintUsage();
				Environment.Exit(0);
				break;
			case "-o" or "--output":
				if (index + 1 >= arguments.Length)
				{
					throw new ArgumentException("Missing value for --output option.");
				}

				outputPath = arguments[++index];
				break;
			default:
				throw new ArgumentException($"Unknown argument '{current}'. Use --help for usage information.");
		}
	}

	return new ToolOptions
	{
		OutputDirectory = outputPath,
	};
}

static void PrintUsage()
{
	Console.WriteLine("RemoteFlutterSharp.Tools\n");
	Console.WriteLine("Exports Remote Flutter Widget assets for the catalog sample.");
	Console.WriteLine();
	Console.WriteLine("Usage:");
	Console.WriteLine("  dotnet run --project tools/RemoteFlutterSharp.Tools -- [-o|--output] <directory>");
	Console.WriteLine();
	Console.WriteLine("Options:");
	Console.WriteLine("  -o|--output    Target directory for generated files (defaults to ./artifacts/remote-ui)");
	Console.WriteLine("  -h|--help      Show this help message");
}

sealed class ToolOptions
{
	public string? OutputDirectory { get; init; }
}
