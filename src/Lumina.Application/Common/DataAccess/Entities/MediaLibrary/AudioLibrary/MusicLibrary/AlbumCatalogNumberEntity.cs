#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for a catalog number of a music release. A release can carry more than one catalog number at the same time.
/// </summary>
/// <param name="CatalogNumber">The catalog number assigned by the label.</param>
[DebuggerDisplay("CatalogNumber: {CatalogNumber}")]
public record AlbumCatalogNumberEntity(
    string CatalogNumber
);
