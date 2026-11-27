namespace Nemera.Infrastructure.Common.Networking;

/// <summary>
/// Names of the named HTTP clients registered by the infrastructure layer.
/// </summary>
internal static class NamedHttpClients
{
    /// <summary>
    /// The name of the HTTP client used to download remote artwork, which is guarded like every other outbound client of the host.
    /// </summary>
    internal const string ARTWORK_DOWNLOAD = "artwork-download";
}
