namespace Lumina.Infrastructure.Common.Networking;

/// <summary>
/// Names of the named HTTP clients registered by the infrastructure layer.
/// </summary>
internal static class NamedHttpClients
{
    /// <summary>
    /// The name of the HTTP client used to download remote artwork, whose redirects are followed only to public hosts.
    /// </summary>
    internal const string ARTWORK_DOWNLOAD = "artwork-download";
}
