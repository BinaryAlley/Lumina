#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Api;

/// <summary>
/// Interface for typed clients that stream responses from the remote API without buffering their content.
/// </summary>
public interface IStreamingApiHttpClient
{
    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the response as a stream, without ever buffering its content in memory.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="requestHeaders">The range and conditional headers of the incoming request to forward, so that the API,
    /// which is the only party that knows the entity tag and the modification time of the resource, validates them itself.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The streamed response of the API request, which the caller must dispose once it has finished copying it.</returns>
    Task<ApiStreamResult> GetStreamAsync(string endpoint, IReadOnlyDictionary<string, string> requestHeaders, CancellationToken cancellationToken = default);
}
