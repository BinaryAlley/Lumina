#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for an alternative name of a music artist.
/// </summary>
[DebuggerDisplay("{Name}")]
public sealed class MusicArtistAlias : ValueObject
{
    /// <summary>
    /// Gets the name of the alias.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional sort name of the alias.
    /// </summary>
    public Optional<string> SortName { get; }

    /// <summary>
    /// Gets the optional type of the alias (e.g., Artist name, Search hint).
    /// </summary>
    public Optional<string> Type { get; }

    /// <summary>
    /// Gets the optional locale the alias is used in.
    /// </summary>
    public Optional<string> Locale { get; }

    /// <summary>
    /// Gets a value indicating whether this is the primary alias of the artist.
    /// </summary>
    public bool IsPrimary { get; }

    /// <summary>
    /// Gets the optional date the alias began being used.
    /// </summary>
    public Optional<DateOnly> BeginDate { get; }

    /// <summary>
    /// Gets the optional date the alias stopped being used.
    /// </summary>
    public Optional<DateOnly> EndDate { get; }

    /// <summary>
    /// Gets a value indicating whether the alias is no longer used.
    /// </summary>
    public bool IsEnded { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtistAlias"/> class.
    /// </summary>
    /// <param name="name">The name of the alias.</param>
    /// <param name="sortName">The optional sort name of the alias.</param>
    /// <param name="type">The optional type of the alias.</param>
    /// <param name="locale">The optional locale the alias is used in.</param>
    /// <param name="isPrimary">Whether this is the primary alias of the artist.</param>
    /// <param name="beginDate">The optional date the alias began being used.</param>
    /// <param name="endDate">The optional date the alias stopped being used.</param>
    /// <param name="isEnded">Whether the alias is no longer used.</param>
    private MusicArtistAlias(
        string name,
        Optional<string> sortName,
        Optional<string> type,
        Optional<string> locale,
        bool isPrimary,
        Optional<DateOnly> beginDate,
        Optional<DateOnly> endDate,
        bool isEnded)
    {
        Name = name;
        SortName = sortName;
        Type = type;
        Locale = locale;
        IsPrimary = isPrimary;
        BeginDate = beginDate;
        EndDate = endDate;
        IsEnded = isEnded;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicArtistAlias"/> class.
    /// </summary>
    /// <param name="name">The name of the alias.</param>
    /// <param name="sortName">The optional sort name of the alias.</param>
    /// <param name="type">The optional type of the alias.</param>
    /// <param name="locale">The optional locale the alias is used in.</param>
    /// <param name="isPrimary">Whether this is the primary alias of the artist.</param>
    /// <param name="beginDate">The optional date the alias began being used.</param>
    /// <param name="endDate">The optional date the alias stopped being used.</param>
    /// <param name="isEnded">Whether the alias is no longer used.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="MusicArtistAlias"/>, or an error message.
    /// </returns>
    public static Result<MusicArtistAlias> Create(
        string name,
        Optional<string> sortName,
        Optional<string> type,
        Optional<string> locale,
        bool isPrimary,
        Optional<DateOnly> beginDate,
        Optional<DateOnly> endDate,
        bool isEnded)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Music.ArtistAliasNameCannotBeEmpty;

        return new MusicArtistAlias(name.Trim(), sortName, type, locale, isPrimary, beginDate, endDate, isEnded);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return SortName;
        yield return Type;
        yield return Locale;
        yield return IsPrimary;
        yield return BeginDate;
        yield return EndDate;
        yield return IsEnded;
    }
}
