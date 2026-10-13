namespace Lumina.Presentation.Api.Common.Http;

/// <summary>
/// Marker metadata that opts an endpoint out of the eventual consistency pipeline, so that a long lived response (for example a track stream) never holds the request's database transaction open.
/// </summary>
public sealed class SkipEventualConsistencyMetadata
{
}
