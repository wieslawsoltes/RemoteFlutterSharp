# RemoteFlutterSharp

RemoteFlutterSharp is a full-stack proof-of-concept that demonstrates how modern .NET applications can author Remote Flutter Widget (RFW) libraries, serve them dynamically, and light them up inside Flutter hosts. The solution combines a fluent C# DSL, an ASP.NET Core backend, and a desktop-ready Flutter client so teams can prototype remote-driven UI experiences end to end.

## Resources

- Flutter SDK & documentation: https://flutter.dev
- Remote Flutter Widgets (`rfw` package): https://pub.dev/packages/rfw
- .NET 8 SDK guidance: https://learn.microsoft.com/dotnet/core/whats-new/dotnet-8
- RemoteFlutterSharp architecture notes: `docs/architecture.md`
- Feature/implementation roadmaps: `docs/implementation-plan.md`, `docs/productization-plan.md`

## Feature Matrix

| Feature | What it delivers | Key technologies |
| --- | --- | --- |
| Fluent RFW DSL | Declare widget trees, event wiring, and iterables with expressive C# helpers (`Widget`, `List`, `For`, `Event`). | .NET 8, `RemoteFlutterSharp.Rfw` |
| XAML authoring support | Build the same remote widget libraries from declarative XAML parsed via XamlX at runtime. | `RemoteFlutterSharp.Xaml`, XamlX |
| Dynamic content builder | Produce RFW-compliant JSON payloads and emit strongly typed catalog data (including cart payloads, highlights, and specifications). | `DynamicContentBuilder`, System.Text.Json |
| Sample catalog server | Hosts `/api/rfw/library`, `/api/rfw/data`, `/api/rfw/product/{id}`, `/api/rfw/event`, and the enriched cart payloads so the Flutter host can navigate and react to events. | ASP.NET Core minimal APIs |
| Flutter host application | Fetches remote bundles, handles catalog and cart navigation, animates transitions, and surfaces event feedback for macOS (and other supported platforms). | Flutter 3.24+, `package:rfw`, `http`, `AnimatedSwitcher` |
| Remote CRUD experience | ProductManagerScreen fires `catalog.create/update/delete` events so server-side C# mutations and response payloads keep the UI in sync with real app logic. | `CatalogData`, `samples/RemoteFlutterSharp.SampleServer/Program.cs`, `flutter/remote_flutter_host/lib/main.dart` |
| CLI export tooling | Generates standalone `.rfwtxt` + JSON assets for offline packaging or integration testing. | `RemoteFlutterSharp.Tools` |

## Feature Breakdown

| Area | Highlights | Location |
| --- | --- | --- |
| Remote DSL & library builder | `RemoteWidgetLibraryBuilder` + `RfwDsl` produce text-based libraries with nested widgets, events, loops, and the ProductManager screen/rows. | `src/RemoteFlutterSharp/Rfw` + `samples/RemoteFlutterSharp.RemoteUi/CatalogRemoteUi.cs` |
| Dynamic data surface | Weathered catalog entries plus animated cart metadata (totals, savings, badge rotations) are generated via `CatalogData` and exposed through `/api/rfw/data`. | `samples/RemoteFlutterSharp.RemoteUi/CatalogData.cs` |
| ASP.NET Core hosting | Minimal APIs render the latest library/data, serve product details, and relay events for telemetry/checkout while executing server-side C# logic (e.g., interest logging and CRUD mutations like `catalog.create/update/delete`). | `samples/RemoteFlutterSharp.SampleServer/Program.cs` |
| Flutter runtime | `Runtime`, `RemoteWidget`, and `AnimatedSwitcher` hydrate remote widgets, handle transitions, and surface event-driven snack bars. | `flutter/remote_flutter_host/lib/main.dart` |
| Tooling & automation | CLI export tool, NuGet + Flutter dependency manifests, and unit/widget tests enable repeatable builds and verification. | `tools/RemoteFlutterSharp.Tools`, `tests/`, `flutter/remote_flutter_host/pubspec.yaml` |

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

## Architecture

### High level

- **Library generation:** C# DSL (e.g., `Widget`, `Event`, `For`) builds `RemoteWidgetLibrary` instances that describe the catalog, cart, and related navigation trees; these are serialized to `.rfwtxt` format.
- **Server delivery:** ASP.NET Core minimal APIs host the library and dynamic JSON data plus product-detail REST endpoints, keeping Flutter in sync through `/api/rfw/library`, `/api/rfw/data`, and `/api/rfw/event`.
- **Flutter consumption:** The host fetches the library/data payloads, registers the remote library with the `Runtime`, and renders `RemoteWidget` subtrees inside `AnimatedSwitcher` to smooth transitions between catalog, detail, and cart screens.

### Low level

- **DSL & writer:** `RfwDsl` helpers compose `RfwExpression`s; `RemoteWidgetLibraryBuilder` prints them via `RfwWriter` to produce the text format parsed by the Flutter `rfw` runtime.
- **Dynamic content:** `CatalogData` (and `DynamicContentBuilder`) keep catalog items and derived cart totals in `DynamicMap` structures that can be updated via the shared `DynamicContent` instance before rendering.
- **Runtime wiring:** The Flutter `Runtime` resolves constructor calls, lazy-caches widget builders, tracks dependencies, and invokes event callbacks that post back to the ASP.NET Core server for actions such as `catalog.select`, `catalog.buy`, and `catalog.cart`.
- **Tooling:** `RemoteFlutterSharp.Tools` exports generated `.rfwtxt`/`.json` assets for other clients while `.github/workflows/` validate builds/tests across the solution.

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
- Author remote layouts in XAML under `samples/RemoteFlutterSharp.RemoteUi.Xaml/`; this variant now serves by default, while `/api/rfw/library?variant=csharp` returns the original DSL version.
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
