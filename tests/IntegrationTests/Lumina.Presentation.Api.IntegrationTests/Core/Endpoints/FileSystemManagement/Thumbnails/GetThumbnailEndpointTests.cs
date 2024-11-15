#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Fixtures;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Thumbnails;

/// <summary>
/// Contains integration tests for the <see cref="GetThumbnailEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailEndpointTests : IClassFixture<LuminaApiFactory>, IClassFixture<TestImageFixture>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly string _testImagePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    /// <param name="imageFixture">Fixture for creating and managing test images.</param>
    public GetThumbnailEndpointTests(LuminaApiFactory apiFactory, TestImageFixture imageFixture)
    {
        _client = apiFactory.CreateClient();
        _testImagePath = imageFixture.ImagePath;
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidPathAndQuality_ShouldReturnOkResultWithThumbnail()
    {
        // Arrange
        int quality = 80;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/thumbnails/get-thumbnail?path={Uri.EscapeDataString(_testImagePath)}&quality={quality}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/jpeg");
        byte[] content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidPath_ShouldReturnForbiddenResult()
    {
        // Arrange
        string invalidImagePath = "/path/to/nonexistent/image.jpg";
        int quality = 80;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/thumbnails/get-thumbnail?path={Uri.EscapeDataString(invalidImagePath)}&quality={quality}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status403Forbidden);
        problemDetails["title"].GetString().Should().Be("General.Failure");
        problemDetails["instance"].GetString().Should().Be("/api/v1/thumbnails/get-thumbnail");
        problemDetails["detail"].GetString().Should().Be("UnauthorizedAccess");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithEmptyPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string invalidImagePath = "";
        int quality = 80;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/thumbnails/get-thumbnail?path={Uri.EscapeDataString(invalidImagePath)}&quality={quality}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("General.Validation");
        problemDetails["instance"].GetString().Should().Be("/api/v1/thumbnails/get-thumbnail");
        problemDetails["detail"].GetString().Should().Be("OneOrMoreValidationErrorsOccurred");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().BeEquivalentTo(["PathCannotBeEmpty"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidQuality_ShouldReturnValidationProblemResult()
    {
        // Arrange
        int invalidQuality = -1;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/thumbnails/get-thumbnail?path={Uri.EscapeDataString(_testImagePath)}&quality={invalidQuality}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("General.Validation");
        problemDetails["instance"].GetString().Should().Be("/api/v1/thumbnails/get-thumbnail");
        problemDetails["detail"].GetString().Should().Be("OneOrMoreValidationErrorsOccurred");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().BeEquivalentTo(["ImageQaulityMustBeBetweenZeroAndOneHundred"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        int quality = 80;
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1/thumbnails/get-thumbnail?path={Uri.EscapeDataString(_testImagePath)}&quality={quality}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
