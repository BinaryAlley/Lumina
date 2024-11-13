#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains integration tests for the <see cref="GetBooksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetBooksEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetBooks_WhenCalled_ShouldReturnBooks()
    {
        // Arrange
        HttpResponseMessage response = await _client.GetAsync("/api/v1/books");

        // Act
        response.EnsureSuccessStatusCode();

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Book[]? bookResponse = await response.Content.ReadFromJsonAsync<Book[]>(_jsonOptions);
        bookResponse.Should().NotBeNull();
    }
    [Fact]
    public async Task ExecuteAsync_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync($"/api/v1/books", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1/books", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
