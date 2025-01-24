#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Scanners.Common;

/// <summary>
/// Interface defining a media library scanner for a specific media library type.
/// </summary>
internal interface IMediaTypeScanner
{
    /// <summary>
    /// The media library type that this media library scanner supports.
    /// </summary>
    LibraryType SupportedType { get; }

    /// <summary>
    /// Creates the media library scan jobs for the provided media library.
    /// </summary>
    /// <param name="library">The media library for which to create the media library scan jobs.</param>
    /// <returns>A collection of media library scan jobs.</returns>
    IEnumerable<MediaScanJob> CreateScanJobsForLibrary(Library library);
}
