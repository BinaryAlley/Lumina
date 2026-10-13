#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Reflection;
#endregion

namespace Lumina.DataAccess.Core.Repositories.MusicLibrary.Specifications;

/// <summary>
/// Represents a filter specification for filtering artists by the alpha key of their name, for the alpha picker.
/// The alpha key is the first character of the name, lowercased, and optionally computed after ignoring a leading "The " prefix.
/// </summary>
internal sealed class ArtistAlphaFilterSpecification : FilterSpecification<ArtistEntity>
{
    private static readonly MethodInfo s_toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
    private static readonly MethodInfo s_startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
    private static readonly MethodInfo s_substringMethod = typeof(string).GetMethod(nameof(string.Substring), [typeof(int)])!;
    private static readonly MethodInfo s_globMethod = typeof(SqliteDbFunctionsExtensions)
        .GetMethod(nameof(SqliteDbFunctionsExtensions.Glob), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private readonly string? _filterAlphaKey;
    private readonly bool _shouldIgnoreThePrefixForAlphaPicker;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtistAlphaFilterSpecification"/> class.
    /// </summary>
    /// <param name="filterAlphaKey">The alpha key to filter by. A single ASCII letter, <see cref="LibraryItemAlphaKeys.NUMBER"/>, or <see cref="LibraryItemAlphaKeys.SYMBOL"/>.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the leading "The " prefix of a name should be ignored when computing the alpha key, or not.</param>
    public ArtistAlphaFilterSpecification(string? filterAlphaKey, bool shouldIgnoreThePrefixForAlphaPicker)
    {
        _filterAlphaKey = filterAlphaKey;
        _shouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker;
    }

    /// <summary>
    /// Creates a LINQ expression that represents the predicate defined by the current specification.
    /// </summary>
    /// <returns>An expression tree that can be used to evaluate whether an <see cref="ArtistEntity"/> satisfies the specification criteria.</returns>
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        // When no alpha key was requested, every artist matches.
        if (_filterAlphaKey is null)
            return artist => true;

        ParameterExpression artist = Expression.Parameter(typeof(ArtistEntity), "artist");
        Expression effectiveName = BuildEffectiveName(artist);

        if (_filterAlphaKey == LibraryItemAlphaKeys.NUMBER)
            return BuildGlobPredicate(artist, effectiveName, "[0-9]*");

        if (_filterAlphaKey == LibraryItemAlphaKeys.SYMBOL)
            return BuildSymbolPredicate(artist, effectiveName);

        // The alpha key is a single ASCII letter.
        string globPattern = char.ToLowerInvariant(_filterAlphaKey[0]) + "*";
        return BuildGlobPredicate(artist, effectiveName, globPattern);
    }

    /// <summary>
    /// Builds the expression representing the lowercased name used to derive the alpha key, falling back to an empty string
    /// when the name is <see langword="null"/> or empty, and optionally stripped of a leading "the " prefix.
    /// </summary>
    /// <param name="artist">The parameter representing the artist entity.</param>
    /// <returns>The expression of the effective name.</returns>
    private Expression BuildEffectiveName(ParameterExpression artist)
    {
        Expression nameProperty = Expression.Property(artist, nameof(ArtistEntity.Name));

        // The raw name is the name, unless it is null, in which case an empty string is used.
        Expression rawName = Expression.Coalesce(nameProperty, Expression.Constant(string.Empty));

        MethodCallExpression lowerName = Expression.Call(rawName, s_toLowerMethod);

        // When ignoring the "The " prefix, a leading "the " is stripped from the lowercased name.
        Expression effectiveName = lowerName;
        if (_shouldIgnoreThePrefixForAlphaPicker)
        {
            MethodCallExpression startsWithThe = Expression.Call(lowerName, s_startsWithMethod, Expression.Constant("the "));
            MethodCallExpression strippedName = Expression.Call(lowerName, s_substringMethod, Expression.Constant(4));
            effectiveName = Expression.Condition(startsWithThe, strippedName, lowerName);
        }

        return effectiveName;
    }

    /// <summary>
    /// Builds a predicate that is satisfied when the effective name starts with a glob pattern.
    /// </summary>
    /// <param name="artist">The parameter representing the artist entity.</param>
    /// <param name="effectiveName">The expression of the effective name.</param>
    /// <param name="globPattern">The glob pattern the effective name must match.</param>
    /// <returns>The built predicate.</returns>
    private static Expression<Func<ArtistEntity, bool>> BuildGlobPredicate(ParameterExpression artist, Expression effectiveName, string globPattern)
    {
        MethodCallExpression glob = BuildGlobCall(effectiveName, globPattern);
        return Expression.Lambda<Func<ArtistEntity, bool>>(glob, artist);
    }

    /// <summary>
    /// Builds a predicate that is satisfied when the first character of the effective name is neither a letter nor a digit.
    /// </summary>
    /// <param name="artist">The parameter representing the artist entity.</param>
    /// <param name="effectiveName">The expression of the effective name.</param>
    /// <returns>The built predicate.</returns>
    private static Expression<Func<ArtistEntity, bool>> BuildSymbolPredicate(ParameterExpression artist, Expression effectiveName)
    {
        Expression notLetter = Expression.Not(BuildGlobCall(effectiveName, "[a-z]*"));
        Expression notDigit = Expression.Not(BuildGlobCall(effectiveName, "[0-9]*"));
        return Expression.Lambda<Func<ArtistEntity, bool>>(Expression.AndAlso(notLetter, notDigit), artist);
    }

    /// <summary>
    /// Builds the call to <see cref="SqliteDbFunctionsExtensions.Glob"/> for the effective name and the provided glob pattern.
    /// </summary>
    /// <param name="effectiveName">The expression of the effective name.</param>
    /// <param name="globPattern">The glob pattern the effective name must match.</param>
    /// <returns>The built method call.</returns>
    private static MethodCallExpression BuildGlobCall(Expression effectiveName, string globPattern)
    {
        Expression functions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        return Expression.Call(s_globMethod, functions, effectiveName, Expression.Constant(globPattern));
    }
}
