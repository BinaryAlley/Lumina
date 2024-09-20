#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Models.Configuration;
using Lumina.Infrastructure.Common.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
#endregion

namespace Lumina.Infrastructure.Common.DependencyInjection;

/// <summary>
/// Class containing the shared configuration bindings of the application.
/// </summary>
public static class SharedConfiguration
{
    #region ================================================================== METHODS ===================================================================================
    /// <summary>
    /// Extension method for registering the shared configuration to the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the configuration to.</param>
    /// <param name="configuration">The configuration manager to use for the configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the base path where the configuration files should be located, does not exist.</exception>
    public static IServiceCollection BindSharedConfiguration(this IServiceCollection services, IConfigurationManager configuration)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"The base path '{basePath}' does not exist.");
        configuration.SetBasePath(basePath);
        configuration.AddJsonFile("appsettings.shared.json", optional: false, reloadOnChange: true);
        configuration.AddJsonFile("appsettings.shared.Development.json", optional: true, reloadOnChange: true);
        configuration.AddEnvironmentVariables(); // environment variables should override the configuration files
        
        // bind the common settings section
        services.AddOptions<CommonSettingsModel>()
                .Bind(configuration.GetRequiredSection(CommonSettingsModel.SectionName))
                .ValidateFluently()
                .ValidateOnStart();

        // bind the database connection string section
        services.AddOptions<DatabaseSettingsModel>()
                .Bind(configuration.GetRequiredSection(DatabaseSettingsModel.SectionName))
                .ValidateFluently()
                .ValidateOnStart(); 
        return services;
    }
    #endregion
}