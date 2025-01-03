#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;

/// <summary>
/// Command for adding a media library.
/// </summary>
/// <param name="Title">The title of the media library.</param>
/// <param name="LibraryType">The type of the media library.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located.</param>
/// <param name="CoverImage">The path of the image file used as the cover for the library.</param>
[DebuggerDisplay("Title: {Title}")]
public record AddLibraryCommand(
    string? Title,
    string? LibraryType,
    string[]? ContentLocations,
    string? CoverImage
) : IRequest<ErrorOr<LibraryResponse>>;
