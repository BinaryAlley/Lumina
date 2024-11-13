#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Drives;

/// <summary>
/// Contains integration tests for the <see cref="GetDrivesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDrivesEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private static readonly bool s_isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetDrivesEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnDrives()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/drives/get-drives");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        List<FileSystemTreeNodeResponse>? drives = JsonSerializer.Deserialize<List<FileSystemTreeNodeResponse>>(content, _jsonOptions);

        drives.Should().NotBeNull();
        drives.Should().NotBeEmpty();

        FileSystemTreeNodeResponse firstDrive = drives!.First();
        firstDrive.Name.Should().Be(s_isUnix ? "/" : "C:\\");
        firstDrive.Path.Should().Be(s_isUnix ? "/" : "C:\\");
        firstDrive.ItemType.Should().Be(FileSystemItemType.Root);
        drives!.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}
