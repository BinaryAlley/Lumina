#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Api;

/// <summary>
/// A streamed request that was sent to the <see cref="StubStreamingApiHttpClient"/> stub.
/// </summary>
/// <param name="Endpoint">The endpoint the stream was requested from.</param>
/// <param name="Headers">The request headers that were forwarded with the request.</param>
[ExcludeFromCodeCoverage]
public record StubStreamRequest(string Endpoint, IReadOnlyDictionary<string, string> Headers);
