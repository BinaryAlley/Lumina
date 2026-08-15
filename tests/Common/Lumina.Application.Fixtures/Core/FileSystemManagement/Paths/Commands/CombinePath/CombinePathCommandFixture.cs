#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Commands.CombinePath;

/// <summary>
/// Fixture class for the <see cref="CombinePathCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathCommandFixture
{
    /// <summary>
    /// Creates a random valid command to combine paths.
    /// </summary>
    /// <param name="originalPath">Optional. The original path.</param>
    /// <param name="newPath">Optional. The new path to combine.</param>
    /// <returns>The created command.</returns>
    public CombinePathCommand Create(string? originalPath = null, string? newPath = null)
    {
        return new Faker<CombinePathCommand>()
            .CustomInstantiator(f => new CombinePathCommand(
                default!,
                default!))
            .RuleFor(x => x.OriginalPath, f => originalPath ?? f.System.FilePath())
            .RuleFor(x => x.NewPath, f => newPath ?? f.System.FilePath());
    }

    /// <summary>
    /// Creates a list of <see cref="CombinePathCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<CombinePathCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
