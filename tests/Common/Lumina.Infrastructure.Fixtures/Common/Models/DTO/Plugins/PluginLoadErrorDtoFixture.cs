#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Infrastructure.Common.Models.DTO.Plugins;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Models.DTO.Plugins;

/// <summary>
/// Fixture class for the <see cref="PluginLoadErrorDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
internal class PluginLoadErrorDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid <see cref="PluginLoadErrorDto"/>.
    /// </summary>
    /// <param name="assemblyName">Optional. The file name of the plugin assembly that failed to load, without its extension.</param>
    /// <param name="errorMessage">Optional. The error message describing the load failure.</param>
    /// <returns>The created <see cref="PluginLoadErrorDto"/>.</returns>
    public PluginLoadErrorDto Create(string? assemblyName = null, string? errorMessage = null)
    {
        return new PluginLoadErrorDto(
            assemblyName ?? _faker.Hacker.Noun(),
            errorMessage ?? $"Failed to load plugin assembly '{_faker.Hacker.Noun()}.dll': {_faker.Lorem.Sentence()}");
    }

    /// <summary>
    /// Creates a list of <see cref="PluginLoadErrorDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PluginLoadErrorDto"/> instances.</returns>
    public List<PluginLoadErrorDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
