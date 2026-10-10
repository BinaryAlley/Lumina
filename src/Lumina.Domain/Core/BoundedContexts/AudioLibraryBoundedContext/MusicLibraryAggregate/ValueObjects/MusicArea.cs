#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for a MusicBrainz area, the geographic area a music artist is associated with.
/// </summary>
[DebuggerDisplay("{Name}")]
public sealed class MusicArea : ValueObject
{
    /// <summary>
    /// Gets the MusicBrainz identifier of the area.
    /// </summary>
    public MusicBrainzId MusicBrainzAreaId { get; }

    /// <summary>
    /// Gets the name of the area.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional sort name of the area.
    /// </summary>
    public Optional<string> SortName { get; }

    /// <summary>
    /// Gets the optional disambiguation comment of the area.
    /// </summary>
    public Optional<string> Disambiguation { get; }

    /// <summary>
    /// Gets the optional MusicBrainz type of the area (e.g., Country, City).
    /// </summary>
    public Optional<string> Type { get; }

    /// <summary>
    /// Gets the optional ISO 3166 code of the area.
    /// </summary>
    public Optional<string> Iso3166Code { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArea"/> class.
    /// </summary>
    /// <param name="musicBrainzAreaId">The MusicBrainz identifier of the area.</param>
    /// <param name="name">The name of the area.</param>
    /// <param name="sortName">The optional sort name of the area.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the area.</param>
    /// <param name="type">The optional MusicBrainz type of the area.</param>
    /// <param name="iso3166Code">The optional ISO 3166 code of the area.</param>
    private MusicArea(
        MusicBrainzId musicBrainzAreaId,
        string name,
        Optional<string> sortName,
        Optional<string> disambiguation,
        Optional<string> type,
        Optional<string> iso3166Code)
    {
        MusicBrainzAreaId = musicBrainzAreaId;
        Name = name;
        SortName = sortName;
        Disambiguation = disambiguation;
        Type = type;
        Iso3166Code = iso3166Code;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicArea"/> class.
    /// </summary>
    /// <param name="musicBrainzAreaId">The MusicBrainz identifier of the area.</param>
    /// <param name="name">The name of the area.</param>
    /// <param name="sortName">The optional sort name of the area.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the area.</param>
    /// <param name="type">The optional MusicBrainz type of the area.</param>
    /// <param name="iso3166Code">The optional ISO 3166 code of the area.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="MusicArea"/>, or an error message.
    /// </returns>
    public static Result<MusicArea> Create(
        MusicBrainzId musicBrainzAreaId,
        string name,
        Optional<string> sortName,
        Optional<string> disambiguation,
        Optional<string> type,
        Optional<string> iso3166Code)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Music.AreaNameCannotBeEmpty;

        return new MusicArea(musicBrainzAreaId, name.Trim(), sortName, disambiguation, type, iso3166Code);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return MusicBrainzAreaId;
        yield return Name;
        yield return SortName;
        yield return Disambiguation;
        yield return Type;
        yield return Iso3166Code;
    }
}
