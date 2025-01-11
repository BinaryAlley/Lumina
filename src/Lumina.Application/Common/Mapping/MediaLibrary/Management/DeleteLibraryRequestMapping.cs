#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="DeleteLibraryRequest"/>.
/// </summary>
public static class DeleteLibraryRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="DeleteLibraryCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static DeleteLibraryCommand ToCommand(this DeleteLibraryRequest request)
    {
        return new DeleteLibraryCommand(
            request.Id
        );
    }
}
