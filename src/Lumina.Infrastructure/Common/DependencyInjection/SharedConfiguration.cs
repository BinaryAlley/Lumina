#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.Configuration;
using Lumina.Infrastructure.Common.Models.Configuration;
using Lumina.Infrastructure.Common.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
#endregion

namespace Lumina.Infrastructure.Common.DependencyInjection;

/// <summary>
/// Class containing the shared configuration bindings of the application.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SharedConfiguration
{
    /// <summary>
    /// Extension method for registering the shared configuration to the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the configuration to.</param>
    /// <param name="configuration">The configuration manager to use for the configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the base path where the configuration files should be located, does not exist.</exception>
    public static IServiceCollection BindSharedConfiguration(this IServiceCollection services, IConfigurationManager configuration)
    {
        string? basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"The base path '{basePath}' does not exist.");
        configuration.SetBasePath(basePath);
        configuration.AddJsonFile("appsettings.shared.json", optional: false, reloadOnChange: true);
        configuration.AddJsonFile("appsettings.shared.Development.json", optional: true, reloadOnChange: true);
        configuration.AddEnvironmentVariables(); // environment variables should override the configuration files

        // bind the appsettings sections
        services.AddOptions<CommonSettingsModel>()
                .Bind(configuration.GetRequiredSection(CommonSettingsModel.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<DatabaseSettingsModel>()
                .Bind(configuration.GetRequiredSection(DatabaseSettingsModel.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<MediaSettingsModel>()
                .Bind(configuration.GetRequiredSection(MediaSettingsModel.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<JwtSettingsModel>()
                .Bind(configuration.GetRequiredSection(JwtSettingsModel.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<CorsSettingsModel>()
                .Bind(configuration.GetRequiredSection(CorsSettingsModel.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<EncryptionSettingsModel>()
                .Bind(configuration.GetRequiredSection(EncryptionSettingsModel.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        return services;
    }
}
