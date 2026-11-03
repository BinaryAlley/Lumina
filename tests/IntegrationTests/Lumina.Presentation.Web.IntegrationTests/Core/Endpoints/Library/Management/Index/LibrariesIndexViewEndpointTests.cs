#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.Index;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.Index;

/// <summary>
/// Contains integration tests for the <see cref="LibrariesIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibrariesIndexViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrariesIndexViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public LibrariesIndexViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task LibrariesIndexView_WhenCalledByAuthenticatedUser_ShouldRenderLibrariesManagementView()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/libraries/manage");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
    }

    [Theory]
    [InlineData("/en-us/libraries/manage", "%2Fen-us%2Flibraries%2Fmanage")] // Plain page.
    [InlineData("/en-us/libraries/manage?tab=scans", "%2Fen-us%2Flibraries%2Fmanage%3Ftab%3Dscans")] // Query string is kept.
    public async Task LibrariesIndexView_WhenNavigatedToWithoutAuthentication_ShouldRedirectToLoginWithRequestedPageAsReturnUrl(string requestedUrl, string expectedEncodedReturnUrl)
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage request = new(HttpMethod.Get, requestedUrl);
        request.Headers.Accept.ParseAdd("text/html");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"http://localhost/en-us/auth/login?returnUrl={expectedEncodedReturnUrl}", response.Headers.Location!.ToString());
    }

    [Theory]
    [InlineData("User", true, "true")] // The permission lets a user manage the libraries they own.
    [InlineData("User", false, "false")] // Without the permission, a user cannot change even the libraries they own.
    [InlineData("Admin", false, "true")] // An administrator can manage every library, with or without the permission.
    public async Task LibrariesIndexView_WhenRendered_ShouldOfferTheEditAndDeleteActionsOnlyToUsersAllowedToManageLibraries(string role, bool hasManagePermission, string expectedCanManageLibraries)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(
            userId: Guid.NewGuid(),
            role: role,
            permissions: hasManagePermission ? [AuthorizationPermission.CanCreateLibraries] : []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/libraries/manage");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains($"const canManageLibraries = {expectedCanManageLibraries};", content);
    }

    [Fact]
    public async Task LibrariesIndexView_WhenRenderedInACultureWithNonAsciiLabels_ShouldInjectTheScriptLabelsAsJson()
    {
        // Arrange
        // Razor would encode the non-ASCII letters of a raw label as HTML entities, which the browser shows literally inside a script, while JSON escapes them
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/ro-ro/libraries/manage");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"LibraryDeleted\":\"Biblioteca a fost \\u0219tears\\u0103!\"", content);
        Assert.DoesNotContain("&#x219;tears", content);
    }
}
