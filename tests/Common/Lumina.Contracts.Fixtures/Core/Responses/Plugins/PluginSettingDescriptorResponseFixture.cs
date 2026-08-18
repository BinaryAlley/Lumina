#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Plugins;

/// <summary>
/// Fixture class for the <see cref="PluginSettingDescriptorResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingDescriptorResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PluginSettingDescriptorResponse"/>.
    /// </summary>
    /// <param name="key">Optional. The unique key of the setting.</param>
    /// <param name="label">Optional. The display label of the setting.</param>
    /// <param name="type">Optional. The type of the setting.</param>
    /// <param name="defaultValue">Optional. The default value of the setting.</param>
    /// <param name="allowedValues">Optional. The list of allowed values, when the setting is a selection.</param>
    /// <returns>The created <see cref="PluginSettingDescriptorResponse"/>.</returns>
    public PluginSettingDescriptorResponse Create(
        string? key = null,
        string? label = null,
        PluginSettingType? type = null,
        string? defaultValue = null,
        IReadOnlyList<string>? allowedValues = null)
    {
        return new PluginSettingDescriptorResponse(
            key ?? _faker.Lorem.Word(),
            label ?? _faker.Lorem.Word(),
            type ?? _faker.PickRandom<PluginSettingType>(),
            defaultValue,
            allowedValues ?? [_faker.Lorem.Word(), _faker.Lorem.Word()]
        );
    }

    /// <summary>
    /// Creates a list of <see cref="PluginSettingDescriptorResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PluginSettingDescriptorResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
