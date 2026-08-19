#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Persists and reads the theme settings file that stores the current theme selection.
/// </summary>
public sealed class ThemeSettingsStore
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _settingsPath;
    private readonly ILogger<ThemeSettingsStore> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeSettingsStore"/> class.
    /// </summary>
    /// <param name="webHostEnvironment">The web host environment, used to resolve the content root.</param>
    /// <param name="themeEngineOptions">The theme engine configuration options.</param>
    /// <param name="logger">The logger for this store.</param>
    public ThemeSettingsStore(IWebHostEnvironment webHostEnvironment, IOptions<ThemeEngineOptionsDto> themeEngineOptions, ILogger<ThemeSettingsStore> logger)
    {
        _logger = logger;
        _settingsPath = ResolvePath(webHostEnvironment.ContentRootPath, themeEngineOptions.Value.SettingsPath);
    }

    /// <summary>
    /// Reads the unique identifier of the currently selected theme.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The current theme identifier, or <see langword="null"/> when no selection is persisted.</returns>
    public async Task<string?> GetCurrentThemeIdAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_settingsPath))
                return null;

            try
            {
                string json = await File.ReadAllTextAsync(_settingsPath, cancellationToken);
                return JsonSerializer.Deserialize<PersistedThemeSettingsDto>(json, s_jsonOptions)?.CurrentThemeId;
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Ignoring an invalid theme settings file at {SettingsPath}.", _settingsPath);
                return null;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Persists the unique identifier of the currently selected theme.
    /// </summary>
    /// <param name="themeId">The theme unique identifier to persist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SetCurrentThemeIdAsync(string themeId, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            string? directory = Path.GetDirectoryName(_settingsPath) ?? throw new InvalidOperationException("The theme settings path has no parent directory.");
            Directory.CreateDirectory(directory);

            string temporaryPath = Path.Combine(directory, $".theme-settings-{Guid.NewGuid():N}.tmp");
            try
            {
                string json = JsonSerializer.Serialize(new PersistedThemeSettingsDto { CurrentThemeId = themeId }, s_jsonOptions);
                await File.WriteAllTextAsync(temporaryPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);
                File.Move(temporaryPath, _settingsPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Resolves a configured path to an absolute path, rooted against the content root when relative.
    /// </summary>
    /// <param name="contentRoot">The content root of the application.</param>
    /// <param name="configuredPath">The path from the configuration.</param>
    /// <returns>The absolute path.</returns>
    private static string ResolvePath(string contentRoot, string configuredPath)
    {
        return Path.GetFullPath(Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(contentRoot, configuredPath));
    }
}
