#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.AddLibrary;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Management.AddLibrary;

/// <summary>
/// Contains integration tests for the <see cref="AddLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddLibraryEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedAdminClientAsync();
    }

    [Fact]
    public async Task AddLibrary_WhenCalledWithValidData_ShouldCreateLibrary()
    {
        // Arrange
        AddLibraryRequest request = new(
            Title: "New Library",
            LibraryType: "EBook",
            ContentLocations: ["C:/Media"],
            CoverImage: null,
            IsEnabled: true,
            IsLocked: false,
            DownloadMetadataFromWeb: true,
            ShouldSaveMetadataInMediaDirectories: false,
            ShouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/libraries", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        LibraryResponse? library = await response.Content.ReadFromJsonAsync<LibraryResponse>(_jsonOptions);
        Assert.NotNull(library);
        Assert.Equal("New Library", library!.Title);
        Assert.Equal(LibraryType.EBook, library.LibraryType);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryEntity? storedLibrary = await dbContext.Libraries.FirstOrDefaultAsync(lib => lib.Id == library.Id);
        Assert.NotNull(storedLibrary);
        Assert.Equal("New Library", storedLibrary!.Title);
    }

    [Fact]
    public async Task AddLibrary_WhenCalledByRegularUser_ShouldReturnForbidden()
    {
        // Arrange
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        AddLibraryRequest request = new(
            Title: "New Library",
            LibraryType: "EBook",
            ContentLocations: ["C:/Media"],
            CoverImage: null,
            IsEnabled: true,
            IsLocked: false,
            DownloadMetadataFromWeb: true,
            ShouldSaveMetadataInMediaDirectories: false,
            ShouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/libraries", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/libraries", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        // clear junction tables first
        await dbContext.Set<Lumina.Application.Common.DataAccess.Entities.Authorization.RolePermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<Lumina.Application.Common.DataAccess.Entities.Authorization.UserRoleEntity>().ExecuteDeleteAsync();
        await dbContext.Set<Lumina.Application.Common.DataAccess.Entities.Authorization.UserPermissionEntity>().ExecuteDeleteAsync();

        // then clear main tables
        await dbContext.Set<Lumina.Application.Common.DataAccess.Entities.UsersManagement.UserEntity>().ExecuteDeleteAsync();
        await dbContext.Set<Lumina.Application.Common.DataAccess.Entities.Authorization.RoleEntity>().ExecuteDeleteAsync();
        await dbContext.Set<Lumina.Application.Common.DataAccess.Entities.Authorization.PermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Libraries.ExecuteDeleteAsync();

        await dbContext.SaveChangesAsync();

        await _apiFactory.RemoveTestUserAsync();
    }
}
