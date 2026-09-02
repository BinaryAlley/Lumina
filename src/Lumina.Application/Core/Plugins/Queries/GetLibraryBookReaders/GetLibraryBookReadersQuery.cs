#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;

/// <summary>
/// Query for getting the book readers configured for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose book readers are retrieved.</param>
public record GetLibraryBookReadersQuery(
    Guid LibraryId
) : IQuery;
