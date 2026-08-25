#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Enums.Plugins;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;

/// <summary>
/// Fixture class for the <see cref="PluginSettingDescriptorDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingDescriptorDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PluginSettingDescriptorDto"/>.
    /// </summary>
    /// <param name="key">Optional. The key of the setting.</param>
    /// <param name="label">Optional. The display label of the setting.</param>
    /// <param name="type">Optional. The type of the setting.</param>
    /// <param name="defaultValue">Optional. The default value of the setting.</param>
    /// <param name="allowedValues">Optional. The list of allowed values of the setting.</param>
    /// <returns>The created <see cref="PluginSettingDescriptorDto"/>.</returns>
    public PluginSettingDescriptorDto Create(
        string? key = null,
        string? label = null,
        PluginSettingType? type = null,
        string? defaultValue = null,
        IReadOnlyList<string>? allowedValues = null)
    {
        return new PluginSettingDescriptorDto
        {
            Key = key ?? _faker.Random.String2(4, 32),
            Label = label ?? _faker.Commerce.ProductName(),
            Type = type ?? _faker.Random.Enum<PluginSettingType>(),
            DefaultValue = defaultValue,
            AllowedValues = allowedValues
        };
    }

    /// <summary>
    /// Creates a list of <see cref="PluginSettingDescriptorDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PluginSettingDescriptorDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
