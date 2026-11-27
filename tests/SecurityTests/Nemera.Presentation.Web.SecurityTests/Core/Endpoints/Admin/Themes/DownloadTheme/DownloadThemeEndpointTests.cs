#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Enums.Authorization;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Themes.DownloadTheme;
using Nemera.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Themes.DownloadTheme;

/// <summary>
/// Contains security tests for the <see cref="DownloadThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DownloadThemeEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DownloadThemeEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task DownloadTheme_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage downloadRequest = new(HttpMethod.Get, "/en-us/admin/themes/api-download-theme/test-theme");
        downloadRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await client.SendAsync(downloadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task DownloadTheme_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage downloadRequest = new(HttpMethod.Get, "/en-us/admin/themes/api-download-theme/test-theme");
        downloadRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(downloadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.BlobEndpointsCalled);
    }

    [Fact]
    public async Task DownloadTheme_WhenCalledByAdminUser_ShouldReturnThemeArchive()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: Enum.GetValues<AuthorizationPermission>());
        byte[] expectedBytes = [0x50, 0x4B, 0x03, 0x04, 0x01, 0x02];
        _apiFactory.ApiClientStub.RegisterBlobResponse(_blobDataDtoFixture.Create(data: expectedBytes, contentType: "application/zip"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage downloadRequest = new(HttpMethod.Get, "/en-us/admin/themes/api-download-theme/test-theme");
        downloadRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(downloadRequest);
        byte[] content = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/zip", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal(expectedBytes, content);
    }
}
