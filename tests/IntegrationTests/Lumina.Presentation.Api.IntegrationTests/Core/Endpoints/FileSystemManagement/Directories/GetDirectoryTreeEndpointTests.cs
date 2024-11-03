#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Fixtures;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Directories;

/// <summary>
/// Contains integration tests for the <see cref="GetDirectoryTreeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoryTreeEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private static readonly bool s_isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoryTreeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetDirectoryTreeEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidPathAndIncludeFilesAndNotIncludeHiddenElements_ShouldReturnDirectoryTreeWithFilesAndNotHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();

        bool includeFiles = true;
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directory-tree?path={Uri.EscapeDataString(testPath)}&includeFiles={includeFiles}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            List<FileSystemTreeNodeResponse>? treeNodes = JsonSerializer.Deserialize<List<FileSystemTreeNodeResponse>>(content, _jsonOptions);
            
            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (s_isLinux) // on UNIX, the leading slash is considered "the drive", the root location - add it!
                pathSegments = ["/", .. pathSegments];
            // recursively validate the tree structure
            void ValidateNode(FileSystemTreeNodeResponse node, int depth)
            {
                // ensure the current node's name and path match the expected segment
                string expectedSegment = pathSegments[depth];

                if (depth == 0 && node.ItemType == FileSystemItemType.Root && expectedSegment.Length == 2 && char.IsLetter(expectedSegment[0]) && expectedSegment[1] == ':'
                    && !expectedSegment.EndsWith(System.IO.Path.DirectorySeparatorChar))
                    expectedSegment += System.IO.Path.DirectorySeparatorChar;

                node.Name.Should().Be(expectedSegment);

                string expectedPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), pathSegments.Take(depth + 1));
                if (expectedPath.StartsWith("//"))
                    expectedPath = expectedPath[1..];
                if (!expectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
                    expectedPath += System.IO.Path.DirectorySeparatorChar;
                node.Path.Should().EndWith(expectedPath);

                // if the node is a drive or directory, check its children recursively
                if (node.ItemType is FileSystemItemType.Root or FileSystemItemType.Directory)
                {
                    node.Children.Should().NotBeNull();
                    foreach (FileSystemTreeNodeResponse childNode in node.Children)
                    {
                        if (childNode.ItemType != FileSystemItemType.File) // go deeper one level, if the iterated node's children is also a directory 
                            ValidateNode(childNode, depth + 1);
                        else if (childNode.ItemType is FileSystemItemType.File)
                        {
                            childNode.Children.Should().BeEmpty(); // files should not have children
                            childNode.Name.Should().Be("TestFile_2.txt");
                        }
                    }
                }
            }
            FileSystemTreeNodeResponse rootNode = treeNodes!.First();
            ValidateNode(rootNode, 0);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidPathAndIncludeFilesAndWithIncludeHiddenElements_ShouldReturnDirectoryTreeWithFilesAndHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();

        bool includeFiles = true;
        bool includeHiddenElements = true;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directory-tree?path={Uri.EscapeDataString(testPath)}&includeFiles={includeFiles}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            List<FileSystemTreeNodeResponse>? treeNodes = JsonSerializer.Deserialize<List<FileSystemTreeNodeResponse>>(content, _jsonOptions);
            
            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (s_isLinux) // on UNIX, the leading slash is considered "the drive", the root location - add it!
                pathSegments = ["/", .. pathSegments];

            // recursively validate the tree structure
            void ValidateNode(FileSystemTreeNodeResponse node, int depth)
            {
                // ensure the current node's name and path match the expected segment
                string expectedSegment = pathSegments[depth];

                if (depth == 0 && node.ItemType == FileSystemItemType.Root && expectedSegment.Length == 2 && char.IsLetter(expectedSegment[0]) && expectedSegment[1] == ':'
                    && !expectedSegment.EndsWith(System.IO.Path.DirectorySeparatorChar))
                    expectedSegment += System.IO.Path.DirectorySeparatorChar;

                node.Name.Should().Be(expectedSegment);

                string expectedPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), pathSegments.Take(depth + 1));
                if (!expectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
                    expectedPath += System.IO.Path.DirectorySeparatorChar;
                if (expectedPath.StartsWith("//"))
                    expectedPath = expectedPath[1..];
                node.Path.Should().EndWith(expectedPath);

                // if the node is a drive or directory, check its children recursively
                if (node.ItemType is FileSystemItemType.Root or FileSystemItemType.Directory)
                {
                    node.Children.Should().NotBeNull();
                    foreach (FileSystemTreeNodeResponse childNode in node.Children)
                    {
                        if (childNode.ItemType != FileSystemItemType.File) // go deeper one level, if the iterated node's children is also a directory 
                            ValidateNode(childNode, depth + 1);
                        else if (childNode.ItemType is FileSystemItemType.File)
                        {
                            childNode.Children.Should().BeEmpty(); // files should not have children
                            childNode.Name.Should().BeOneOf((s_isLinux ? "." : string.Empty) + "TestFile_1.txt", "TestFile_2.txt");
                        }
                    }
                }
            }
            FileSystemTreeNodeResponse rootNode = treeNodes!.First();
            ValidateNode(rootNode, 0);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidPathAndNotIncludeFilesAndNotIncludeHiddenElements_ShouldReturnDirectoryTreeWithoutFiles()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();

        bool includeFiles = false;
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directory-tree?path={Uri.EscapeDataString(testPath)}&includeFiles={includeFiles}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            List<FileSystemTreeNodeResponse>? treeNodes = JsonSerializer.Deserialize<List<FileSystemTreeNodeResponse>>(content, _jsonOptions);

            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            // there should not be a File node
            treeNodes.Should().AllSatisfy(node => node.ItemType.Should().BeOneOf(FileSystemItemType.Directory, FileSystemItemType.Root));

            string[] pathSegments = testPath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (s_isLinux) // on UNIX, the leading slash is considered "the drive", the root location - add it!
                pathSegments = ["/", .. pathSegments];

            // recursively validate the tree structure
            void ValidateNode(FileSystemTreeNodeResponse node, int depth)
            {
                // ensure the current node's name and path match the expected segment
                string expectedSegment = pathSegments[depth];

                if (depth == 0 && node.ItemType == FileSystemItemType.Root && expectedSegment.Length == 2 && char.IsLetter(expectedSegment[0]) && expectedSegment[1] == ':'
                    && !expectedSegment.EndsWith(System.IO.Path.DirectorySeparatorChar))
                    expectedSegment += System.IO.Path.DirectorySeparatorChar;

                node.Name.Should().Be(expectedSegment);

                string expectedPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), pathSegments.Take(depth + 1));
                if (!expectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
                    expectedPath += System.IO.Path.DirectorySeparatorChar;
                if (expectedPath.StartsWith("//"))
                    expectedPath = expectedPath[1..];
                node.Path.Should().EndWith(expectedPath);

                node.Children.Should().NotBeNull();
                foreach (FileSystemTreeNodeResponse childNode in node.Children)
                    ValidateNode(childNode, depth + 1);
            }
            FileSystemTreeNodeResponse rootNode = treeNodes!.First();
            ValidateNode(rootNode, 0);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidPathAndNotIncludeFilesAndIncludeHiddenElements_ShouldReturnDirectoryTreeWithoutFiles()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();

        bool includeFiles = false;
        bool includeHiddenElements = true;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directory-tree?path={Uri.EscapeDataString(testPath)}&includeFiles={includeFiles}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            List<FileSystemTreeNodeResponse>? treeNodes = JsonSerializer.Deserialize<List<FileSystemTreeNodeResponse>>(content, _jsonOptions);

            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            // there should not be a File node
            treeNodes.Should().AllSatisfy(node => node.ItemType.Should().BeOneOf(FileSystemItemType.Directory, FileSystemItemType.Root));

            string[] pathSegments = testPath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (s_isLinux) // on UNIX, the leading slash is considered "the drive", the root location - add it!
                pathSegments = ["/", .. pathSegments];

            // recursively validate the tree structure
            void ValidateNode(FileSystemTreeNodeResponse node, int depth)
            {
                // ensure the current node's name and path match the expected segment
                string expectedSegment = pathSegments[depth];

                if (depth == 0 && node.ItemType == FileSystemItemType.Root && expectedSegment.Length == 2 && char.IsLetter(expectedSegment[0]) && expectedSegment[1] == ':'
                    && !expectedSegment.EndsWith(System.IO.Path.DirectorySeparatorChar))
                    expectedSegment += System.IO.Path.DirectorySeparatorChar;

                node.Name.Should().Be(expectedSegment);

                string expectedPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), pathSegments.Take(depth + 1));
                if (!expectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
                    expectedPath += System.IO.Path.DirectorySeparatorChar;
                if (expectedPath.StartsWith("//"))
                    expectedPath = expectedPath[1..];
                node.Path.Should().EndWith(expectedPath);

                node.Children.Should().NotBeNull();
                foreach (FileSystemTreeNodeResponse childNode in node.Children)
                    ValidateNode(childNode, depth + 1);
            }
            FileSystemTreeNodeResponse rootNode = treeNodes!.First();
            ValidateNode(rootNode, 0);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string invalidPath = "invalid:path";
        string encodedPath = Uri.EscapeDataString(invalidPath);
        bool includeFiles = false;
        bool includeHiddenElements = true;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directory-tree?path={Uri.EscapeDataString(encodedPath)}&includeFiles={includeFiles}&includeHiddenElements={includeHiddenElements}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("Validation Error");
        problemDetails["instance"].GetString().Should().Be("/api/v1/directories/get-directory-tree");
        problemDetails["detail"].GetString().Should().Be("One or more validation errors occurred.");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().BeEquivalentTo(["InvalidPath"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithEmptyPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string emptyPath = "";
        string encodedPath = Uri.EscapeDataString(emptyPath);
        bool includeFiles = false;
        bool includeHiddenElements = true;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directory-tree?path={Uri.EscapeDataString(encodedPath)}&includeFiles={includeFiles}&includeHiddenElements={includeHiddenElements}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("Validation Error");
        problemDetails["instance"].GetString().Should().Be("/api/v1/directories/get-directory-tree");
        problemDetails["detail"].GetString().Should().Be("One or more validation errors occurred.");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().BeEquivalentTo(["PathCannotBeEmpty"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        bool includeFiles = false;
        bool includeHiddenElements = true;
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync($"/api/v1/directories/get-directory-tree?path={Uri.EscapeDataString(encodedPath)}&includeFiles={includeFiles}&includeHiddenElements={includeHiddenElements}", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        bool includeFiles = false;
        bool includeHiddenElements = true;
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1/directories/get-directory-tree?path={Uri.EscapeDataString(encodedPath)}&includeFiles={includeFiles}&includeHiddenElements={includeHiddenElements}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
