#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Management;

/// <summary>
/// Represents the request for retrieving the book readers of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose book readers are retrieved. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetBookReadersRequest(
    Guid LibraryId
);
