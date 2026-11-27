#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Extension methods for converting <see cref="CombinePathRequest"/>.
/// </summary>
public static class CombinePathRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="CombinePathCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static CombinePathCommand ToCommand(this CombinePathRequest request)
    {
        return new CombinePathCommand(
            request.OriginalPath,
            request.NewPath
        );
    }
}
