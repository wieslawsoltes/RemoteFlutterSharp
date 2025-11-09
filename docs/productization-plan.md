# Productization Plan

## Objectives
- Harden the RemoteFlutterSharp core library for production scenarios.
- Ensure sample experiences remain functional and easy to operate.
- Provide tooling and guidance that shortens the integration feedback loop.

## Scope
This plan targets immediate work required to move the proof of concept toward a production-ready baseline while keeping the sample assets runnable.

## Work Items

### 1. Quality Gate – Automated Tests
- [x] Add `RemoteFlutterSharp.Tests` (xUnit) covering:
  - [x] `RemoteWidgetLibraryBuilder` serialization for key widgets.
  - [x] `DynamicContentBuilder` normalization rules (scalars, maps, lists, invalid inputs).

### 2. Tooling – Remote UI Exporter
- [x] Create a console project `RemoteFlutterSharp.Tools` that references the core library and outputs the catalog `.rfwtxt` and data `.json` files on demand.
  - [x] Add CLI arguments for output directory.
  - [x] Document usage in the repository README.

### 3. Samples – Runbook Improvements
- [x] Update sample documentation (`README.md`) with:
  - [x] Verified server port and `--dart-define` guidance.
  - [x] Troubleshooting steps for missing Flutter SDK.
  - [x] Combined workflow script references (server + Flutter).

## Out of Scope (Future Considerations)
- Binary `.rfw`/data blob encoding.
- Authentication and authorization for the sample API.
- Real-time update channels (SignalR/WebSockets).
- Production deployment pipelines.
