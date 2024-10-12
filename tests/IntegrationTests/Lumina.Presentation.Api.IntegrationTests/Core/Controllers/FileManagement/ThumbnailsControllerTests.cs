#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Lumina.Presentation.Api.IntegrationTests.Core.Controllers.FileManagement.Fixtures;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Controllers.FileManagement;

/// <summary>
/// Contains integration tests for the <see cref="ThumbnailsController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailsControllerTests : IClassFixture<LuminaApiFactory>, IClassFixture<TestImageFixture>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly string _testImagePath;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailsControllerTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    /// <param name="imageFixture">Fixture for creating and managing test images.</param>
    public ThumbnailsControllerTests(LuminaApiFactory apiFactory, TestImageFixture imageFixture)
    {
        _client = apiFactory.CreateClient();
        _testImagePath = imageFixture.ImagePath;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task GetThumbnail_WhenCalledWithValidPathAndQuality_ShouldReturnOkResultWithThumbnail()
    {
        // Arrange
        int quality = 80;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/thumbnails/get-thumbnail?path={Uri.EscapeDataString(_testImagePath)}&quality={quality}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/jpeg");
        byte[] content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetThumbnail_WhenCalledWithInvalidPath_ShouldReturnForbidden()
    {
        // Arrange
        string invalidImagePath = "/path/to/nonexistent/image.jpg";
        int quality = 80;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/thumbnails/get-thumbnail?path={Uri.EscapeDataString(invalidImagePath)}&quality={quality}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();
        ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task GetThumbnail_WhenCalledWithEmptyPath_ShouldReturnForbidden()
    {
        // Arrange
        string invalidImagePath = "";
        int quality = 80;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/thumbnails/get-thumbnail?path={Uri.EscapeDataString(invalidImagePath)}&quality={quality}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        ValidationProblemDetails? problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Errors["General.Validation"].Should().BeEquivalentTo("PathCannotBeEmpty");
    }

    [Fact]
    public async Task GetThumbnail_WhenCalledWithInvalidQuality_ShouldReturnForbidden()
    {
        // Arrange
        int invalidQuality = -1;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/thumbnails/get-thumbnail?path={Uri.EscapeDataString(_testImagePath)}&quality={invalidQuality}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        ValidationProblemDetails? problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
    }

    [Fact]
    public async Task GetThumbnail_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        int quality = 80;
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1.0/thumbnails/get-thumbnail?path={Uri.EscapeDataString(_testImagePath)}&quality={quality}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
    #endregion
}
