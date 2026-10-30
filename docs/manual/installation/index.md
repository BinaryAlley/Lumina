# Installation

## Contents

**Getting started**

1. [Introduction](../index.md)
2. **Installation**

**Using Lumina**

3. [Using Lumina](../using-lumina/index.md)

**Administration**

4. [Managing Libraries](../managing-libraries/index.md)
5. [Plugins](../plugins/index.md)
6. [Themes](../themes/index.md)

Lumina ships as two applications that talk to each other over the network: the **API server** and the **web client**. You can run both on one machine or split them across many.

> **Not production ready yet.** Lumina is pre-alpha software. Updates are one way and can lose Lumina's data, so read [Updating](#updating) before you install or upgrade.

There are **two ways to run Lumina**. Pick one:

- **[Option A: run with Docker Compose](#option-a-run-with-docker-compose)**, the recommended route, where a single command brings up the whole system.
- **[Option B: run the applications directly](#option-b-run-the-applications-directly)**, whether you install a release package or build from source, if you would rather run them without containers.

Everything from Configuration onward applies to whichever route you chose.

## Before you start

- **Docker**, if you want the container route. For a non-container install, you need the Lumina application files, from a release package or installer, or from your own build, plus the **.NET 10 runtime** unless that package is self-contained. The **.NET 10 SDK** is only needed if you build from source.
- **ffmpeg**, for any non-container install, however you obtained the application files. The API server uses it to remux or transcode audio that the browser cannot play as stored. The Docker API container image already includes it.
- By default the **web client listens on port `5012`** and the **API on port `5214`**. You can change either, as long as the two agree on where the API is (see [Configuration](#configuration)).
- For anything reachable beyond your own machine, put a **reverse proxy** in front to terminate TLS and forward to the web client.
- Lumina needs **no external database server**. It keeps its data in a local SQLite file that is created automatically.

## Option A: run with Docker Compose

The `src` directory ships a `docker-compose.yml` that brings up the whole system. From that directory:

```bash
docker compose up
```

Add `--build` after changing the code, so the images are rebuilt:

```bash
docker compose up --build
```

This starts the following services:

| Service | Purpose | Address or endpoint |
| --- | --- | --- |
| `lumina-web` | Web client | http://localhost:5012 |
| `lumina-api` | API server | http://localhost:5214 |
| `otel-collector` | Receives OpenTelemetry data | OTLP at `localhost:4317` (gRPC) and `localhost:4318` (HTTP) |
| `jaeger` | Trace search | http://localhost:16686 |
| `prometheus` | Metrics | http://localhost:9090 |
| `grafana` | Dashboards | http://localhost:3000 (bound to `127.0.0.1`) |

Inside the stack, the web client is pointed at the API with the `ServerConfiguration__BaseAddress=http://lumina-api` environment variable, and the API and web client ship their telemetry to the collector.

The observability stack is meant for local development. Grafana signs in with `admin` and a password defaulting to `lumina-admin`, which you must override for any real deployment:

```bash
GRAFANA_ADMIN_PASSWORD=change-me docker compose up
```

### Production secrets

The development configuration embeds throwaway keys so the container route works out of the box. A real deployment must supply its own, or the API refuses to start:

- `JwtSettings__SecretKey`: the signing key for authentication tokens.
- `EncryptionSettings__SecretKey`: a base64 encryption key, used by both the API and the web client.

Set them as environment variables on the `lumina-api` and `lumina-web` services in your own compose file or orchestration.

## Option B: run the applications directly

Use a release package or installer when one is available, or, with the .NET 10 SDK installed, build the solution from the `src` directory:

```bash
dotnet build Lumina.slnx
```

Run the API and the web client from the installed release package or from the published build. In development, their addresses come from each project's `launchSettings.json` (`http://localhost:5214` and `http://localhost:5012`). When you run a published build directly on a host, set the addresses yourself, either with `ASPNETCORE_URLS` or with `ASPNETCORE_HTTP_PORTS`:

```bash
ASPNETCORE_HTTP_PORTS=5012 dotnet Lumina.Presentation.Web.dll
```

The API creates its SQLite database (`Lumina.db`) next to its own binary and applies any pending migrations at startup, so the first launch sets up the schema on its own.

The API server needs **ffmpeg** on its `PATH` to normalize audio that a player cannot play as stored. Install it with your platform's package manager, or point `TranscodingSettings:FfmpegPath` at the binary if it is somewhere else.

## Configuration

Each application reads its own `appsettings.json` and an optional environment specific `appsettings.development.json`, and environment variables override both. The API also loads the `appsettings.shared.json` file (and its development variant), which holds the server side settings that are not specific to a single layer, such as the media, plugins and theme options.

All settings are validated when the application starts. A missing or malformed value stops the process immediately with a clear error. In environment variables, sections are flattened with a double underscore, so `JwtSettings:SecretKey` becomes `JwtSettings__SecretKey`.

The settings that matter most when installing:

| Setting | Where | Meaning |
| --- | --- | --- |
| `ServerConfiguration:BaseAddress`, `ServerConfiguration:Port`, `ServerConfiguration:ApiVersion` | Web client | Where the web client finds the API. Must match the API's listening address. |
| `CorsSettings:AllowedOrigins` | API server | The origins allowed to call the API from a browser. Must include the web client's public address. |
| `JwtSettings:SecretKey`, `JwtSettings:ExpiryMinutes`, `JwtSettings:Issuer`, `JwtSettings:Audience` | API server | Authentication token signing and lifetime. |
| `EncryptionSettings:SecretKey` | API server and web client | Base64 encryption key. |
| `MediaSettings:RootDirectory` | API server | Root folder for media, `media` by default, served by the API at the `/media` route. |
| `MediaSettings:LibrariesDirectory`, `MediaSettings:BooksDirectory`, `MediaSettings:MusicDirectory` | API server | Subfolders under the media root for the library types. |
| `PluginsSettings:Directory` | API server | Folder holding installed plugin assemblies, `plugins` by default. |
| `ThemeEngine:StoragePath`, `ThemeEngine:BundledThemesPath`, `ThemeEngine:DefaultThemeId` | API server | Where installed and bundled themes live, and which theme is activated by default. |
| `ThemeEngine:MaxArchiveBytes`, `MaxExpandedBytes`, `MaxSingleFileBytes`, `MaxEntries` | API server | Limits enforced on uploaded theme packs. |
| `TranscodingSettings:FfmpegPath`, `MaxConcurrentJobs`, `MaxConcurrentJobsPerUser`, `MaxCacheBytes`, `JobTimeoutSeconds`, `EvictionGraceSeconds`, `CacheDirectory` | API server | Where the ffmpeg binary is, how many audio normalization jobs may run at once in total and per user, the maximum size of the normalized audio cache in bytes, how long a single job may take in seconds, how long a freshly produced or served cache entry is protected from eviction, and where the cache is stored. When `CacheDirectory` is absent, a private directory under the API account's temporary directory is used. |
| `Telemetry:Enabled`, `Telemetry:Otlp:Endpoint`, `Telemetry:TraceSampleRatio`, `Telemetry:ConsoleExporterEnabled` | API server and web client | Telemetry behavior. The OTLP endpoint also falls back to the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable. Again, while telemetry is enabled as a feature, nothing is sent anywhere. |

The `LOG_PATH` environment variable selects where the rolling log files are written.

The normalized audio cache must not be placed inside `MediaSettings:RootDirectory`, because that directory is served at the `/media` route, and it must not contain that directory either, because cleaning the cache would remove the media. The startup configuration also rejects a cache directory that is a filesystem root, the application directory or one of its parents, or that overlaps the logs or plugins directories. The default uses a private directory under the API account's temporary directory and avoids all of these. The cache holds transcoded or remuxed audio only while it is useful: at API startup only the entries the cache itself owns are removed, and a directory is cleaned only when it carries Lumina's cache marker file (`.lumina-transcoding-cache`) or is empty, so a directory that belongs to something else is left untouched. Its total footprint is capped by `TranscodingSettings:MaxCacheBytes`, but an entry that is still being read, or that was just produced when the cache was already at the limit, is kept until it can be removed, so the cache can exceed the cap temporarily. The cache is not part of the media library.

The default cache directory is scoped to the API account, but the coordinator, the running jobs and the startup cleanup are per process. Run a single API process per account and host, or configure a distinct `CacheDirectory` for each instance. Playback delivery handles are also process local: in a load balanced multi-instance deployment, a track's manifest and its subsequent segment requests must reach the same API instance, so configure sticky routing for them as described for SignalR below. On a multi-user Unix host the directory is created with private permissions, and a pre-existing directory owned by another account is treated as unavailable rather than reused or deleted.

### Keeping the two applications in step

The web client and the API must agree on two addresses, and getting them wrong is the usual cause of a client that loads but never updates live:

- The web client's `ServerConfiguration:BaseAddress` and `ServerConfiguration:Port` must resolve to the API's listening address.
- The API's `CorsSettings:AllowedOrigins` must include the web client's public address.

The shipped defaults (`http://localhost:5012` for the client, `http://localhost:5214` for the API) already line up. If you change the host or the port of either, change the matching value in the other file too.

## Running behind a reverse proxy

A reverse proxy in front of the web client should forward the standard `X-Forwarded-For`, `X-Forwarded-Proto` and `X-Forwarded-Host` headers, because the client uses them to reconstruct the original request. If you host Lumina under a subpath, forward `X-Forwarded-Prefix` as well; the client honors it as its path base.

```nginx
# Nginx
proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
proxy_set_header X-Forwarded-Proto $scheme;
proxy_set_header X-Forwarded-Host  $host;
# only when hosting under a subpath, for example /lumina
proxy_set_header X-Forwarded-Prefix /lumina;
```

```apache
# Apache (httpd)
ProxyPreserveHost On
RequestHeader set X-Forwarded-For    "%{REMOTE_ADDR}s"
RequestHeader set X-Forwarded-Proto  "https"
RequestHeader set X-Forwarded-Host   "%{Host}i"
# only when hosting under a subpath, for example /lumina
RequestHeader set X-Forwarded-Prefix "/lumina"
```

```text
# Caddy
reverse_proxy localhost:5012 {
    header_up X-Forwarded-For    {remote_host}
    header_up X-Forwarded-Proto  {scheme}
    header_up X-Forwarded-Host   {host}
    # only when hosting under a subpath, for example /lumina
    header_up X-Forwarded-Prefix /lumina
}
```

SignalR relies on WebSockets, so the proxy must allow the WebSocket upgrade. A single instance needs nothing more; if you run several instances behind a load balancer, route a given client to the same instance so its SignalR connection stays on one node.

### Rate limiting headers

The authentication endpoints (login, registration and password recovery) are rate limited to 10 attempts per 15 minutes per client IP address. The API reads the client IP from `X-Forwarded-For` first and falls back to the direct connection address. If a reverse proxy does not forward the original client IP, every request looks like it comes from the proxy, and the limit becomes global instead of per visitor. Forward the header as shown above so the limit stays meaningful.

The header is trusted as it arrives, with no list of known proxies, so a client that can reach the API directly can set it and sidestep the limit. Keep the API reachable only through your proxy, so that only the proxy can set it.

## Health checks

Both applications expose liveness and readiness probes that container orchestrators and load balancers can call:

| Application | Endpoint | What it reports |
| --- | --- | --- |
| API server | `/health/live` | The process is running. |
| API server | `/health/ready` | The process is running and the database is reachable. |
| Web client | `/health/live` | The process is running. |
| Web client | `/health/ready` | The process is running and the API is reachable. |

## First run

Open the web client in a browser. When no account exists yet, Lumina detects that the application has not been set up and sends you to create the administrator account. Fill in the form, and you are signed in and ready to create your first library.

## Updating

Updating Lumina is not a step to take lightly. The API applies any pending database migrations at startup, and those migrations can change the schema and the way Lumina's own data is stored. That work is one way: there is no downgrade path and no built in rollback, so once an update has run you cannot simply reinstall the previous version and expect everything to be where you left it.

Bugs happen, and an update is a place they can bite. A migration can transform Lumina's data incorrectly, fail partway through, or leave your instance in a state that only reveals itself after it is too late to walk back. When that happens, data loss is possible, and the only thing standing between you and it is a backup you made yourself. Your original media files are only ever read, so they are not what is at stake here; what an update can affect is the data Lumina keeps for itself.

That is why **backing up before every update is mandatory, not optional**. Before you update:

1. Stop the API, so nothing is writing to the database.
2. Copy the SQLite database file (`Lumina.db`, next to the API binary).
3. Copy your installed plugins and themes.
4. Only then pull the new images, or rebuild, and restart.

Lumina is free and open source software, provided as is, without warranty of any kind. The project cannot recover data that was lost to an update, and assumes no responsibility for the outcome if you update without a backup. Treat the backup as part of the update, not as advice.
