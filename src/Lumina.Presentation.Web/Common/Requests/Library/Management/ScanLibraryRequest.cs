#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Management;

/// <summary>
/// Represents a request to scan a media library.
/// </summary>
/// <param name="Id">The unique identifier of the media library to be scanned. Required.</param>
[DebuggerDisplay("Id: {Id}")]
public record ScanLibraryRequest(
    Guid Id
);
