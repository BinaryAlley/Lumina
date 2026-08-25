#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Reflection;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Books.Specifications;

/// <summary>
/// Represents a filter specification for filtering books by the alpha key of their title, for the alpha picker.
/// The alpha key is the first character of the title, lowercased, and optionally computed after ignoring a leading "The " prefix.
/// </summary>
internal sealed class BookAlphaFilterSpecification : FilterSpecification<BookEntity>
{
    private static readonly MethodInfo s_toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
    private static readonly MethodInfo s_startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
    private static readonly MethodInfo s_substringMethod = typeof(string).GetMethod(nameof(string.Substring), [typeof(int)])!;
    private static readonly MethodInfo s_globMethod = typeof(SqliteDbFunctionsExtensions)
        .GetMethod(nameof(SqliteDbFunctionsExtensions.Glob), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private readonly string? _filterAlphaKey;
    private readonly bool _shouldIgnoreThePrefixForAlphaPicker;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookAlphaFilterSpecification"/> class.
    /// </summary>
    /// <param name="filterAlphaKey">The alpha key to filter by. A single ASCII letter, <see cref="LibraryItemAlphaKeys.NUMBER"/>, or <see cref="LibraryItemAlphaKeys.SYMBOL"/>.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the leading "The " prefix of a title should be ignored when computing the alpha key, or not.</param>
    public BookAlphaFilterSpecification(string? filterAlphaKey, bool shouldIgnoreThePrefixForAlphaPicker)
    {
        _filterAlphaKey = filterAlphaKey;
        _shouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker;
    }

    /// <summary>
    /// Creates a LINQ expression that represents the predicate defined by the current specification.
    /// </summary>
    /// <returns>An expression tree that can be used to evaluate whether a <see cref="BookEntity"/> satisfies the specification criteria.</returns>
    public override Expression<Func<BookEntity, bool>> ToExpression()
    {
        // when no alpha key was requested, every book matches
        if (_filterAlphaKey is null)
            return book => true;

        ParameterExpression book = Expression.Parameter(typeof(BookEntity), "book");
        Expression effectiveTitle = BuildEffectiveTitle(book);

        if (_filterAlphaKey == LibraryItemAlphaKeys.NUMBER)
            return BuildGlobPredicate(book, effectiveTitle, "[0-9]*");

        if (_filterAlphaKey == LibraryItemAlphaKeys.SYMBOL)
            return BuildSymbolPredicate(book, effectiveTitle);

        // the alpha key is a single ASCII letter
        string globPattern = char.ToLowerInvariant(_filterAlphaKey[0]) + "*";
        return BuildGlobPredicate(book, effectiveTitle, globPattern);
    }

    /// <summary>
    /// Builds the expression representing the lowercased title used to derive the alpha key, falling back to the original title
    /// when the title is <see langword="null"/> or empty, and optionally stripped of a leading "the " prefix.
    /// </summary>
    /// <param name="book">The parameter representing the book entity.</param>
    /// <returns>The expression of the effective title.</returns>
    private Expression BuildEffectiveTitle(ParameterExpression book)
    {
        Expression titleProperty = Expression.Property(book, nameof(BookEntity.Title));

        // the raw title: the title, unless it is null or empty, in which case the original title (or an empty string) is used
        BinaryExpression isTitleMissing = Expression.OrElse(
            Expression.Equal(titleProperty, Expression.Constant(null, typeof(string))),
            Expression.Equal(titleProperty, Expression.Constant(string.Empty)));
        Expression rawTitle = Expression.Condition(isTitleMissing,
            Expression.Coalesce(Expression.Property(book, nameof(BookEntity.OriginalTitle)), Expression.Constant(string.Empty)),
            titleProperty);

        MethodCallExpression lowerTitle = Expression.Call(rawTitle, s_toLowerMethod);

        // when ignoring the "The " prefix, strip a leading "the " from the lowercased title
        Expression effectiveTitle = lowerTitle;
        if (_shouldIgnoreThePrefixForAlphaPicker)
        {
            MethodCallExpression startsWithThe = Expression.Call(lowerTitle, s_startsWithMethod, Expression.Constant("the "));
            MethodCallExpression strippedTitle = Expression.Call(lowerTitle, s_substringMethod, Expression.Constant(4));
            effectiveTitle = Expression.Condition(startsWithThe, strippedTitle, lowerTitle);
        }

        return effectiveTitle;
    }

    /// <summary>
    /// Builds a predicate that is satisfied when the effective title starts with a glob pattern.
    /// </summary>
    /// <param name="book">The parameter representing the book entity.</param>
    /// <param name="effectiveTitle">The expression of the effective title.</param>
    /// <param name="globPattern">The glob pattern the effective title must match.</param>
    /// <returns>The built predicate.</returns>
    private static Expression<Func<BookEntity, bool>> BuildGlobPredicate(ParameterExpression book, Expression effectiveTitle, string globPattern)
    {
        MethodCallExpression glob = BuildGlobCall(effectiveTitle, globPattern);
        return Expression.Lambda<Func<BookEntity, bool>>(glob, book);
    }

    /// <summary>
    /// Builds a predicate that is satisfied when the first character of the effective title is neither a letter nor a digit.
    /// </summary>
    /// <param name="book">The parameter representing the book entity.</param>
    /// <param name="effectiveTitle">The expression of the effective title.</param>
    /// <returns>The built predicate.</returns>
    private static Expression<Func<BookEntity, bool>> BuildSymbolPredicate(ParameterExpression book, Expression effectiveTitle)
    {
        Expression notLetter = Expression.Not(BuildGlobCall(effectiveTitle, "[a-z]*"));
        Expression notDigit = Expression.Not(BuildGlobCall(effectiveTitle, "[0-9]*"));
        return Expression.Lambda<Func<BookEntity, bool>>(Expression.AndAlso(notLetter, notDigit), book);
    }

    /// <summary>
    /// Builds the call to <see cref="SqliteDbFunctionsExtensions.Glob"/> for the effective title and the provided glob pattern.
    /// </summary>
    /// <param name="effectiveTitle">The expression of the effective title.</param>
    /// <param name="globPattern">The glob pattern the effective title must match.</param>
    /// <returns>The built method call.</returns>
    private static MethodCallExpression BuildGlobCall(Expression effectiveTitle, string globPattern)
    {
        Expression functions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        return Expression.Call(s_globMethod, functions, effectiveTitle, Expression.Constant(globPattern));
    }
}
