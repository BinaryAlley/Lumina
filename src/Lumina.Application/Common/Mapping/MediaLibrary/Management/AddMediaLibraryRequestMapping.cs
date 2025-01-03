#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="AddLibraryRequest"/>.
/// </summary>
public static class AddMediaLibraryRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddLibraryCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static AddLibraryCommand ToCommand(this AddLibraryRequest request)
    {
        return new AddLibraryCommand(
            request.Title,
            request.LibraryType,
            request.ContentLocations,
            request.CoverImage
        );
    }
}
