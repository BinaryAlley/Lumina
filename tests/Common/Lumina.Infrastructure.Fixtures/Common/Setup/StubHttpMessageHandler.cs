#region ========================================================================= USING =====================================================================================
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Setup;

/// <summary>
/// <see cref="HttpMessageHandler"/> test double that returns the configured response for every request, without performing any network I/O.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private HttpStatusCode _statusCode;
    private byte[] _responseBody;

    /// <summary>
    /// Initializes a new instance of the <see cref="StubHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="statusCode">The initial status code of the response to return.</param>
    /// <param name="responseBody">The initial body of the response to return.</param>
    public StubHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK, byte[]? responseBody = null)
    {
        _statusCode = statusCode;
        _responseBody = responseBody ?? [];
    }

    /// <summary>
    /// Configures the response to return for every subsequent request.
    /// </summary>
    /// <param name="statusCode">The status code of the response to return.</param>
    /// <param name="responseBody">The body of the response to return.</param>
    public void SetResponse(HttpStatusCode statusCode, byte[] responseBody)
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
    }

    /// <summary>
    /// Returns a response with the configured status code and body.
    /// </summary>
    /// <param name="request">The request, which is ignored.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to cancel the operation.</param>
    /// <returns>The configured response.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new ByteArrayContent(_responseBody)
        });
    }
}
