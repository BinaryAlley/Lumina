#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using FastEndpoints.Swagger;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Presentation.Api.Common.Authentication;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
#endregion

namespace Lumina.Presentation.Api.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Presentation API layer.
/// </summary>
[ExcludeFromCodeCoverage]
public static class PresentationApiLayerServices
{
    /// <summary>
    /// Registers the services of the Presentation API layer into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">Application configuration properties.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPresentationApiLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache(); // used for the tracking involved in authentication rate limiting
        services.AddProblemDetails();

        services.Configure<JsonOptions>(jsonOptions =>
        {
            jsonOptions.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.SerializerOptions.MaxDepth = 256;
            jsonOptions.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // needed because file system API responses can have very nested structures
        });

        // TODO: also implement account locking after a number of failed login attempts
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            rateLimiterOptions.OnRejected = async (onRejectedContext, cancellationToken) =>
            {
                ILogger<Program> logger = onRejectedContext.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                string clientIp = onRejectedContext.HttpContext.Request.Headers["X-Forwarded-For"]
                    .FirstOrDefault()?.Split(',')[0].Trim()
                    ?? onRejectedContext.HttpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";
                logger.LogWarning(
                    "Rate limit exceeded. IP: {IpAddress}, Route: {Route}, Lease Acquired: {IsAcquired}",
                    clientIp,
                    onRejectedContext.HttpContext.Request.Path,
                    onRejectedContext.Lease.IsAcquired); // when False, the request was rejected because it exceeded rate limits

                onRejectedContext.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                onRejectedContext.HttpContext.Response.ContentType = "application/problem+json";

                // set standard rate limit headers
                if (onRejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    onRejectedContext.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                // set custom rate limit headers
                onRejectedContext.HttpContext.Response.Headers["X-RateLimit-Limit"] = "10";
                onRejectedContext.HttpContext.Response.Headers["X-RateLimit-Remaining"] = "0";
                onRejectedContext.HttpContext.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString();

                var problem = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.29",
                    title = "TooManyRequests", 
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "TooManyRequests",
                    retryAfter = "900"
                };

                await onRejectedContext.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            };
            rateLimiterOptions.AddPolicy("authenticationPolicy", httpContext =>
            {
                string clientIp = httpContext.Request.Headers["X-Forwarded-For"] // first check forwarded headers, in case this is behind a reverse proxy 
                    .FirstOrDefault()?.Split(',')[0].Trim() // take first IP, if multiple available, to avoid IP spoofing attempts
                    ?? httpContext.Connection.RemoteIpAddress?.ToString() // then try RemoteIpAddress
                    ?? "unknown"; // fallback if both are null
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"auth_{clientIp}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10, // allow 10 attempts
                        Window = TimeSpan.FromMinutes(15), // within 15 minute window
                        QueueLimit = 0,  // don't queue requests
                        AutoReplenishment = true  // automatically reset rate limiter tokens
                    });
            });
        });

        // add authentication and specify the JWT scheme to check tokens against
        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.GetValue<string>("JwtSettings:Issuer"),
                    ValidAudience = configuration.GetValue<string>("JwtSettings:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("JwtSettings:SecretKey")!)),
                };

                jwtBearerOptions.Events = new JwtBearerEvents
                {
                    OnChallenge = jwtBearerChallengeContext => // event triggered when authentication is not successful for whatever reason
                    {
                        jwtBearerChallengeContext.HandleResponse(); // prevent the default 401 response
                        // set the response status code to 401 Unauthorized
                        jwtBearerChallengeContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        jwtBearerChallengeContext.Response.ContentType = "application/problem+json";

                        string errorDetails = "Authentication failed";
                        if (!string.IsNullOrEmpty(jwtBearerChallengeContext.Error))
                        {
                            errorDetails = jwtBearerChallengeContext.Error switch
                            { // TODO: implement translation
                                "invalid_token" => !string.IsNullOrEmpty(jwtBearerChallengeContext.ErrorDescription)
                                    ? $"Invalid token: {jwtBearerChallengeContext.ErrorDescription}"
                                    : "The token is invalid",
                                "expired_token" => "The token has expired",
                                _ => $"Authentication failed{(jwtBearerChallengeContext.Error is not null ? ": " + jwtBearerChallengeContext.Error : string.Empty)}"
                            };
                        }

                        string detail = jwtBearerChallengeContext.Error switch
                        {
                            "invalid_token" => !string.IsNullOrEmpty(jwtBearerChallengeContext.ErrorDescription)
                                ? $"Invalid token: {jwtBearerChallengeContext.ErrorDescription}"
                                : "The token is invalid",
                            "expired_token" => "The token has expired",
                            _ => $"Authentication failed{(jwtBearerChallengeContext.Error is not null ? ": " + jwtBearerChallengeContext.Error : string.Empty)}"
                        };

                        return jwtBearerChallengeContext.Response.WriteAsJsonAsync(new
                        {
                            type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                            status = StatusCodes.Status401Unauthorized,
                            title = "Unauthorized",
                            detail,
                            instance = jwtBearerChallengeContext.HttpContext.Request.Path
                        });
                    }
                };
            });

        services.AddAuthorization();

        services.AddFastEndpoints(endpointDiscoveryOptions => endpointDiscoveryOptions.Assemblies = [ typeof(Program).Assembly ]);

        services.AddOpenApi();

        services.SwaggerDocument(documentOptions =>
        {
            documentOptions.EnableJWTBearerAuth = true; 
            documentOptions.SerializerSettings = jsonSerializerOptions =>
            {
                jsonSerializerOptions.PropertyNamingPolicy = null;
                jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            };
            documentOptions.MaxEndpointVersion = 1;
            documentOptions.MinEndpointVersion = 1;
            documentOptions.DocumentSettings = aspNetCoreOpenApiDocumentGeneratorSettings =>
            {
                aspNetCoreOpenApiDocumentGeneratorSettings.DocumentName = "v1";
                aspNetCoreOpenApiDocumentGeneratorSettings.Title = "Lumina API v1";
                aspNetCoreOpenApiDocumentGeneratorSettings.Version = "v1";
            };
            documentOptions.RemoveEmptyRequestSchema = true;
            documentOptions.ShortSchemaNames = true;
        }); // for an eventual next version:
        //.SwaggerDocument(documentOptions => 
        //{
        //    documentOptions.MaxEndpointVersion = 2;
        //    documentOptions.MinEndpointVersion = 2;
        //    documentOptions.DocumentSettings = aspNetCoreOpenApiDocumentGeneratorSettings =>
        //    {
        //        aspNetCoreOpenApiDocumentGeneratorSettings.DocumentName = "v2";
        //        aspNetCoreOpenApiDocumentGeneratorSettings.Title = "Lumina API v2";
        //        aspNetCoreOpenApiDocumentGeneratorSettings.Version = "v2";
        //    };
        //});

        services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy("SecurePolicy", 
                corsPolicyBuilder => corsPolicyBuilder
                    .WithOrigins(configuration.GetValue<string[]>("CorsSettings:AllowedOrigins") ?? [])
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        services.AddSignalR()
            .AddMessagePackProtocol(messagePackHubProtocolOptions => 
            messagePackHubProtocolOptions.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));
        
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

        return services;
    }
}
