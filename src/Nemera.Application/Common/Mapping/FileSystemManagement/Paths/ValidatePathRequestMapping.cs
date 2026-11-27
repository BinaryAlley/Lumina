#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Extension methods for converting <see cref="ValidatePathRequest"/>.
/// </summary>
public static class ValidatePathRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="ValidatePathQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static ValidatePathQuery ToQuery(this ValidatePathRequest request)
    {
        return new ValidatePathQuery(
            request.Path
        );
    }
}
