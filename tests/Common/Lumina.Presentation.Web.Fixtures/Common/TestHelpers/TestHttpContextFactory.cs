#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Claims;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.TestHelpers;

/// <summary>
/// Factory for creating and configuring <see cref="HttpContext"/> instances for unit testing Web endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TestHttpContextFactory
{
    /// <summary>
    /// Creates a <see cref="DefaultHttpContext"/> with a writable response body, optional culture route value, session and user.
    /// </summary>
    /// <param name="culture">The value of the <c>culture</c> route value.</param>
    /// <param name="user">The user associated with the context, <see langword="null"/> for anonymous requests.</param>
    /// <param name="session">The session associated with the context, <see langword="null"/> when no session is available.</param>
    /// <returns>A configured <see cref="DefaultHttpContext"/> instance.</returns>
    public static DefaultHttpContext Create(string? culture = "en-US", ClaimsPrincipal? user = null, ISession? session = null)
    {
        DefaultHttpContext httpContext = new();
        // provide a writable response body, so that executed IResult instances can be inspected by the tests
        httpContext.Features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(new MemoryStream()));
        // provide the minimal services needed by the result execution machinery (logging, options, JSON options)
        httpContext.RequestServices = CreateServiceProvider();
        httpContext.Request.RouteValues["culture"] = culture;
        if (user is not null)
            httpContext.User = user;
        if (session is not null)
            httpContext.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
        return httpContext;
    }

    /// <summary>
    /// Creates a service provider that provides the minimal services needed by the result execution machinery.
    /// </summary>
    /// <returns>A configured <see cref="IServiceProvider"/> instance.</returns>
    public static IServiceProvider CreateServiceProvider()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddOptions();
        services.Configure<JsonOptions>(jsonOptions => jsonOptions.SerializerOptions.MaxDepth = 256);
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Attaches a session to the provided HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context to configure.</param>
    /// <param name="session">The session to attach.</param>
    public static void ConfigureSession(HttpContext httpContext, ISession session)
    {
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
    }

    /// <summary>
    /// Attaches a user to the provided HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context to configure.</param>
    /// <param name="user">The user to attach.</param>
    public static void ConfigureUser(HttpContext httpContext, ClaimsPrincipal user)
    {
        httpContext.User = user;
    }

    /// <summary>
    /// Sets the value of the <c>culture</c> route value of the provided HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context to configure.</param>
    /// <param name="culture">The culture to set.</param>
    public static void ConfigureCulture(HttpContext httpContext, string? culture)
    {
        httpContext.Request.RouteValues["culture"] = culture;
    }

    /// <summary>
    /// Creates a mock <see cref="ISession"/> that supports the <c>GetString</c> and <c>SetString</c> extension methods.
    /// </summary>
    /// <returns>A mock <see cref="ISession"/> instance.</returns>
    public static ISession CreateSession()
    {
        ISession session = Substitute.For<ISession>();
#pragma warning disable CS8601 // possible null reference assignment in the NSubstitute out-argument stub
        session.TryGetValue(Arg.Any<string>(), out Arg.Any<byte[]>()).Returns(callInfo =>
        {
            callInfo[1] = null;
            return false;
        });
#pragma warning restore CS8601
        return session;
    }

    /// <summary>
    /// Creates an authenticated <see cref="ClaimsPrincipal"/> with the specified username.
    /// </summary>
    /// <param name="username">The username of the authenticated user.</param>
    /// <returns>An authenticated <see cref="ClaimsPrincipal"/> instance.</returns>
    public static ClaimsPrincipal CreateAuthenticatedUser(string username = "testuser")
    {
        return new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, username)], "TestAuthentication"));
    }

    /// <summary>
    /// Minimal <see cref="ISessionFeature"/> implementation used to attach a session to test HTTP contexts.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class TestSessionFeature : ISessionFeature
    {
        /// <summary>
        /// Gets or sets the session associated with the feature.
        /// </summary>
        public ISession Session { get; set; } = null!;
    }
}
