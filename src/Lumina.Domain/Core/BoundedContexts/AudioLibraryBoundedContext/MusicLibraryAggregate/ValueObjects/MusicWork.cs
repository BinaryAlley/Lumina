#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for a MusicBrainz work, the musical composition a track is a recording of.
/// </summary>
[DebuggerDisplay("{Title}")]
public sealed class MusicWork : ValueObject
{
    private readonly List<LanguageInfo> _languages;
    private readonly List<string> _iswcs;

    /// <summary>
    /// Gets the MusicBrainz identifier of the work.
    /// </summary>
    public MusicBrainzId MusicBrainzWorkId { get; }

    /// <summary>
    /// Gets the title of the work.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the optional MusicBrainz type of the work (e.g., Song).
    /// </summary>
    public Optional<string> Type { get; }

    /// <summary>
    /// Gets the list of languages of the work.
    /// </summary>
    public IReadOnlyCollection<LanguageInfo> Languages => _languages.AsReadOnly();

    /// <summary>
    /// Gets the list of ISWC (International Standard Musical Work Code) of the work.
    /// </summary>
    public IReadOnlyCollection<string> Iswcs => _iswcs.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicWork"/> class.
    /// </summary>
    /// <param name="musicBrainzWorkId">The MusicBrainz identifier of the work.</param>
    /// <param name="title">The title of the work.</param>
    /// <param name="type">The optional MusicBrainz type of the work.</param>
    /// <param name="languages">The list of languages of the work.</param>
    /// <param name="iswcs">The list of ISWC of the work.</param>
    private MusicWork(
        MusicBrainzId musicBrainzWorkId,
        string title,
        Optional<string> type,
        List<LanguageInfo> languages,
        List<string> iswcs)
    {
        MusicBrainzWorkId = musicBrainzWorkId;
        Title = title;
        Type = type;
        _languages = languages;
        _iswcs = iswcs;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicWork"/> class.
    /// </summary>
    /// <param name="musicBrainzWorkId">The MusicBrainz identifier of the work.</param>
    /// <param name="title">The title of the work.</param>
    /// <param name="type">The optional MusicBrainz type of the work.</param>
    /// <param name="languages">The list of languages of the work.</param>
    /// <param name="iswcs">The list of ISWC of the work.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="MusicWork"/>, or an error message.
    /// </returns>
    public static Result<MusicWork> Create(
        MusicBrainzId musicBrainzWorkId,
        string title,
        Optional<string> type,
        List<LanguageInfo> languages,
        List<string> iswcs)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Errors.Music.WorkTitleCannotBeEmpty;

        return new MusicWork(musicBrainzWorkId, title.Trim(), type, languages, iswcs);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return MusicBrainzWorkId;
        yield return Title;
        yield return Type;
        foreach (LanguageInfo language in _languages)
            yield return language;
        foreach (string iswc in _iswcs)
            yield return iswc;
    }
}
