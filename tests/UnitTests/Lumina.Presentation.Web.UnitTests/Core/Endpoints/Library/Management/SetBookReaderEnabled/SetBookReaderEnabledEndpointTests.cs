#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.SetBookReaderEnabled;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EmptyRequest = Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.SetBookReaderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetBookReaderEnabledEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetBookReaderEnabledEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly SetBookReaderEnabledEndpoint _sut;
    private readonly SetBookReaderEnabledRequestFixture _requestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetBookReaderEnabledEndpointTests"/> class.
    /// </summary>
    public SetBookReaderEnabledEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<SetBookReaderEnabledEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnSuccessJson()
    {
        // Arrange
        SetBookReaderEnabledRequest request = _requestFixture.Create();
        _mockApiHttpClient.PutAsync<EmptyRequest, SetBookReaderEnabledRequest>(Arg.Any<string>(), Arg.Any<SetBookReaderEnabledRequest>(), Arg.Any<CancellationToken>())
            .Returns(new EmptyRequest());

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendBookReaderEnabledChangeToApi()
    {
        // Arrange
        SetBookReaderEnabledRequest request = _requestFixture.Create();
        _mockApiHttpClient.PutAsync<EmptyRequest, SetBookReaderEnabledRequest>(Arg.Any<string>(), Arg.Any<SetBookReaderEnabledRequest>(), Arg.Any<CancellationToken>())
            .Returns(new EmptyRequest());
        string expectedEndpoint = ApiRoutes.Libraries.SET_LIBRARY_BOOK_READER_ENABLED
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{pluginId}", request.PluginId.ToString());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<EmptyRequest, SetBookReaderEnabledRequest>(
            expectedEndpoint,
            Arg.Is<SetBookReaderEnabledRequest>(payload =>
                payload.LibraryId == request.LibraryId &&
                payload.PluginId == request.PluginId &&
                payload.IsEnabled == request.IsEnabled),
            Arg.Any<CancellationToken>());
    }
}
