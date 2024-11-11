#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
#endregion

namespace Lumina.Presentation.Api.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Presentation API layer.
/// </summary>
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

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                var problem = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.29",
                    title = "TooManyRequests", // TODO: implement translation?
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Too many attempts. Please try again later.",
                    retryAfter = "15"
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            };
            options.AddFixedWindowLimiter("authenticationPolicy", config =>
            {
                config.PermitLimit = 10;  // allow 10 attempts
                config.Window = TimeSpan.FromMinutes(15);  // within 15 minute window
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                config.QueueLimit = 0;  // don't queue requests
            });
        });

        // add authentication and specify the JWT scheme to check tokens against
        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context => // event triggered when authentication is not successful for whatever reason
                    {
                        context.HandleResponse(); // prevent the default 401 response
                        // set the response status code to 401 Unauthorized
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";
                        
                        IProblemDetailsService problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                        return context.Response.WriteAsJsonAsync(new
                        {
                            type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                            status = StatusCodes.Status401Unauthorized,
                            title = "Unauthorized",
                            detail = "You are not authorized",
                            instance = context.HttpContext.Request.Path
                        });
                    }
                };
            });

        services.AddAuthorization();

        services.AddFastEndpoints(
        o => o.Assemblies =
        [
            typeof(Program).Assembly
        ]);
        services.SwaggerDocument(documentOptions =>
        {
            documentOptions.SerializerSettings = s =>
            {
                s.PropertyNamingPolicy = null;
                s.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            };
            documentOptions.MaxEndpointVersion = 1;
            documentOptions.MinEndpointVersion = 1;
            documentOptions.DocumentSettings = s =>
            {
                s.DocumentName = "Release 1.0";
                s.Title = "Lumina API";
                s.Version = "v1.0";
            };
            documentOptions.RemoveEmptyRequestSchema = true;
            documentOptions.ShortSchemaNames = true;            
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                 builder => builder
                     .AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader());
        });

        return services;
    }
}
