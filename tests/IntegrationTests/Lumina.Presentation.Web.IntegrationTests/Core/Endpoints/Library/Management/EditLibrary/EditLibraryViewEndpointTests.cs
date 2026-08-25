#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.EditLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.EditLibrary;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/item/{{id}}</c> route served by the <see cref="EditLibraryViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditLibraryViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EditLibraryViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public EditLibraryViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task EditLibraryView_WhenCalledByAuthenticatedUser_ShouldRenderLibraryEditingView()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto expectedLibrary = _libraryDtoFixture.Create(id: libraryId, title: "Books", libraryType: "Book");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", expectedLibrary);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/libraries/manage/item/{libraryId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains($"libraries/{libraryId}", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
