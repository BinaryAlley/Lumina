#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to get the book readers configured for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose book readers are retrieved. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public sealed record GetLibraryBookReadersRequest(
    Guid LibraryId
);
