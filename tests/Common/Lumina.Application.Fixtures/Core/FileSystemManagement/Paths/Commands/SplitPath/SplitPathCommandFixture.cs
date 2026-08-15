#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Commands.SplitPath;

/// <summary>
/// Fixture class for the <see cref="SplitPathCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathCommandFixture
{
    /// <summary>
    /// Creates a random valid command for splitting paths.
    /// </summary>
    /// <param name="path">Optional. The file system path.</param>
    /// <returns>The created command.</returns>
    public SplitPathCommand Create(string? path = null)
    {
        return new Faker<SplitPathCommand>()
            .CustomInstantiator(f => new SplitPathCommand(
                default!))
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath());
    }

    /// <summary>
    /// Creates a list of <see cref="SplitPathCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<SplitPathCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
