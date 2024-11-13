#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Commands.CombinePath.Fixtures;

/// <summary>
/// Fixture class for the <see cref="CombinePathCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathCommandFixture
{
    /// <summary>
    /// Creates a random valid command to combine paths.
    /// </summary>
    /// <returns>The created command.</returns>
    public static CombinePathCommand CreateCombinePathCommand()
    {
        return new Faker<CombinePathCommand>()
            .CustomInstantiator(f => new CombinePathCommand(
                default!,
                default!
            ))
            .RuleFor(x => x.OriginalPath, f => f.System.FilePath())
            .RuleFor(x => x.NewPath, f => f.System.FilePath());
    }
}
