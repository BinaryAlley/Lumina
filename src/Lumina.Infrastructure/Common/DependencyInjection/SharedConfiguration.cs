#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
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
        configuration.AddJsonFile("appsettings.shared.development.json", optional: true, reloadOnChange: true);
        configuration.AddEnvironmentVariables(); // environment variables should override the configuration files

        // bind the appsettings sections
        services.AddOptions<DatabaseSettingsDto>()
                .Bind(configuration.GetRequiredSection(DatabaseSettingsDto.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<MediaSettingsDto>()
                .Bind(configuration.GetRequiredSection(MediaSettingsDto.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<PluginsSettingsDto>()
                .Bind(configuration.GetRequiredSection(PluginsSettingsDto.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<JwtSettingsDto>()
                .Bind(configuration.GetRequiredSection(JwtSettingsDto.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<CorsSettingsDto>()
                .Bind(configuration.GetRequiredSection(CorsSettingsDto.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<EncryptionSettingsDto>()
                .Bind(configuration.GetRequiredSection(EncryptionSettingsDto.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        services.AddOptions<ThemeEngineOptionsDto>()
                .Bind(configuration.GetRequiredSection(ThemeEngineOptionsDto.SECTION_NAME))
                .ValidateFluently()
                .ValidateOnStart();

        return services;
    }
}
