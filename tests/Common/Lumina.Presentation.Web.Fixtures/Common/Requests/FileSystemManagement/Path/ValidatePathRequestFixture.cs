#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.FileSystemManagement.Path;

/// <summary>
/// Fixture class for generating <see cref="ValidatePathRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="ValidatePathRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="path">Optional file system path to validate.</param>
    /// <returns>A configured <see cref="ValidatePathRequest"/> instance.</returns>
    public ValidatePathRequest Create(string? path = null)
    {
        return new ValidatePathRequest(
            Path: path ?? $"/media/{System.Guid.NewGuid():N}"
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ValidatePathRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ValidatePathRequest"/> instances.</returns>
    public List<ValidatePathRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
