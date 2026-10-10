# Architecture Decision Record (ADR): 0015 - Plugin Capability Providers for Media Library Enrichment

## Status

**Accepted** (2026-10-09)

Supersedes [ADR-0008](architecture-decision-record-0008.md).

## Context

[ADR-0008](architecture-decision-record-0008.md) extended the media library scanning subsystem with plugin jobs: it defined named scan job hook points in `ScanJobHooks` and a scan job registry (`IScanJobRegistry` / `ScanJobRegistry`) that mapped a hook name to the plugin job types to splice into the scan job graph at that junction.

That model coupled plugins to the host scan graph. A plugin had to know the internal job model and the junction semantics to contribute anything, the host had to know every plugin job type up front to build the graph, and the resulting graph mixed host jobs and plugin jobs with no clear ownership of the enrichment behavior. It also kept the `HashComparerJob` runtime type switching around, because jobs of different types exchanged differently typed payloads.

What a plugin actually contributes is data: metadata for a media item, artwork for a media item, or the ability to decode a book. Which jobs run, in what order, and how their results are persisted is a host concern that should not be delegated to plugins.

## Decision

Separate plugin capabilities from the host scan graph. Plugins contribute capability providers, and the host owns the graph and decides when to invoke the capabilities.

### Plugin contracts

- `IPlugin` carries the identity of the plugin (`Id`, `Name`, `Author`, `Version`, `Description`) and its settings schema (`GetSettingsSchema()`).
- `IPluginServiceRegistrator` registers the services of the plugin into the host dependency injection container through `RegisterServices(IServiceCollection)`.

### Capability contracts

The host defines the capability contracts, and a plugin implements the ones it supports:

| Capability | Contract | Purpose |
|---|---|---|
| Metadata provider | `IMetadataProvider` | Searches for and returns the metadata of a media item (`GetSearchResultsAsync`, `GetMetadataAsync`), declaring its `SupportedLibraryTypes`, the `LookupType` it accepts, and whether it `RequiresWebAccess` |
| Artwork provider | `IArtworkProvider` | Returns the artwork of a media item (`GetArtworkAsync`), declaring its `SupportedLibraryTypes` and whether it `RequiresWebAccess` |
| Book reader | `IBookReader` | Decodes a book format into a normalized reading document (`OpenAsync`, `GetResourceAsync`), declaring its `SupportedExtensions` and `SupportedLibraryTypes` |

### Registration and selection

- A plugin registers each capability as a keyed transient service, keyed by its plugin `Id` (for example `AddKeyedTransient<IMetadataProvider, MusicBrainzTrackMetadataProvider>(pluginId)`). A single plugin can register several providers, of the same or of different capabilities.
- Plugins contribute zero scan jobs. The scan job graph of each media library type is built entirely by its own scanner from host jobs created through `IMediaLibraryScanJobFactory`.
- The host persists which providers are enabled for each media library and their rank, and exposes that configuration through the media library management endpoints.
- The enrichment jobs resolve the configured, enabled providers of the media library and invoke them in rank order: the metadata enrichment job delegates to `IMediaLibraryScanMetadataEnricher`, the artwork enrichment job delegates to `IMediaLibraryScanArtworkEnricher`, and the results save job delegates to `IMediaLibraryScanItemMaterializer`.

### Removed

`ScanJobHooks`, `IScanJobRegistry` and `ScanJobRegistry` are removed, along with the `HashComparerJob` runtime type switching they depended on. [ADR-0008](architecture-decision-record-0008.md) is superseded.

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| Ownership | Plugins contribute data capabilities; the host owns the scan graph and when to invoke them |
| Decoupling | A plugin depends only on the capability contracts, not on the internal job model or the graph junctions |
| Extensibility | A new plugin adds keyed capability services and no host code, and a plugin can expose several capabilities |
| Per-library control | Providers are enabled and ranked per media library, independent of plugin installation |
| Simplicity | No hook registry, no graph splicing across the plugin boundary, and no runtime payload type switching |

### Risks and Mitigations

| Risk | Mitigation Strategy |
|---|---|
| A plugin registers a capability a library type does not support | Providers declare `SupportedLibraryTypes`, and the host only offers and invokes providers that support the library being processed |
| Two enabled providers return conflicting metadata | Providers are tried in the configured rank order, so precedence is explicit and user controlled |
| A crashing provider fails the enrichment | Enrichment is best effort and isolates provider failures ([ADR-0010](architecture-decision-record-0010.md)), so one provider cannot fail the scan |

## Alternatives Considered

### 1. Keep the scan job hook points and the scan job registry

**Rejected**: it couples plugins to the host scan graph, forces the host to know plugin job types up front, and mixes host and plugin jobs without a clear owner. It is the model this ADR replaces.

### 2. Let plugins register scan jobs directly, without a registry

**Rejected**: it removes the named junction points but keeps the coupling: plugins still have to know the job model, the graph shape and the payload contracts, and the graph still has no single owner.

### 3. Resolve capability providers by convention, without per-library configuration

**Rejected**: enabling and ranking providers per media library is a requirement. It lets a user run several metadata or artwork sources for one library, in a chosen precedence, and turn providers off without uninstalling the plugin.
