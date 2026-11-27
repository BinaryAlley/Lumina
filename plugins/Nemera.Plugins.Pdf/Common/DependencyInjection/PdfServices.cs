#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Reading;
using Lumina.Plugins.Pdf.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.Plugins.Pdf.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the PDF format reader into the Dependency Injection container.
/// </summary>
internal static class PdfServices
{
    /// <summary>
    /// Registers the services of the PDF format reader into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="pluginId">The unique identifier of the plugin that provides the book reader.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    internal static IServiceCollection AddPdfReader(this IServiceCollection services, Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register the book reader as a keyed by pluginId transient service, so it can be resolved specifically among other IBookReader implementations.
        services.AddKeyedTransient<IBookReader, PdfReader>(pluginId);

        return services;
    }
}
