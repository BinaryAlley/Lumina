#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Middlewares;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Middlewares;

/// <summary>
/// Contains unit tests for the <see cref="ExceptionHandlingMiddleware"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ExceptionHandlingMiddlewareTests
{
    private readonly ILogger<ExceptionHandlingMiddleware> _mockLogger;
    private readonly IStringLocalizer<ExceptionHandlingMiddleware> _mockStringLocalizer;
    private readonly IAuthenticationService _mockAuthenticationService;
    private readonly ExceptionHandlingMiddleware _sut;
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private RequestDelegate _nextDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddlewareTests"/> class.
    /// </summary>
    public ExceptionHandlingMiddlewareTests()
    {
        _mockLogger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        _mockStringLocalizer = Substitute.For<IStringLocalizer<ExceptionHandlingMiddleware>>();
        _mockStringLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), $"localized:{callInfo.Arg<string>()}"));
        _mockAuthenticationService = Substitute.For<IAuthenticationService>();
        _mockAuthenticationService.SignOutAsync(Arg.Any<HttpContext>(), Arg.Any<string?>(), Arg.Any<AuthenticationProperties?>()).Returns(Task.CompletedTask);
        _mockAuthenticationService.ForbidAsync(Arg.Any<HttpContext>(), Arg.Any<string?>(), Arg.Any<AuthenticationProperties?>()).Returns(Task.CompletedTask);
        _nextDelegate = context => Task.CompletedTask;
        _sut = new ExceptionHandlingMiddleware(context => _nextDelegate(context), _mockLogger, _mockStringLocalizer);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextCompletesSuccessfully_ShouldNotModifyResponse()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext();
        _nextDelegate = context => Task.CompletedTask;

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.Equal(0, httpContext.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsNonApiException_ShouldLogErrorAndWriteFailureJson()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext();
        _nextDelegate = _ => throw new InvalidOperationException("Unexpected failure");

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Contains("application/json", httpContext.Response.ContentType);
        string body = await ReadResponseBodyAsync(httpContext);
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.False(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Unexpected failure", jsonDocument.RootElement.GetProperty("errorMessage").GetString());
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Is<Exception>(exception => exception is InvalidOperationException && exception.Message == "Unexpected failure"),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionUnauthorizedAndApiRequest_ShouldSignOutAndWriteUnauthorizedJson()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: true, requestServices: CreateServiceProvider());
        _nextDelegate = _ => throw new ApiException(null, HttpStatusCode.Unauthorized);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
        Assert.Equal("Unauthorized", await ReadErrorMessageAsync(httpContext));
        Assert.Contains("Token=", httpContext.Response.Headers["Set-Cookie"].ToString());
        await _mockAuthenticationService.Received(1).SignOutAsync(httpContext, CookieAuthenticationDefaults.AuthenticationScheme, Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionUnauthorizedAndPageRequest_ShouldSignOutAndRedirectToLogin()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: false, culture: "en-US", requestServices: CreateServiceProvider());
        httpContext.Request.Path = "/library/books";
        httpContext.Request.QueryString = new QueryString("?id=1");
        _nextDelegate = _ => throw new ApiException(null, HttpStatusCode.Unauthorized);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, httpContext.Response.StatusCode);
        Assert.Equal("/en-us/auth/login?returnUrl=%2Flibrary%2Fbooks%3Fid%3D1", httpContext.Response.Headers.Location.ToString());
        await _mockAuthenticationService.Received(1).SignOutAsync(httpContext, CookieAuthenticationDefaults.AuthenticationScheme, Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionNotFoundAndApiRequest_ShouldWriteNotFoundJson()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: true);
        _nextDelegate = _ => throw new ApiException(_problemDetailsDtoFixture.Create(), HttpStatusCode.NotFound);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
        Assert.Equal("NotFound", await ReadErrorMessageAsync(httpContext));
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionNotFoundAndPageRequest_ShouldRedirectToNotFoundPage()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: false, culture: "de-DE");
        _nextDelegate = _ => throw new ApiException(_problemDetailsDtoFixture.Create(), HttpStatusCode.NotFound);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, httpContext.Response.StatusCode);
        Assert.Equal("/de-de/not-found", httpContext.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionNotFoundForAuthenticatedAuthorizationCheckAndApiRequest_ShouldSignOutAndWriteUnauthorizedJson()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: true, user: CreateAuthenticatedUser(), requestServices: CreateServiceProvider());
        _nextDelegate = _ => throw new ApiException(_problemDetailsDtoFixture.Create(), HttpStatusCode.NotFound, "/api/v1/auth/get-authorization?userId=1");

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
        Assert.Equal("Unauthorized", await ReadErrorMessageAsync(httpContext));
        await _mockAuthenticationService.Received(1).SignOutAsync(httpContext, CookieAuthenticationDefaults.AuthenticationScheme, Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionNotFoundForAuthenticatedAuthorizationCheckAndPageRequest_ShouldSignOutAndRedirectToLogin()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: false, culture: null, user: CreateAuthenticatedUser(), requestServices: CreateServiceProvider());
        httpContext.Request.Path = "/dashboard";
        _nextDelegate = _ => throw new ApiException(_problemDetailsDtoFixture.Create(), HttpStatusCode.NotFound, "/api/v1/auth/get-authorization?userId=1");

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, httpContext.Response.StatusCode);
        Assert.Equal("/en-us/auth/login?returnUrl=%2Fdashboard", httpContext.Response.Headers.Location.ToString());
        await _mockAuthenticationService.Received(1).SignOutAsync(httpContext, CookieAuthenticationDefaults.AuthenticationScheme, Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionForbiddenAndApiRequest_ShouldWriteFailureJsonWithProblemDetail()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: true);
        ProblemDetailsDto problemDetails = _problemDetailsDtoFixture.Create(detail: "Access.Denied", title: "General.Forbidden");
        _nextDelegate = _ => throw new ApiException(problemDetails, HttpStatusCode.Forbidden);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        string errorMessage = await ReadErrorMessageAsync(httpContext);
        Assert.Contains("localized:Access.Denied", errorMessage);
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionForbiddenAndPageRequest_ShouldForbidTheRequest()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: false, requestServices: CreateServiceProvider());
        _nextDelegate = _ => throw new ApiException(_problemDetailsDtoFixture.Create(), HttpStatusCode.Forbidden);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        await _mockAuthenticationService.Received(1).ForbidAsync(httpContext, Arg.Any<string?>(), Arg.Any<AuthenticationProperties?>());
        Assert.Equal(0, httpContext.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionForbiddenAndXmlHttpRequestHeader_ShouldWriteFailureJson()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: false);
        httpContext.Request.Headers.XRequestedWith = "XMLHttpRequest";
        ProblemDetailsDto problemDetails = _problemDetailsDtoFixture.Create(detail: "Access.Denied", title: "General.Forbidden");
        _nextDelegate = _ => throw new ApiException(problemDetails, HttpStatusCode.Forbidden);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        string errorMessage = await ReadErrorMessageAsync(httpContext);
        Assert.Contains("localized:Access.Denied", errorMessage);
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionHasProblemDetail_ShouldWriteLocalizedDetailMessage()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: true);
        ProblemDetailsDto problemDetails = _problemDetailsDtoFixture.Create(detail: "Validation.Failed", title: "General.Validation", status: 400);
        _nextDelegate = _ => throw new ApiException(problemDetails, HttpStatusCode.BadRequest);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        string errorMessage = await ReadErrorMessageAsync(httpContext);
        Assert.Contains("localized:Validation.Failed", errorMessage);
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionHasOnlyProblemTitle_ShouldWriteLocalizedTitleMessage()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: true);
        ProblemDetailsDto problemDetails = new()
        {
            Title = "General.Failure",
            Status = 400
        };
        _nextDelegate = _ => throw new ApiException(problemDetails, HttpStatusCode.BadRequest);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        string errorMessage = await ReadErrorMessageAsync(httpContext);
        Assert.Contains("localized:General.Failure", errorMessage);
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionProblemDetailsContainValidationErrors_ShouldWriteLocalizedFieldErrors()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: true);
        ProblemDetailsDto problemDetails = DeserializeProblemDetailsWithErrors();
        _nextDelegate = _ => throw new ApiException(problemDetails, HttpStatusCode.BadRequest);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        string errorMessage = await ReadErrorMessageAsync(httpContext);
        Assert.Contains("localized:General.Validation", errorMessage);
        Assert.Contains("localized:EmailIsInvalid", errorMessage);
        Assert.Contains("localized:EmailIsTaken", errorMessage);
        Assert.Contains("localized:PasswordTooShort", errorMessage);
    }

    [Fact]
    public async Task InvokeAsync_WhenApiExceptionHasNoProblemDetails_ShouldWriteExceptionMessageAndLocalizedStatus()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext(isApiRequest: true);
        _nextDelegate = _ => throw new ApiException(null, HttpStatusCode.InternalServerError, "/api/v1/libraries/1");

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        string errorMessage = await ReadErrorMessageAsync(httpContext);
        Assert.Contains("An error occurred.", errorMessage);
        Assert.Contains("localized:InternalServerError", errorMessage);
    }

    /// <summary>
    /// Creates a <see cref="DefaultHttpContext"/> with a writable response body and optional API/user/culture/service configuration.
    /// </summary>
    /// <param name="isApiRequest">Whether the request accepts JSON, making it an API request.</param>
    /// <param name="culture">The culture route value, <see langword="null"/> when no route value is present.</param>
    /// <param name="user">The user associated with the context, <see langword="null"/> for anonymous requests.</param>
    /// <param name="requestServices">The request services, <see langword="null"/> to leave the default provider in place.</param>
    /// <returns>The configured <see cref="DefaultHttpContext"/> instance.</returns>
    private static DefaultHttpContext CreateHttpContext(bool isApiRequest = false, string? culture = null, ClaimsPrincipal? user = null, IServiceProvider? requestServices = null)
    {
        DefaultHttpContext httpContext = new()
        {
            Response = { Body = new MemoryStream() }
        };
        if (isApiRequest)
            httpContext.Request.Headers.Accept = "application/json";
        if (culture is not null)
            httpContext.Request.RouteValues["culture"] = culture;
        if (user is not null)
            httpContext.User = user;
        if (requestServices is not null)
            httpContext.RequestServices = requestServices;
        return httpContext;
    }

    /// <summary>
    /// Creates a service provider that resolves the mocked <see cref="IAuthenticationService"/>.
    /// </summary>
    /// <returns>The configured <see cref="IServiceProvider"/> instance.</returns>
    private IServiceProvider CreateServiceProvider()
    {
        ServiceCollection services = new();
        services.AddSingleton(_mockAuthenticationService);
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates an authenticated user principal with a name identifier claim.
    /// </summary>
    /// <returns>The created <see cref="ClaimsPrincipal"/> instance.</returns>
    private static ClaimsPrincipal CreateAuthenticatedUser()
    {
        return new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "TestAuthentication"));
    }

    /// <summary>
    /// Reads the current response body of the provided HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context whose response body is read.</param>
    /// <returns>The response body content.</returns>
    private static async Task<string> ReadResponseBodyAsync(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        using StreamReader reader = new(httpContext.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Reads the <c>errorMessage</c> property from the JSON response body of the provided HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context whose JSON response body is read.</param>
    /// <returns>The value of the <c>errorMessage</c> property.</returns>
    private static async Task<string> ReadErrorMessageAsync(HttpContext httpContext)
    {
        string body = await ReadResponseBodyAsync(httpContext);
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        return jsonDocument.RootElement.GetProperty("errorMessage").GetString() ?? string.Empty;
    }

    /// <summary>
    /// Deserializes a problem details payload that contains a validation errors extension object.
    /// </summary>
    /// <returns>The deserialized <see cref="ProblemDetailsDto"/> instance.</returns>
    private static ProblemDetailsDto DeserializeProblemDetailsWithErrors()
    {
        string problemDetailsJson = """
        {
            "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
            "title": "General.Validation",
            "status": 400,
            "errors": {
                "Email": ["EmailIsInvalid", "EmailIsTaken"],
                "Password": ["PasswordTooShort"]
            }
        }
        """;
        return JsonSerializer.Deserialize<ProblemDetailsDto>(problemDetailsJson)!;
    }
}
