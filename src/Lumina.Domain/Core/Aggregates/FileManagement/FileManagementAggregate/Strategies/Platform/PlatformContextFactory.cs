#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;

/// <summary>
/// Defines a factory for creating platform contexts.
/// </summary>
public class PlatformContextFactory : IPlatformContextFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformContextFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public PlatformContextFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates and returns the appropriate platform context.
    /// </summary>
    /// <typeparam name="TPlatformContext">The type of platform context to create.</typeparam>
    /// <returns>The created platform context.</returns>
    public TPlatformContext CreateStrategy<TPlatformContext>() where TPlatformContext : IPlatformContext
    {
        return _serviceProvider.GetRequiredService<TPlatformContext>() ?? throw new ArgumentException();
    }
}