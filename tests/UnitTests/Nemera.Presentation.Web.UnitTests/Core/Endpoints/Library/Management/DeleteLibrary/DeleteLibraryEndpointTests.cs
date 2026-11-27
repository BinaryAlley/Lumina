#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.DeleteLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.DeleteLibrary;

/// <summary>
/// Contains unit tests for the <see cref="DeleteLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteLibraryEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly DeleteLibraryEndpoint _sut;
    private readonly DeleteLibraryRequestFixture _deleteLibraryRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLibraryEndpointTests"/> class.
    /// </summary>
    public DeleteLibraryEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<DeleteLibraryEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldDeleteLibraryViaApiAndReturnSuccess()
    {
        // Arrange
        DeleteLibraryRequest request = _deleteLibraryRequestFixture.Create();

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = ApiRoutes.Libraries.DELETE_LIBRARY.Replace("{id}", request.Id.ToString());
        await _mockApiHttpClient.Received(1).DeleteAsync(expectedEndpoint, Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
    }
}
