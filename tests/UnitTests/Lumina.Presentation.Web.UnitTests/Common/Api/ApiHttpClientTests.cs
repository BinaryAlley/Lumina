#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Fixtures.Common.Api;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Api;

/// <summary>
/// Contains unit tests for the <see cref="ApiHttpClient"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApiHttpClientTests
{
    private readonly ServerConfigurationDtoFixture _serverConfigurationDtoFixture = new();

    [Fact]
    public async Task GetAsync_WhenApiReturnsSuccess_ShouldDeserializeResponseContent()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """[{"title":"Library One"}]"""));
        ApiHttpClient sut = CreateSut(messageHandler);

        // Act
        Web.Common.DTO.Libraries.LibraryDto[] result = await sut.GetAsync<Web.Common.DTO.Libraries.LibraryDto[]>("libraries", CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Library One", result[0].Title);
        Assert.Equal(new Uri("http://localhost:5214/api/v1/libraries"), messageHandler.Requests[0].RequestUri);
    }

    [Fact]
    public async Task GetAsync_WhenApiReturnsError_ShouldThrowApiExceptionWithProblemDetails()
    {
        // Arrange
        string problemDetailsJson = """
        {
            "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
            "title": "General.Validation",
            "status": 422,
            "detail": "OneOrMoreValidationErrorsOccurred"
        }
        """;
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.UnprocessableEntity, problemDetailsJson));
        ApiHttpClient sut = CreateSut(messageHandler);

        // Act
        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => sut.GetAsync<object>("libraries", CancellationToken.None));

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.HttpStatusCode);
        Assert.NotNull(exception.ProblemDetails);
        Assert.Equal("General.Validation", exception.ProblemDetails!.Title);
        Assert.Equal(422, exception.ProblemDetails.Status);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", exception.ProblemDetails.Detail);
    }

    [Fact]
    public async Task GetAsync_WhenApiReturnsEmptyContent_ShouldReturnDefault()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) });
        ApiHttpClient sut = CreateSut(messageHandler);

        // Act
        object? result = await sut.GetAsync<object>("libraries", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenUserHasTokenClaim_ShouldSendAuthorizationHeader()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, "{}"));
        ApiHttpClient sut = CreateSut(messageHandler, token: "test-jwt-token");

        // Act
        await sut.GetAsync<object>("libraries", CancellationToken.None);

        // Assert
        Assert.Equal("Bearer", messageHandler.Requests[0].Headers.Authorization!.Scheme);
        Assert.Equal("test-jwt-token", messageHandler.Requests[0].Headers.Authorization!.Parameter);
    }

    [Fact]
    public async Task PostAsync_WhenCalled_ShouldSerializePayloadAndDeserializeResponse()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """{"success":true}"""));
        ApiHttpClient sut = CreateSut(messageHandler);

        // Act
        SuccessResponse result = await sut.PostAsync<SuccessResponse, object>("auth/login", new { username = "testuser" }, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(HttpMethod.Post, messageHandler.Requests[0].Method);
        Assert.Equal("http://localhost:5214/api/v1/auth/login", messageHandler.Requests[0].RequestUri!.ToString());
        Assert.Contains("testuser", messageHandler.RequestBodies[0]);
    }

    [Fact]
    public async Task DeleteAsync_WhenApiReturnsError_ShouldThrowApiException()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.InternalServerError, """{"title":"General.Failure","status":500}"""));
        ApiHttpClient sut = CreateSut(messageHandler);

        // Act
        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => sut.DeleteAsync("libraries/1", CancellationToken.None));

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, exception.HttpStatusCode);
        Assert.Equal("/api/v1/libraries/1", exception.RequestPath);
    }

    [Fact]
    public async Task GetBlobAsync_WhenApiReturnsSuccess_ShouldReturnBlobData()
    {
        // Arrange
        byte[] imageBytes = Encoding.UTF8.GetBytes("fake-image-bytes");
        TestApiHttpMessageHandler messageHandler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(imageBytes) };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            return response;
        });
        ApiHttpClient sut = CreateSut(messageHandler);

        // Act
        BlobDataDto blob = await sut.GetBlobAsync("libraries/1/cover", CancellationToken.None);

        // Assert
        Assert.Equal(imageBytes, blob.Data);
        Assert.Equal("image/png", blob.ContentType);
    }

    /// <summary>
    /// Creates the system under test configured with the provided message handler.
    /// </summary>
    /// <param name="messageHandler">The message handler backing the inner <see cref="HttpClient"/>.</param>
    /// <param name="token">Optional token claim to place on the current HTTP context user.</param>
    /// <returns>The created <see cref="ApiHttpClient"/>.</returns>
    private ApiHttpClient CreateSut(TestApiHttpMessageHandler messageHandler, string? token = null)
    {
        HttpClient httpClient = new(messageHandler);
        IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        if (token is not null)
        {
            DefaultHttpContext httpContext = new()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("Token", token)], "TestAuthentication"))
            };
            httpContextAccessor.HttpContext.Returns(httpContext);
        }
        IOptionsSnapshot<ServerConfigurationDto> serverConfigurationOptions = Substitute.For<IOptionsSnapshot<ServerConfigurationDto>>();
        serverConfigurationOptions.Value.Returns(_serverConfigurationDtoFixture.Create(apiVersion: '1', baseAddress: "http://localhost", port: 5214));
        return new ApiHttpClient(httpClient, httpContextAccessor, serverConfigurationOptions);
    }

    /// <summary>
    /// Creates an <see cref="HttpResponseMessage"/> with the given status code and JSON body.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="json">The JSON payload of the response body.</param>
    /// <returns>The created <see cref="HttpResponseMessage"/>.</returns>
    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}
