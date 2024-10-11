#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Lumina.Presentation.Api.IntegrationTests.Core.Controllers.FileManagement.Fixtures;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Controllers.FileManagement;

/// <summary>
/// Contains integration tests for the <see cref="FileSystemController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemControllerTests : IClassFixture<LuminaApiFactory>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemControllerTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public FileSystemControllerTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task GetType_WhenCalled_ShouldReturnOkResultWithFileSystemTypeResponse()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1.0/file-system/get-type");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        FileSystemTypeResponse? result = JsonSerializer.Deserialize<FileSystemTypeResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.PlatformType.Should().BeOneOf(PlatformType.Unix, PlatformType.Windows);
    }

    [Fact]
    public async Task GetType_WhenCalledMultipleTimes_ShouldReturnConsistentResults()
    {
        // Act
        HttpResponseMessage response1 = await _client.GetAsync("/api/v1.0/file-system/get-type");
        HttpResponseMessage response2 = await _client.GetAsync("/api/v1.0/file-system/get-type");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        string content1 = await response1.Content.ReadAsStringAsync();
        string content2 = await response2.Content.ReadAsStringAsync();

        FileSystemTypeResponse? result1 = JsonSerializer.Deserialize<FileSystemTypeResponse>(content1, _jsonOptions);
        FileSystemTypeResponse? result2 = JsonSerializer.Deserialize<FileSystemTypeResponse>(content2, _jsonOptions);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task GetType_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync("/api/v1.0/file-system/get-type", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }
    #endregion
}
