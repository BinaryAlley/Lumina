#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Telemetry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Common.DependencyInjection;

/// <summary>
/// Contains the OpenTelemetry services of the Presentation API layer.
/// </summary>
[ExcludeFromCodeCoverage]
public static class PresentationApiTelemetryServices
{
    private const string SERVICE_NAME = "Lumina.Api";
    private const string ENTITY_FRAMEWORK_CORE_METER_NAME = "Microsoft.EntityFrameworkCore";

    /// <summary>
    /// Registers the OpenTelemetry tracing and metrics pipelines into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">Application configuration properties.</param>
    /// <param name="environment">The hosting environment of the application.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPresentationApiTelemetryServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        bool isTelemetryEnabled = configuration.GetValue<bool?>("Telemetry:Enabled") ?? true;
        if (!isTelemetryEnabled)
            return services;

        bool isConsoleExporterEnabled = configuration.GetValue<bool?>("Telemetry:ConsoleExporterEnabled") ?? environment.IsDevelopment();
        string? otlpEndpoint = GetOtlpEndpoint(configuration);
        double traceSampleRatio = configuration.GetValue<double?>("Telemetry:TraceSampleRatio") ?? 1.0;
        string serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";

        services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder => resourceBuilder
                .AddService(serviceName: SERVICE_NAME, serviceVersion: serviceVersion)
                .AddEnvironmentVariableDetector()
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment.EnvironmentName
                }))
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder.SetSampler(CreateSampler(traceSampleRatio));
                tracingBuilder.AddSource(ApplicationTelemetry.SOURCE_NAME);
                tracingBuilder.AddAspNetCoreInstrumentation();
                if (isConsoleExporterEnabled)
                    tracingBuilder.AddConsoleExporter();
                if (otlpEndpoint is not null)
                    tracingBuilder.AddOtlpExporter(otlpExporterOptions => otlpExporterOptions.Endpoint = new Uri(otlpEndpoint));
            })
            .WithMetrics(metricsBuilder =>
            {
                metricsBuilder.AddMeter(ApplicationTelemetry.SOURCE_NAME);
                metricsBuilder.AddMeter(ENTITY_FRAMEWORK_CORE_METER_NAME);
                metricsBuilder.AddAspNetCoreInstrumentation();
                metricsBuilder.AddRuntimeInstrumentation();
                if (isConsoleExporterEnabled)
                    metricsBuilder.AddConsoleExporter();
                if (otlpEndpoint is not null)
                    metricsBuilder.AddOtlpExporter(otlpExporterOptions => otlpExporterOptions.Endpoint = new Uri(otlpEndpoint));
            });

        return services;
    }

    /// <summary>
    /// Gets the OTLP exporter endpoint from configuration, falling back to the standard <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> environment variable.
    /// </summary>
    /// <param name="configuration">Application configuration properties.</param>
    /// <returns>The OTLP endpoint, or <see langword="null"/> when no endpoint is configured.</returns>
    private static string? GetOtlpEndpoint(IConfiguration configuration)
    {
        string? endpoint = configuration.GetValue<string?>("Telemetry:Otlp:Endpoint");
        if (string.IsNullOrWhiteSpace(endpoint))
            endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        return string.IsNullOrWhiteSpace(endpoint) ? null : endpoint;
    }

    /// <summary>
    /// Creates the trace sampler used by the API, sampling every trace by default and honoring the configured ratio for production traffic.
    /// </summary>
    /// <param name="traceSampleRatio">The ratio of traces to sample, in the range of zero to one.</param>
    /// <returns>The configured trace sampler.</returns>
    private static Sampler CreateSampler(double traceSampleRatio)
    {
        return traceSampleRatio < 1.0
            ? new ParentBasedSampler(new TraceIdRatioBasedSampler(traceSampleRatio))
            : new AlwaysOnSampler();
    }
}
