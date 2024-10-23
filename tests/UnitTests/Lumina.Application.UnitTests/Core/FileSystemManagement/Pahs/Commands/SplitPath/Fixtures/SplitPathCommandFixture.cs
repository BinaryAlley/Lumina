#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Commands.SplitPath.Fixtures;

/// <summary>
/// Fixture class for the <see cref="SplitPathCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathCommandFixture
{
    /// <summary>
    /// Creates a random valid command for splitting paths.
    /// </summary>
    /// <returns>The created command.</returns>
    public static SplitPathCommand CreateSplitPathCommand()
    {
        return new Faker<SplitPathCommand>()
            .CustomInstantiator(f => new SplitPathCommand(
                default!
            ))
            .RuleFor(x => x.Path, f => f.System.FilePath());
    }
}
