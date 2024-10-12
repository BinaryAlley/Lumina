#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
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
/// Contains integration tests for the <see cref="DrivesController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DrivesControllerTests : IClassFixture<LuminaApiFactory>
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
    /// Initializes a new instance of the <see cref="DrivesControllerTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DrivesControllerTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }


    [Fact]
    public async Task GetDrives_WhenCalled_ShouldReturnDrives()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/drives/get-drives");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        List<FileSystemTreeNodeResponse> drives = [];

        using (Stream stream = await response.Content.ReadAsStreamAsync())
        {
            using (JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream))
            {
                foreach (JsonElement element in jsonDoc.RootElement.EnumerateArray())
                {
                    FileSystemTreeNodeResponse? drive = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(element.GetRawText(), _jsonOptions);
                    if (drive is not null)
                        drives.Add(drive);
                }
            }
        }
        drives.Should().NotBeNull();
        drives.Should().NotBeEmpty();

        FileSystemTreeNodeResponse firstDrive = drives.First();
        firstDrive.Name.Should().Be(s_isLinux ? "/" : "C:\\");
        firstDrive.Path.Should().Be(s_isLinux ? "/" : "C:\\");
        firstDrive.ItemType.Should().Be(FileSystemItemType.Root);
        drives.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}
