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
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Controllers.FileManagement;

/// <summary>
/// Contains integration tests for the <see cref="DirectoriesController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoriesControllerTests : IClassFixture<LuminaApiFactory>
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
    /// Initializes a new instance of the <see cref="DirectoriesControllerTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DirectoriesControllerTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task GetDirectoryTree_WhenCalledWithValidPathAndIncludeFilesAndNotIncludeHiddenElements_ShouldReturnDirectoryTreeWithFilesAndNotHiddenElements()
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

            List<FileSystemTreeNodeResponse> treeNodes = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? node = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (node is not null)
                            treeNodes.Add(node);
                    }
                }
            }
            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (s_isLinux) // on UNIX, the leading slash is considered "the drive", the root location - add it!
                pathSegments = new string[] { "/" }.Concat(pathSegments).ToArray();
            // recursively validate the tree structure
            void ValidateNode(FileSystemTreeNodeResponse node, int depth)
            {
                // ensure the current node's name and path match the expected segment
                string expectedSegment = pathSegments[depth];

                if (depth == 0 && node.ItemType == FileSystemItemType.Root && expectedSegment.Length == 2 && char.IsLetter(expectedSegment[0]) && expectedSegment[1] == ':' 
                    && !expectedSegment.EndsWith(Path.DirectorySeparatorChar))
                    expectedSegment += Path.DirectorySeparatorChar;

                node.Name.Should().Be(expectedSegment);

                string expectedPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathSegments.Take(depth + 1));
                if (expectedPath.StartsWith("//"))
                    expectedPath = expectedPath.Substring(1);
                if (!expectedPath.EndsWith(Path.DirectorySeparatorChar))
                    expectedPath += Path.DirectorySeparatorChar;
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
            FileSystemTreeNodeResponse rootNode = treeNodes.First();
            ValidateNode(rootNode, 0);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetDirectoryTree_WhenCalledWithValidPathAndIncludeFilesAndWithIncludeHiddenElements_ShouldReturnDirectoryTreeWithFilesAndHiddenElements()
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

            List<FileSystemTreeNodeResponse> treeNodes = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? node = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (node is not null)
                            treeNodes.Add(node);
                    }
                }
            }
            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (s_isLinux) // on UNIX, the leading slash is considered "the drive", the root location - add it!
                pathSegments = new string[] { "/" }.Concat(pathSegments).ToArray();

            // recursively validate the tree structure
            void ValidateNode(FileSystemTreeNodeResponse node, int depth)
            {
                // ensure the current node's name and path match the expected segment
                string expectedSegment = pathSegments[depth];

                if (depth == 0 && node.ItemType == FileSystemItemType.Root && expectedSegment.Length == 2 && char.IsLetter(expectedSegment[0]) && expectedSegment[1] == ':'
                    && !expectedSegment.EndsWith(Path.DirectorySeparatorChar))
                    expectedSegment += Path.DirectorySeparatorChar;

                node.Name.Should().Be(expectedSegment);

                string expectedPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathSegments.Take(depth + 1));
                if (!expectedPath.EndsWith(Path.DirectorySeparatorChar))
                    expectedPath += Path.DirectorySeparatorChar;
                if (expectedPath.StartsWith("//"))
                    expectedPath = expectedPath.Substring(1);
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
            FileSystemTreeNodeResponse rootNode = treeNodes.First();
            ValidateNode(rootNode, 0);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetDirectoryTree_WhenCalledWithValidPathAndNotIncludeFilesAndNotIncludeHiddenElements_ShouldReturnDirectoryTreeWithoutFiles()
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

            List<FileSystemTreeNodeResponse> treeNodes = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? node = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (node is not null)
                            treeNodes.Add(node);
                    }
                }
            }
            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            // there should not be a File node
            treeNodes.Should().AllSatisfy(node => node.ItemType.Should().BeOneOf(FileSystemItemType.Directory, FileSystemItemType.Root));

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (s_isLinux) // on UNIX, the leading slash is considered "the drive", the root location - add it!
                pathSegments = new string[] { "/" }.Concat(pathSegments).ToArray();

            // recursively validate the tree structure
            void ValidateNode(FileSystemTreeNodeResponse node, int depth)
            {
                // ensure the current node's name and path match the expected segment
                string expectedSegment = pathSegments[depth];

                if (depth == 0 && node.ItemType == FileSystemItemType.Root && expectedSegment.Length == 2 && char.IsLetter(expectedSegment[0]) && expectedSegment[1] == ':'
                    && !expectedSegment.EndsWith(Path.DirectorySeparatorChar))
                    expectedSegment += Path.DirectorySeparatorChar;

                node.Name.Should().Be(expectedSegment);

                string expectedPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathSegments.Take(depth + 1));
                if (!expectedPath.EndsWith(Path.DirectorySeparatorChar))
                    expectedPath += Path.DirectorySeparatorChar;
                if (expectedPath.StartsWith("//"))
                    expectedPath = expectedPath.Substring(1);
                node.Path.Should().EndWith(expectedPath);

                node.Children.Should().NotBeNull();
                foreach (FileSystemTreeNodeResponse childNode in node.Children)
                    ValidateNode(childNode, depth + 1);
            }
            FileSystemTreeNodeResponse rootNode = treeNodes.First();
            ValidateNode(rootNode, 0);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetDirectoryTree_WhenCalledWithValidPathAndNotIncludeFilesAndIncludeHiddenElements_ShouldReturnDirectoryTreeWithoutFiles()
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

            List<FileSystemTreeNodeResponse> treeNodes = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? node = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (node is not null)
                            treeNodes.Add(node);
                    }
                }
            }
            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            // there should not be a File node
            treeNodes.Should().AllSatisfy(node => node.ItemType.Should().BeOneOf(FileSystemItemType.Directory, FileSystemItemType.Root));

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (s_isLinux) // on UNIX, the leading slash is considered "the drive", the root location - add it!
                pathSegments = new string[] { "/" }.Concat(pathSegments).ToArray();

            // recursively validate the tree structure
            void ValidateNode(FileSystemTreeNodeResponse node, int depth)
            {
                // ensure the current node's name and path match the expected segment
                string expectedSegment = pathSegments[depth];

                if (depth == 0 && node.ItemType == FileSystemItemType.Root && expectedSegment.Length == 2 && char.IsLetter(expectedSegment[0]) && expectedSegment[1] == ':'
                    && !expectedSegment.EndsWith(Path.DirectorySeparatorChar))
                    expectedSegment += Path.DirectorySeparatorChar;

                node.Name.Should().Be(expectedSegment);

                string expectedPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathSegments.Take(depth + 1));
                if (!expectedPath.EndsWith(Path.DirectorySeparatorChar))
                    expectedPath += Path.DirectorySeparatorChar;
                if (expectedPath.StartsWith("//"))
                    expectedPath = expectedPath.Substring(1);
                node.Path.Should().EndWith(expectedPath);

                node.Children.Should().NotBeNull();
                foreach (FileSystemTreeNodeResponse childNode in node.Children)
                    ValidateNode(childNode, depth + 1);
            }
            FileSystemTreeNodeResponse rootNode = treeNodes.First();
            ValidateNode(rootNode, 0);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetTreeDirectories_WhenCalledWithValidPathAndNotIncludeHiddenElements_ShouldReturnTreeDirectoriesWithoutHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = Path.GetDirectoryName(testPath) ?? testPath;
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-tree-directories?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<FileSystemTreeNodeResponse> treeNodes = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? node = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (node is not null)
                            treeNodes.Add(node);
                    }
                }
            }
            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            
            FileSystemTreeNodeResponse firstNode = treeNodes.First();
            firstNode.Name.Should().Be("NestedDirectory_3");
            firstNode.Path.Should().Be(Path.Combine(testPath, "NestedDirectory_3" + Path.DirectorySeparatorChar));
            firstNode.ItemType.Should().Be(FileSystemItemType.Directory);
            treeNodes.Should().AllSatisfy(n => n.ItemType.Should().Be(FileSystemItemType.Directory));
            treeNodes.Count.Should().Be(1); // only directories
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetTreeDirectories_WhenCalledWithValidPathAndHiddenChildrenAndNotIncludeHiddenElements_ShouldReturnNoTreeDirectories()
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
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-tree-directories?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<FileSystemTreeNodeResponse> treeNodes = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? node = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (node is not null)
                            treeNodes.Add(node);
                    }
                }
            }
            treeNodes.Should().NotBeNull();
            treeNodes!.Should().BeEmpty();
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetTreeDirectories_WhenCalledWithValidPathAndWithIncludeHiddenElements_ShouldReturnTreeDirectoriesWithHiddenElements()
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
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-tree-directories?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<FileSystemTreeNodeResponse> treeNodes = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        FileSystemTreeNodeResponse? node = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                        if (node is not null)
                            treeNodes.Add(node);
                    }
                }
            }
            treeNodes.Should().NotBeNull();
            treeNodes!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);


            FileSystemTreeNodeResponse firstNode = treeNodes.First();
            firstNode.Name.Should().Be((s_isLinux ? "." : string.Empty) + "NestedDirectory_2");
            firstNode.Path.Should().Be(Path.Combine(testPath, (s_isLinux ? "." : string.Empty) + "NestedDirectory_2" + Path.DirectorySeparatorChar));
            firstNode.ItemType.Should().Be(FileSystemItemType.Directory);
            treeNodes.Should().AllSatisfy(n => n.ItemType.Should().Be(FileSystemItemType.Directory));
            treeNodes.Count.Should().Be(1); // only directories
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetDirectories_WhenCalledWithValidPathAndNotIncludeHiddenElements_ShouldReturnDirectoriesWithoutHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = Path.GetDirectoryName(testPath) ?? testPath;
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directories?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<DirectoryResponse> directories = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        DirectoryResponse? directory = JsonSerializer.Deserialize<DirectoryResponse>(element.GetRawText(), _jsonOptions);
                        if (directory is not null)
                            directories.Add(directory);
                    }
                }
            }
            directories.Should().NotBeNull();
            directories!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);


            DirectoryResponse firstDirectory = directories.First();
            firstDirectory.Name.Should().Be("NestedDirectory_3");
            firstDirectory.Path.Should().Be(Path.Combine(testPath, "NestedDirectory_3" + Path.DirectorySeparatorChar));
            directories.Count.Should().Be(1); // only directories
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetDirectories_WhenCalledWithValidPathAndHiddenChildrenAndNotIncludeHiddenElements_ShouldReturnNoDirectories()
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
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directories?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<DirectoryResponse> directories = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        DirectoryResponse? directory = JsonSerializer.Deserialize<DirectoryResponse>(element.GetRawText(), _jsonOptions);
                        if (directory is not null)
                            directories.Add(directory);
                    }
                }
            }
            directories.Should().NotBeNull();
            directories!.Should().BeEmpty();
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetDirectories_WhenCalledWithValidPathAndWithIncludeHiddenElements_ShouldReturnDirectoriesWithHiddenElements()
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
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directories?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<DirectoryResponse> directories = [];

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
                {
                    foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                    {
                        DirectoryResponse? directory = JsonSerializer.Deserialize<DirectoryResponse>(element.GetRawText(), _jsonOptions);
                        if (directory is not null)
                            directories.Add(directory);
                    }
                }
            }
            directories.Should().NotBeNull();
            directories!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);


            DirectoryResponse firstDirectory = directories.First();
            firstDirectory.Name.Should().Be((s_isLinux ? "." : string.Empty) + "NestedDirectory_2");
            firstDirectory.Path.Should().Be(Path.Combine(testPath, (s_isLinux ? "." : string.Empty) + "NestedDirectory_2" + Path.DirectorySeparatorChar));
            directories.Count.Should().Be(1); // only directories
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }
    #endregion
}
