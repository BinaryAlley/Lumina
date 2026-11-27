#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for a type of a music release. A release can carry more than one type at the same time.
/// </summary>
/// <param name="ReleaseType">The type of the release.</param>
[DebuggerDisplay("ReleaseType: {ReleaseType}")]
public record AlbumReleaseTypeEntity(
    MusicReleaseType ReleaseType
);
