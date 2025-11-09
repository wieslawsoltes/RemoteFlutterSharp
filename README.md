# RemoteFlutterSharp

RemoteFlutterSharp is a full-stack proof-of-concept that demonstrates how modern .NET applications can author Remote Flutter Widget (RFW) libraries, serve them dynamically, and light them up inside Flutter hosts. The solution combines a fluent C# DSL, an ASP.NET Core backend, and a desktop-ready Flutter client so teams can prototype remote-driven UI experiences end to end.

## Feature Matrix

| Feature | What it delivers | Key technologies |
| --- | --- | --- |
| Fluent RFW DSL | Declare widget trees, event wiring, and iterables with expressive C# helpers (`Widget`, `List`, `For`, `Event`). | .NET 8, `RemoteFlutterSharp.Rfw` |
| Dynamic content builder | Produce RFW-compliant JSON payloads and emit strongly typed catalog data (including full Unicode coverage). | `DynamicContentBuilder`, System.Text.Json |
| Sample catalog server | Hosts `/api/rfw/library`, `/api/rfw/data`, `/api/rfw/product/{id}`, and `/api/rfw/event`, enabling navigation and event telemetry. | ASP.NET Core minimal APIs |
| Flutter host application | Fetches remote bundles, handles catalog navigation, and surfaces event feedback for macOS (and other supported platforms). | Flutter 3.24+, `package:rfw`, `http` |
| CLI export tooling | Generates standalone `.rfwtxt` + JSON assets for offline packaging or integration testing. | `RemoteFlutterSharp.Tools` |

## Repository Layout

```
RemoteFlutterSharp.sln              # Solution orchestrating libraries, samples, tools, and tests
src/RemoteFlutterSharp/             # Core DSL + builders
samples/RemoteFlutterSharp.RemoteUi # Catalog remote UI definitions and data providers
samples/RemoteFlutterSharp.SampleServer
                                                       # ASP.NET Core backend exposing remote UI endpoints
flutter/remote_flutter_host/         # Flutter desktop/mobile/web host application
tools/RemoteFlutterSharp.Tools/      # CLI tool for exporting remote UI artifacts
docs/                                # Architecture notes, design decisions, roadmap
tests/                               # .NET unit tests
```

## Prerequisites

- **.NET 8 SDK** (`dotnet --version` >= 8.0.0)
- **Flutter SDK** 3.24 or newer with Dart >= 3.3
- **macOS** (for the provided desktop host) with Xcode Command Line Tools installed

> Note: Node, Yarn, or other JavaScript tooling is not required for the current workflow.

## Getting Started

1. **Clone the repository**
    ```bash
    git clone https://github.com/<your-org>/RemoteFlutterSharp.git
    cd RemoteFlutterSharp
    ```
2. **Restore dependencies**
    ```bash
    dotnet restore RemoteFlutterSharp.sln
    cd flutter/remote_flutter_host
    flutter pub get
    cd ../..
    ```
3. **Run the sample server** (terminal 1)
    ```bash
    dotnet run --project samples/RemoteFlutterSharp.SampleServer/RemoteFlutterSharp.SampleServer.csproj
    ```
    The server listens on `http://localhost:5209` by default. Override with `ASPNETCORE_URLS` for custom ports or network interfaces.
4. **Launch the Flutter host** (terminal 2)
    ```bash
    cd flutter/remote_flutter_host
    flutter run -d macos --dart-define=REMOTE_FLUTTER_SERVER=http://localhost:5209
    ```
    Swap the device (`-d`) or the server URL as needed for simulators, emulators, or remote hosts.

## Build Instructions

| Target | Command |
| --- | --- |
| Build all .NET projects | `dotnet build RemoteFlutterSharp.sln` |
| Build sample server only | `dotnet build samples/RemoteFlutterSharp.SampleServer/RemoteFlutterSharp.SampleServer.csproj` |
| Build Flutter host (debug) | `flutter build macos` (desktop), `flutter build apk`, `flutter build ios`, etc. |

Artifacts are emitted under the respective `bin/Debug` or Flutter `build/` directories.

## Local Testing & Quality Gates

- **.NET unit tests**
   ```bash
   dotnet test RemoteFlutterSharp.sln
   ```
- **Flutter widget/unit tests**
   ```bash
   cd flutter/remote_flutter_host
   flutter test
   ```
- **Endpoint checks** (after the server starts)
   ```bash
   curl http://localhost:5209/api/rfw/library
   curl http://localhost:5209/api/rfw/data | jq '.'
   curl http://localhost:5209/api/rfw/product/1 | jq '.'
   ```
- **CLI export smoke test**
   ```bash
   dotnet run --project tools/RemoteFlutterSharp.Tools/RemoteFlutterSharp.Tools.csproj -- --output artifacts/remote-ui
   ls artifacts/remote-ui
   ```

## Customising the Remote UI

- Update `samples/RemoteFlutterSharp.RemoteUi/CatalogRemoteUi.cs` to compose new widget trees, events, or navigation flows.
- Extend `CatalogData.cs` with additional catalog items, localized strings, or richer metadata.
- Regenerate exported artifacts via the CLI tool for integration into other environments.

## Continuous Integration

GitHub Actions workflows under `.github/workflows/` build, test, package, and publish artifacts for every push and pull request. Extend the matrix or integrate additional deployment steps as your release strategy evolves.

## Troubleshooting

- **Flutter command missing** – Ensure `flutter/bin` is on your `PATH`; verify with `flutter --version`.
- **macOS sandbox networking** – The provided entitlements enable HTTP client/server access. If you customise bundle identifiers, sync entitlements accordingly.
- **Port conflicts** – Override `ASPNETCORE_URLS` for the server and update the Flutter `--dart-define` value.

## Further Reading

- `docs/architecture.md` – Solution overview and component responsibilities
- `docs/implementation-plan.md` – Backlog of enhancements and milestones
- `samples/RemoteFlutterSharp.RemoteUi/` – Live examples of authoring RFW widgets in C#
