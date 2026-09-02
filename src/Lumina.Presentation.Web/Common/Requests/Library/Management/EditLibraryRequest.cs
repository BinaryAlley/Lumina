#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Management;

/// <summary>
/// Represents the request for displaying the media library editing view.
/// </summary>
/// <param name="Id">The unique identifier of the media library to edit. Required.</param>
[DebuggerDisplay("Id: {Id}")]
public record EditLibraryRequest(
    Guid Id
);
