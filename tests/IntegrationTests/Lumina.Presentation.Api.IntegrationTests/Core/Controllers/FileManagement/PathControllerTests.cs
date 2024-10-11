#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
/// Contains integration tests for the <see cref="PathController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathControllerTests : IClassFixture<LuminaApiFactory>
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
    /// Initializes a new instance of the <see cref="PathControllerTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public PathControllerTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task GetPathRoot_WhenCalledWithValidPath_ShouldReturnOkResultWithPathSegmentResponse()
    {
        // Arrange
        string testPath = System.IO.Path.GetTempPath();
        string encodedPath = Uri.EscapeDataString(testPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/get-path-root?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathSegmentResponse? result = JsonSerializer.Deserialize<PathSegmentResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Path.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPathRoot_WhenCalledWithInvalidPath_ShouldReturnProblemDetails()
    {
        // Arrange
        string invalidPath = "invalid:path";
        string encodedPath = Uri.EscapeDataString(invalidPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/get-path-root?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPathRoot_WhenCalledWithEmptyPath_ShouldReturnBadRequestWithValidationError()
    {
        // Arrange
        string emptyPath = "";
        string encodedPath = Uri.EscapeDataString(emptyPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/get-path-root?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        ValidationProblemDetails? problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Errors["General.Validation"].Should().BeEquivalentTo("PathCannotBeEmpty");
    }

    [Fact]
    public async Task GetPathRoot_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        string testPath = System.IO.Path.GetTempPath();
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync($"/api/v1.0/path/get-path-root?path={encodedPath}", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetPathRoot_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.GetTempPath();
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1.0/path/get-path-root?path={encodedPath}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task GetPathSeparator_WhenCalled_ShouldReturnOkResultWithPathSeparatorResponse()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1.0/path/get-path-separator");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathSeparatorResponse? result = JsonSerializer.Deserialize<PathSeparatorResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Separator.Should().BeOneOf("/", "\\");
    }

    [Fact]
    public async Task GetPathSeparator_WhenCalledMultipleTimes_ShouldReturnConsistentResults()
    {
        // Act
        HttpResponseMessage response1 = await _client.GetAsync("/api/v1.0/path/get-path-separator");
        HttpResponseMessage response2 = await _client.GetAsync("/api/v1.0/path/get-path-separator");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        string content1 = await response1.Content.ReadAsStringAsync();
        string content2 = await response2.Content.ReadAsStringAsync();

        PathSeparatorResponse? result1 = JsonSerializer.Deserialize<PathSeparatorResponse>(content1, _jsonOptions);
        PathSeparatorResponse? result2 = JsonSerializer.Deserialize<PathSeparatorResponse>(content2, _jsonOptions);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task GetPathSeparator_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync("/api/v1.0/path/get-path-separator", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetPathSeparator_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync("/api/v1.0/path/get-path-separator", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task GetPathParent_WhenCalledWithValidPath_ShouldReturnOkResultWithPathSegmentResponses()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory", "testFile.txt");
        string encodedPath = Uri.EscapeDataString(testPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/get-path-parent?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        IEnumerable<PathSegmentResponse>? result = JsonSerializer.Deserialize<IEnumerable<PathSegmentResponse>>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Should().NotBeEmpty();

        List<string> hierarchy = [];
        string path = System.IO.Path.GetDirectoryName(testPath)!;
        while (!string.IsNullOrEmpty(path))
        {
            string segment = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(segment))
            {
                hierarchy.Add(System.IO.Path.GetPathRoot(path)!.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                break;
            }
            hierarchy.Add(segment);
            path = System.IO.Path.GetDirectoryName(path)!;
        }
        hierarchy.Reverse();
       
        result!.Select(r => r.Path).Should().BeEquivalentTo(hierarchy, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetPathParent_WhenCalledWithRootPath_ShouldReturnOkResultWithEmptyList()
    {
        // Arrange
        string rootPath = System.IO.Path.GetPathRoot(System.IO.Path.GetTempPath())!;
        string encodedPath = Uri.EscapeDataString(rootPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/get-path-parent?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();
        ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Failure");
        JsonElement jsonElement = (JsonElement)problemDetails!.Extensions["errors"]!;
        string?[] errors = jsonElement.EnumerateArray()
                                .Select(element => element.GetString())
                                .ToArray();
        errors.Should().BeEquivalentTo(["CannotNavigateUp"]);

    }

    [Fact]
    public async Task GetPathParent_WhenCalledWithInvalidPath_ShouldReturnBadRequest()
    {
        // Arrange
        string invalidPath = "invalid:path";
        string encodedPath = Uri.EscapeDataString(invalidPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/get-path-parent?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPathParent_WhenCalledWithEmptyPath_ShouldReturnBadRequestWithValidationError()
    {
        // Arrange
        string emptyPath = "";
        string encodedPath = Uri.EscapeDataString(emptyPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/get-path-parent?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        ValidationProblemDetails? problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Errors["General.Validation"].Should().BeEquivalentTo("PathCannotBeEmpty");
    }

    [Fact]
    public async Task GetPathParent_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync($"/api/v1.0/path/get-path-parent?path={encodedPath}", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetPathParent_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1.0/path/get-path-parent?path={encodedPath}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task CombinePath_WhenCalledWithValidPaths_ShouldReturnOkResultWithCombinedPath()
    {
        // Arrange
        string originalPath = System.IO.Path.GetTempPath();
        string newPath = "testDirectory";
        string expectedPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        if (!expectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
            expectedPath += System.IO.Path.DirectorySeparatorChar;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/combine?originalPath={Uri.EscapeDataString(originalPath)}&newPath={Uri.EscapeDataString(newPath)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathSegmentResponse? result = JsonSerializer.Deserialize<PathSegmentResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Path.Should().Be(expectedPath);
    }

    [Fact]
    public async Task CombinePath_WhenCalledWithEmptyPaths_ShouldReturnOkResultWithEmptyPath()
    {
        // Arrange
        string originalPath = "";
        string newPath = "";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/combine?originalPath={Uri.EscapeDataString(originalPath)}&newPath={Uri.EscapeDataString(newPath)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        ValidationProblemDetails? problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Errors["General.Validation"].Should().BeEquivalentTo("PathCannotBeEmpty", "PathCannotBeEmpty");
    }

    [Fact]
    public async Task CombinePath_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        string originalPath = "/home/user";
        string newPath = "documents";
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync($"/api/v1.0/path/combine?originalPath={Uri.EscapeDataString(originalPath)}&newPath={Uri.EscapeDataString(newPath)}", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CombinePath_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string originalPath = System.IO.Path.GetTempPath();
        string newPath = "testFile.txt";
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1.0/path/combine?originalPath={Uri.EscapeDataString(originalPath)}&newPath={Uri.EscapeDataString(newPath)}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task SplitPath_WhenCalledWithValidPath_ShouldReturnOkResultWithSplitSegments()
    {
        // Arrange
        string tempPath = System.IO.Path.GetTempPath();
        string testPath = System.IO.Path.Combine(tempPath, "testFolder", "testFile.txt");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/split?path={Uri.EscapeDataString(testPath)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        IEnumerable<PathSegmentResponse>? result = JsonSerializer.Deserialize<IEnumerable<PathSegmentResponse>>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Should().HaveCountGreaterThanOrEqualTo(3);
        result!.Select(r => r.Path).Should().Contain(["testFolder", "testFile.txt"]);
    }

    [Fact]
    public async Task SplitPath_WhenCalledWithEmptyPath_ShouldReturnOkResultWithEmptySegments()
    {
        // Arrange
        string path = "";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/split?path={Uri.EscapeDataString(path)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        ValidationProblemDetails? problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Errors["General.Validation"].Should().BeEquivalentTo("PathCannotBeEmpty");
    }

    [Fact]
    public async Task SplitPath_WhenCalledWithInvalidPath_ShouldReturnProblemDetails()
    {
        // Arrange
        string path = "invalid:path";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/split?path={Uri.EscapeDataString(path)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
    }

    [Fact]
    public async Task SplitPath_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1.0/path/split?path={encodedPath}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task ValidatePath_WhenCalledWithValidPath_ShouldReturnOkResultWithIsValidTrue()
    {
        // Arrange
        string tempPath = System.IO.Path.GetTempPath();
        string testPath = System.IO.Path.Combine(tempPath, "testFolder", "testFile.txt");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/validate?path={Uri.EscapeDataString(testPath)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathValidResponse? result = JsonSerializer.Deserialize<PathValidResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePath_WhenCalledWithInvalidPath_ShouldReturnOkResultWithIsValidFalse()
    {
        // Arrange
        string invalidPath = "invalid:path";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/validate?path={Uri.EscapeDataString(invalidPath)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathValidResponse? result = JsonSerializer.Deserialize<PathValidResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePath_WhenCalledWithEmptyPath_ShouldReturnBadRequest()
    {
        // Arrange
        string path = "";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/validate?path={Uri.EscapeDataString(path)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathValidResponse? result = JsonSerializer.Deserialize<PathValidResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePath_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1.0/path/validate?path={encodedPath}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task CheckPathExists_WhenCalledWithExistingPath_ShouldReturnOkResultWithExistsTrue()
    {
        // Arrange
        string tempPath = System.IO.Path.GetTempPath();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/check-path-exists?path={Uri.EscapeDataString(tempPath)}&includeHiddenElements=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathExistsResponse? result = JsonSerializer.Deserialize<PathExistsResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Exists.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPathExists_WhenCalledWithNonExistingPath_ShouldReturnOkResultWithExistsFalse()
    {
        // Arrange
        string nonExistingPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/check-path-exists?path={Uri.EscapeDataString(nonExistingPath)}&includeHiddenElements=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathExistsResponse? result = JsonSerializer.Deserialize<PathExistsResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPathExists_WhenCalledWithEmptyPath_ShouldReturnBadRequest()
    {
        // Arrange
        string path = "";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/check-path-exists?path={Uri.EscapeDataString(path)}&includeHiddenElements=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathExistsResponse? result = JsonSerializer.Deserialize<PathExistsResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPathExists_WhenCalledWithInvalidPath_ShouldReturnBadRequest()
    {
        // Arrange
        string invalidPath = "invalid:path";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1.0/path/check-path-exists?path={Uri.EscapeDataString(invalidPath)}&includeHiddenElements=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathExistsResponse? result = JsonSerializer.Deserialize<PathExistsResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPathExists_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.GetTempPath();
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1.0/path/check-path-exists?path={encodedPath}&includeHiddenElements=false", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
    #endregion
}
