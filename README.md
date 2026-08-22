# Lumina: because your files deserve better than collecting digital dust.

### Welcome to Lumina, the radiant new star in the galaxy of media servers! 🌟

Lumina aims to be your go-to hub for all things media - movies, TV shows, music, and more. We're just getting started, but our vision is bright:

- Stream your content anywhere, anytime.
- Organize your media with style.
- Support for any type of digital media.
- Sleek apps for all your devices.
- Lightning-fast performance.
- Your media, your control.
- Free and Open Source.

Stay tuned as we build something amazing together. Lumina is about to light up your media experience! 

P.S. This README is just a spark. Watch it grow into a supernova! ✨

## Documentation

For those with technical curiosities, here are some goodies to browse through:
- This application was written following Clean Architecture and Domain Driven Design. Although you've probably seen it countless times before, here is the diagram of how the structure looks like.
- A tree-like schematic of the Lumina's Server Domain layer can be seen [here](./docs/technical/domain/Domain.md).
- The Ubiquitous Language used by Lumina's Server Domain layer can be read [here](./docs/technical/domain/UbiquitousLanguage.md).
- For Software Architects, the architecture documents of Lumina were designed using the [C4 model](https://c4model.com/), therefor, you can visualize:
1. [1 System Context diagram](./docs/technical/architecture/1%20system-context-diagram.svg)
2. [2 Container diagram](./docs/technical/architecture/2%20container-diagram.svg)
3. [3.1 Component Web Client diagram](./docs/technical/architecture/3.1%20component-web-client-diagram.svg)
4. [3.2 Component API Server diagram](./docs/technical/architecture/3.2%20component-api-server-diagram.svg)
5. [4.1 Code media library scanning diagram](./docs/technical/architecture/4.1%20code-media-library-scanning-diagram.svg)
- Also for Software Architects, you might want to take a look at the Architecture Decision Log, where you may find important stories from the past of Lumina's development, like [this one](./docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md), and others like it.

## Telemetry and Observability

Lumina instruments its applications with OpenTelemetry (traces and metrics) and structured logging (Serilog), correlated through trace and span identifiers.

- Every query, command, and domain event handler in the application layer emits a trace span, latency and invocation metrics, and a structured log entry, through a telemetry decorator registered in the dependency injection container.
- Serilog enriches every log line with `TraceId` and `SpanId`, so logs can be joined to the traces that produced them.
- The API and Web applications export traces and metrics over OTLP, to an endpoint configured through `Telemetry:Otlp:Endpoint` or the standard `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable.
- The Web application injects the W3C trace context into its calls to the API, so a single trace spans both applications.

### Local observability stack

The `docker-compose.yml` file ships an observability stack for local development, in addition to the API and Web applications:

- OpenTelemetry Collector, receiving OTLP at `localhost:4317` (gRPC) and `localhost:4318` (HTTP), and fanning traces and metrics out to the backends below.
- Jaeger for trace search: http://localhost:16686
- Prometheus for metrics: http://localhost:9090
- Grafana for dashboards: http://localhost:3000, pre-provisioned with a "Lumina" dashboard (handler latency and invocation rates) and Prometheus and Jaeger data sources. The local development stack signs in with `admin` / `lumina-admin`, overridable through the `GRAFANA_ADMIN_PASSWORD` environment variable.

Start everything with `docker compose up` (or `docker compose up --build` after code changes), then open the dashboards. The Web application runs at http://localhost:5012 and the API at http://localhost:5214.

### Configuration

Telemetry behavior is controlled by the `Telemetry` section of `appsettings.json`:

- `Enabled`: master switch for the OpenTelemetry pipelines (default `true`).
- `Otlp:Endpoint`: OTLP exporter endpoint, falling back to the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable when empty.
- `TraceSampleRatio`: the ratio of traces to sample, `1.0` in development and `0.25` by default in production.
- `ConsoleExporterEnabled`: writes telemetry to the console for local inspection (default off outside the Development environment).

Note that telemetry being enabled by default does not mean anything is collected and sent anywhere on its own: with `Enabled` at `true`, the applications register the OpenTelemetry pipelines and keep emitting traces, metrics, and logs, but nothing leaves the process unless an OTLP endpoint is configured (via `Otlp:Endpoint` or `OTEL_EXPORTER_OTLP_ENDPOINT`) or the console exporter is turned on. Out of the box the default configuration sends nothing anywhere.

For a production deployment, point the OTLP endpoint at a real collector instead of relying on the local stack.

## Production notes

The development setup ships convenience defaults that must never be used in a real deployment:

- **API secrets.** The API requires a JWT signing key and an encryption key, both validated with `ValidateOnStart()`. Production deployments must supply their own values through the `JwtSettings__SecretKey` and `EncryptionSettings__SecretKey` environment variables, otherwise the API fails to start.
- **Grafana.** The local observability stack signs in with the well-known `admin` / `lumina-admin` credentials and binds Grafana to `127.0.0.1`. Do not leave the default password: set `GRAFANA_ADMIN_PASSWORD` (or `GF_SECURITY_ADMIN_PASSWORD`) before exposing Grafana beyond your own machine.
- **Scope.** The observability stack (collector, Jaeger, Prometheus, Grafana) in `docker-compose.yml` is intended for local development only.

## Contributing

Lumina welcomes community contributions. All forms of input, be it code, bug reports, or feature suggestions, are appreciated. Be sure to read the [guidelines](./docs/CONTRIBUTING.md) for contributing first!

## Acknowledgments and Credits

### Visual style

This project has drawn inspiration from the visual style and certain graphic elements of [Enlightenment](https://www.enlightenment.org/), a Window Manager, Compositor, and Minimal Desktop for Linux and other compatible UNIX systems. Their distinctive design has greatly influenced the aesthetics of this project, and I wish to express my profound appreciation for their innovative work.

Any modifications or adaptations made to the original graphics and styles are my responsibility. All rights, acknowledgments, and credits for the original design elements belong to the Enlightenment project and its contributors.

To explore more about Enlightenment and their contributions to the open-source community, please visit the [official Enlightenment website](https://www.enlightenment.org/).

### Icons

Most icons were taken from [Lyra Icon Theme](https://github.com/yeyushengfan258/Lyra-icon-theme/tree/master/src), a beautiful icon theme for Linux desktops.

Several icons used in this project were adapted from icons sourced from [svgrepo](https://www.svgrepo.com/).

- [`toggle-thumbnails.svg`](https://www.svgrepo.com/svg/370469/page-image) (Public Domain License)
- [`toggle-hidden.svg`](https://www.svgrepo.com/svg/470389/hidden) (MIT License)
- [`delete.svg`](https://www.svgrepo.com/svg/488148/delete) (MIT License)
- [`add-file.svg`](https://www.svgrepo.com/svg/467914/add-file-8) (Public Domain License)
- [`add-directory.svg`](https://www.svgrepo.com/svg/488040/add-folder) (MIT License)
- [`information.svg`](https://www.svgrepo.com/svg/403685/information) (MIT License)
- [`question.svg`](https://www.svgrepo.com/svg/486470/question-filled) (MIT License)
- [`warning.svg`](https://www.svgrepo.com/svg/454067/warning) (MIT License)
- [`error.svg`](https://www.svgrepo.com/svg/486408/error-filled) (MIT License)
- [`trigger.svg`](https://www.svgrepo.com/svg/489497/lightning-1) (Public Domain License)
- [`stop.svg`](https://www.svgrepo.com/svg/521862/stop) (CC Attribution License)

**I greatly appreciate the creators and contributors for providing these assets.**

## License

This project is licensed under the GPLv3.0. See the [LICENSE](./LICENSE.md) file for details.
