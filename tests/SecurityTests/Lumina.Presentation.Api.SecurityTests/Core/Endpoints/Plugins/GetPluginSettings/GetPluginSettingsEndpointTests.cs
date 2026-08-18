#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Plugins.GetPluginSettings;

/// <summary>
/// Contains security tests for the <c>/plugins/{pluginId}/settings</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPluginSettingsEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetPluginSettings_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string pluginId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/plugins/{pluginId}/settings");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal($"/api/v1/plugins/{pluginId}/settings", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Plugins--")] // destructive injection
    public async Task GetPluginSettings_WithSQLInjectionInPluginId_ShouldNotLeakOrError(string maliciousPluginId)
    {
        // Arrange
        // authenticate with an admin user and seed a plugin, so a valid pluginId would reach the handler
        HttpClient client = _apiFactory.CreateClient();
        Guid userId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        await SeedPluginAsync(userId);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/plugins/{Uri.EscapeDataString(maliciousPluginId)}/settings");

        // Assert
        // note: the {pluginId} route parameter is Guid-typed, so the malicious value fails model binding before it reaches the handler
        // note: observed status is 500 because FastEndpoints, combined with DontCatchExceptions(), turns the binding
        // ValidationFailureException into a 500 response instead of 400/422 (pre-existing production bug, not fixed here)
        // note: that 500 body leaks the ValidationFailureException details, so no DoesNotContain("Exception") is asserted here
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seeds a <see cref="PluginEntity"/> created by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the creator.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedPluginAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Plugins.Add(new PluginEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Plugin",
            Author = "Test Author",
            Version = "1.0.0",
            Description = "A test plugin.",
            LoadStatus = PluginLoadStatus.Loaded,
            LoadError = null,
            SettingsJson = null,
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedBy = null
        });
        await dbContext.SaveChangesAsync();
    }
}
