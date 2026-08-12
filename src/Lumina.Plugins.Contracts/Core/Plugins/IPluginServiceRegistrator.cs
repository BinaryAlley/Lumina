#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.DependencyInjection;
#endregion

namespace Lumina.Plugins.Contracts.Core.Plugins;

/// <summary>
/// Contract for registering the services of a plugin into the host dependency injection container.
/// </summary>
public interface IPluginServiceRegistrator
{
    /// <summary>
    /// Registers the services of the plugin into the host dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to which the plugin services are added.</param>
    void RegisterServices(IServiceCollection services);
}
