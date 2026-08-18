#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetEnabledLibraries;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Management.GetEnabledLibraries;

/// <summary>
/// Contains integration tests for the <see cref="GetEnabledLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetEnabledLibrariesEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEnabledLibrariesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetEnabledLibrariesEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task GetEnabledLibraries_WhenEnabledLibrariesExist_ShouldReturnThem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid enabledLibraryId = Guid.NewGuid();
        await SeedLibraryAsync(enabledLibraryId, userId, "Enabled Library", isEnabled: true);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/enabled");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LibraryResponse[]? libraries = await response.Content.ReadFromJsonAsync<LibraryResponse[]>(_jsonOptions);
        Assert.NotNull(libraries);
        Assert.Contains(libraries!, library => library.Id == enabledLibraryId);
    }

    [Fact]
    public async Task GetEnabledLibraries_WhenOnlyDisabledLibrariesExist_ShouldReturnEmptyList()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        await SeedLibraryAsync(Guid.NewGuid(), userId, "Disabled Library", isEnabled: false);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/enabled");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LibraryResponse[]? libraries = await response.Content.ReadFromJsonAsync<LibraryResponse[]>(_jsonOptions);
        Assert.NotNull(libraries);
        Assert.Empty(libraries!);
    }

    /// <summary>
    /// Seeds a <see cref="LibraryEntity"/> owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library to seed.</param>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="title">The title of the library.</param>
    /// <param name="isEnabled">Whether the library is enabled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedLibraryAsync(Guid libraryId, Guid userId, string title, bool isEnabled)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(new LibraryEntity
        {
            Id = libraryId,
            UserId = userId,
            Title = title,
            LibraryType = LibraryType.EBook,
            ContentLocations = [],
            IsEnabled = isEnabled,
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedBy = null
        });
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
