#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Plugins.Commands.InstallPlugin;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Commands.InstallPlugin;

/// <summary>
/// Fixture class for the <see cref="InstallPluginCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid <see cref="InstallPluginCommand"/>.
    /// </summary>
    /// <param name="archive">Optional. The archive stream of the uploaded plugin.</param>
    /// <param name="fileName">Optional. The file name of the uploaded plugin.</param>
    /// <returns>The created <see cref="InstallPluginCommand"/>.</returns>
    public InstallPluginCommand Create(
        Stream? archive = null, 
        string? fileName = null)
    {
        return new InstallPluginCommand(
            archive ?? new MemoryStream(_faker.Random.Bytes(64)),
            fileName ?? $"{_faker.Hacker.Noun()}.dll");
    }

    /// <summary>
    /// Creates a list of <see cref="InstallPluginCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="InstallPluginCommand"/> instances.</returns>
    public List<InstallPluginCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
