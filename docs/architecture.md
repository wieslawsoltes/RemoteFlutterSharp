# RemoteFlutterSharp Architecture

## Overview
RemoteFlutterSharp enables .NET applications to author Remote Flutter Widget (RFW) libraries and publish them to Flutter hosts at runtime. The proof of concept couples a high-performance .NET 8 backend with a Flutter host application. The backend compiles declarative UI definitions using a fluent DSL, serves dynamic configuration data, and captures user events emitted from the rendered Flutter experience.

## Components
- **RemoteFlutterSharp (library, `src/RemoteFlutterSharp`)** – Provides a typed DSL for building RFW text libraries and a dynamic data builder that enforces the value constraints required by the RFW runtime.
- **Sample server (`samples/RemoteFlutterSharp.SampleServer`)** – ASP.NET Core minimal API that publishes the compiled RFW library, exposes configuration data, and accepts interaction events for telemetry or business logic.
- **Flutter host (`flutter/remote_flutter_host`)** – Cross-platform Flutter app that loads the remote library and data, renders the UI via `RemoteWidget`, and routes user events back to the .NET backend.
- **Documentation (`docs`)** – Architecture and implementation plan to guide future iterations.

## Data & UI Flow
1. The .NET server boots and calls `CatalogRemoteUi.CreateLibrary()` to produce a `RemoteWidgetLibrary`. The library is serialized to `.rfwtxt` using the DSL writer and cached alongside dynamic content built with `DynamicContentBuilder`.
2. The Flutter host requests `/api/rfw/library` and `/api/rfw/data` during start-up. The library text is parsed with `parseLibraryFile`, and the data payload is transformed into `DynamicMap` entries and registered on the shared `Runtime`/`DynamicContent` instances.
3. `RemoteWidget` composes the runtime, data, remote library name (`catalog`), and renders widget trees that bottom out in `createCoreWidgets`/`createMaterialWidgets`.
4. When a user interacts with the UI, event arguments are propagated to the host. The Flutter app forwards them to `/api/rfw/event`, enabling server-side orchestration (logging, persistence, or data refresh).

## Library Design
- **RFW DSL (`RemoteFlutterSharp.Rfw`)** – Uses composable expression nodes (`RfwExpression`) to describe widgets, literals, events, lists, and loops. Factories in `RfwDsl` hide the serialization details and maintain compatibility with the `rfwtxt` grammar.
- **Dynamic Content Builder (`RemoteFlutterSharp.Dynamic`)** – Normalises nested dictionaries/lists to supported scalar types and serializes them to JSON. This guarantees schema correctness before the Flutter host consumes the payload.
- **Extensibility** – Builders are designed for future additions such as binary `.rfw` encoding, expression validation, or higher-level UI abstractions.

## Sample Server Responsibilities
- Serves static snapshots of the remote library and data for deterministic demos.
- Applies permissive CORS so that Flutter running on emulators or devices can reach `localhost` during development.
- Captures remote events for diagnostics and acts as a rendezvous point for richer back-end workflows.

## Flutter Host Responsibilities
- Initializes the RFW runtime with core/material local libraries and fetches remote content.
- Converts JSON data to `DynamicMap` instances, respecting the no-null constraint of RFW dynamic content.
- Displays remote UI inside a lightweight scaffold, provides manual refresh, and surfaces interaction events (snack bars + HTTP callback).

## Deployment Considerations
- **Scaling** – The RFW documents are small; caching in memory avoids regeneration costs. Future deployments can front the API with CDN or serverless endpoints.
- **Security** – Implement authentication on the API before exposing on shared networks. Validate remote event payloads to protect against malicious clients.
- **Performance** – Binary `.rfw` blobs and data blobs can replace the text transport for smaller payloads and faster parsing once implemented in the DSL.
- **Observability** – Events routed to the API can be instrumented with tracing/logging and correlated with backend actions.

## Next Steps
- Extend the DSL to support advanced RFW constructs (stateful widgets, switches, animations).
- Implement binary encoder/decoder parity with the Dart `rfw` package.
- Add hot-reload style update endpoints to push UI/data deltas without full reloads.
- Integrate automated tests (unit tests for serialization, integration tests covering the HTTP contract).
