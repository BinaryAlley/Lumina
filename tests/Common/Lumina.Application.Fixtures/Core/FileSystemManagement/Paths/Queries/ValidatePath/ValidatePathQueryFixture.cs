#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Queries.ValidatePath;

/// <summary>
/// Fixture class for the <see cref="ValidatePathQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathQueryFixture
{
    /// <summary>
    /// Creates a random valid query to validate a path.
    /// </summary>
    /// <param name="path">Optional. The file system path.</param>
    /// <returns>The created query.</returns>
    public ValidatePathQuery Create(string? path = null)
    {
        return new Faker<ValidatePathQuery>()
            .CustomInstantiator(f => new ValidatePathQuery(
                default!))
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath());
    }

    /// <summary>
    /// Creates a list of <see cref="ValidatePathQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ValidatePathQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
