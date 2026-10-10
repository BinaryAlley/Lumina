#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for an alternative name of a music artist.
/// </summary>
/// <param name="Name">The name of the alias.</param>
/// <param name="SortName">The sort name of the alias, if applicable.</param>
/// <param name="Type">The type of the alias, if applicable.</param>
/// <param name="Locale">The locale the alias is used in, if applicable.</param>
/// <param name="IsPrimary">Whether this is the primary alias of the artist.</param>
/// <param name="BeginDate">The date the alias began being used, if applicable.</param>
/// <param name="EndDate">The date the alias stopped being used, if applicable.</param>
/// <param name="IsEnded">Whether the alias is no longer used.</param>
[DebuggerDisplay("Name: {Name}")]
public record ArtistAliasEntity(
    string Name,
    string? SortName,
    string? Type,
    string? Locale,
    bool IsPrimary,
    DateOnly? BeginDate,
    DateOnly? EndDate,
    bool IsEnded
);
