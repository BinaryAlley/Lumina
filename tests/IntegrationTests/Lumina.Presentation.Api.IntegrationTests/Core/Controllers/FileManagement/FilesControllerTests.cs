#region ========================================================================= USING =====================================================================================
using FluentAssertions;
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
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Controllers.FileManagement;

/// <summary>
/// Contains integration tests for the <see cref="FilesController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FilesControllerTests : IClassFixture<LuminaApiFactory>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private static readonly bool s_isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesControllerTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public FilesControllerTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task GetTreeFiles_WhenCalledWithValidPathAndNotIncludeHiddenElements_ShouldReturnTreeFilesWithoutHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = Path.GetDirectoryName(testPath) ?? testPath;
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-tree-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<FileSystemTreeNodeResponse> files = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? file = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (file is not null)
                            files.Add(file);
                    }
                }
            }
            files.Should().NotBeNull();
            files!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);


            FileSystemTreeNodeResponse firstDirectory = files.First();
            firstDirectory.Name.Should().Be("TestFile_2.txt");
            firstDirectory.Path.Should().Be(Path.Combine(testPath, "TestFile_2.txt"));
            files.Count.Should().Be(1); // only files
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetTreeFiles_WhenCalledWithValidPathAndHiddenChildrenAndNotIncludeHiddenElements_ShouldReturnNoTreeFiles()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = Path.GetDirectoryName(testPath) ?? testPath;
        testPath = Path.GetDirectoryName(testPath) ?? testPath; // two levels to get to the element that has a hidden element as child
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-tree-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<FileSystemTreeNodeResponse> files = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? file = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (file is not null)
                            files.Add(file);
                    }
                }
            }
            files.Should().NotBeNull();
            FileSystemTreeNodeResponse firstDirectory = files.First();
            firstDirectory.Name.Should().Be("TestFile_2.txt");
            firstDirectory.Path.Should().Be(Path.Combine(testPath, "TestFile_2.txt"));
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetTreeFiles_WhenCalledWithValidPathAndWithIncludeHiddenElements_ShouldReturnTreeFilesWithHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = Path.GetDirectoryName(testPath) ?? testPath;
        testPath = Path.GetDirectoryName(testPath) ?? testPath; // two levels to get to the element that has a hidden element as child
        bool includeHiddenElements = true;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-tree-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<FileSystemTreeNodeResponse> files = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? file = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (file is not null)
                            files.Add(file);
                    }
                }
            }
            files.Should().NotBeNull();
            files!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            FileSystemTreeNodeResponse firstDirectory = files.First();
            firstDirectory.Name.Should().Be((s_isLinux ? "." : string.Empty) + "TestFile_1.txt");
            firstDirectory.Path.Should().Be(Path.Combine(testPath, (s_isLinux ? "." : string.Empty) + "TestFile_1.txt"));
            FileSystemTreeNodeResponse secondDirectory = files.Last();
            secondDirectory.Name.Should().Be("TestFile_2.txt");
            secondDirectory.Path.Should().Be(Path.Combine(testPath, "TestFile_2.txt"));
            files.Count.Should().Be(2); 
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetFiles_WhenCalledWithValidPathAndNotIncludeHiddenElements_ShouldReturnFilesWithoutHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = Path.GetDirectoryName(testPath) ?? testPath;
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<FileResponse> files = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileResponse? file = JsonSerializer.Deserialize<FileResponse>(element.GetRawText(), _jsonOptions);
                        if (file is not null)
                            files.Add(file);
                    }
                }
            }
            files.Should().NotBeNull();
            files!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);


            FileResponse firstDirectory = files.First();
            firstDirectory.Name.Should().Be("TestFile_2.txt");
            firstDirectory.Path.Should().Be(Path.Combine(testPath, "TestFile_2.txt"));
            files.Count.Should().Be(1); // only files
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetFiles_WhenCalledWithValidPathAndHiddenChildrenAndNotIncludeHiddenElements_ShouldReturnNoFiles()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = Path.GetDirectoryName(testPath) ?? testPath;
        testPath = Path.GetDirectoryName(testPath) ?? testPath; // two levels to get to the element that has a hidden element as child
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<FileResponse> files = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileResponse? file = JsonSerializer.Deserialize<FileResponse>(element.GetRawText(), _jsonOptions);
                        if (file is not null)
                            files.Add(file);
                    }
                }
            }
            files.Should().NotBeNull();
            FileResponse firstDirectory = files.First();
            firstDirectory.Name.Should().Be("TestFile_2.txt");
            firstDirectory.Path.Should().Be(Path.Combine(testPath, "TestFile_2.txt"));
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetFiles_WhenCalledWithValidPathAndWithIncludeHiddenElements_ShouldReturnFilesWithHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = Path.GetDirectoryName(testPath) ?? testPath;
        testPath = Path.GetDirectoryName(testPath) ?? testPath; // two levels to get to the element that has a hidden element as child
        bool includeHiddenElements = true;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<FileResponse> files = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileResponse? file = JsonSerializer.Deserialize<FileResponse>(element.GetRawText(), _jsonOptions);
                        if (file is not null)
                            files.Add(file);
                    }
                }
            }
            files.Should().NotBeNull();
            files!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            FileResponse firstDirectory = files.First();
            firstDirectory.Name.Should().Be( (s_isLinux ? "." : string.Empty) + "TestFile_1.txt");
            firstDirectory.Path.Should().Be(Path.Combine(testPath, (s_isLinux ? "." : string.Empty) + "TestFile_1.txt"));
            FileResponse secondDirectory = files.Last();
            secondDirectory.Name.Should().Be("TestFile_2.txt");
            secondDirectory.Path.Should().Be(Path.Combine(testPath, "TestFile_2.txt"));
            files.Count.Should().Be(2);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetFiles_WhenCancellationIsRequested_ShouldStopYieldingResults()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();

        bool includeHiddenElements = false;
        CancellationTokenSource cts = new();

        try
        {
            // Act
            Task<HttpResponseMessage> getTreeFilesTask = _client.GetAsync(
                $"/api/v1/files/get-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}",
                cts.Token
            );

            // simulate cancellation before the request completes
            cts.Cancel();

            HttpResponseMessage? response = null;
            try
            {
                response = await getTreeFilesTask;
            }
            catch (TaskCanceledException)
            {
                // expected when the task is cancelled, suppress the exception to continue the test
            }
            // Assert
            response?.StatusCode.Should().Be(HttpStatusCode.OK, "The cancellation should occur gracefully on the server side");

            // assert that no elements were returned after cancellation
            if (response != null)
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                    jsonDoc.RootElement.GetArrayLength().Should().Be(0, "no elements should be yielded after cancellation");
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }
    #endregion
}
