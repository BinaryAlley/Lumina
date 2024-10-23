#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Extension methods for converting <see cref="SplitPathRequest"/>.
/// </summary>
public static class SplitPathRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="SplitPathCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static SplitPathCommand ToCommand(this SplitPathRequest request)
    {
        return new SplitPathCommand(
            request.Path
        );
    }
}
