#region ========================================================================= USING =====================================================================================
using System;
using System.Net.Http;
#endregion

namespace Nemera.Infrastructure.Common.Networking;

/// <summary>
/// The exception thrown when the host of an outbound request cannot be resolved.
/// </summary>
/// <remarks>
/// The <see cref="HttpRequestError.NameResolutionError"/> error tells the provider clients that the failure is transient and may be retried, unlike a rejection
/// by the host address policy. When the exception is thrown by the connect callback, SocketsHttpHandler wraps it in an <see cref="HttpRequestException"/> whose
/// error is <see cref="HttpRequestError.ConnectionError"/> and whose inner exception is this one, so consumers inspect the exception chain to distinguish it from a policy rejection.
/// </remarks>
internal sealed class PublicHostResolutionException : HttpRequestException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicHostResolutionException"/> class.
    /// </summary>
    /// <param name="host">The host that could not be resolved.</param>
    /// <param name="innerException">The exception that caused the resolution failure, when one is available.</param>
    public PublicHostResolutionException(string host, Exception? innerException = null)
        : base(HttpRequestError.NameResolutionError, $"The host '{host}' could not be resolved.", innerException)
    {
    }
}
