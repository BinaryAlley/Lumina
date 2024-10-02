#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the ISBN of a book.
/// </summary>
[DebuggerDisplay("{Value} ({Format})")]
public sealed class Isbn : ValueObject
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private static readonly Regex s_isbn10Regex = new(@"^(?:ISBN(?:-10)?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[-\ ]){3})[-\ 0-9X]{13}$)[0-9]{1,5}[-\ ]?[0-9]+[-\ ]?[0-9]+[-\ ]?[0-9X]$", RegexOptions.Compiled);
    private static readonly Regex s_isbn13Regex = new(@"^(?:ISBN(?:-13)?:? )?(?=[0-9]{13}$|(?=(?:[0-9]+[-\ ]){4})[-\ 0-9]{17}$)97[89][-\ ]?[0-9]{1,5}[-\ ]?[0-9]+[-\ ]?[0-9]+[-\ ]?[0-9]$", RegexOptions.Compiled);
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the value of the ISBN.
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    /// Gets the format of the ISBN.
    /// </summary>
    public IsbnFormat Format { get; private set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="Isbn"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    /// <param name="format">The format of the ISBN representing this object.</param>
    private Isbn(string value, IsbnFormat format)
    {
        Value = value;
        Format = format;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="Isbn"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    /// <param name="format">The format of the ISBN representing this object.</param>
    /// <returns>The created <see cref="Isbn"/> instance.</returns>
    public static ErrorOr<Isbn> Create(string value, IsbnFormat format)
    {
        // enforce invariants
        if (string.IsNullOrWhiteSpace(value))
            return Errors.WrittenContent.IsbnValueCannotBeEmpty;
        // remove hyphens and spaces, for validation
        string cleanedValue = Regex.Replace(value, @"[-\s]", string.Empty);
        switch (format)
        {
            case IsbnFormat.Isbn10:
                if (!IsValidIsbn10(cleanedValue))
                    return Errors.WrittenContent.InvalidIsbn10Format;
                break;
            case IsbnFormat.Isbn13:
                if (!IsValidIsbn13(cleanedValue))
                    return Errors.WrittenContent.InvalidIsbn13Format;
                break;
            default:
                return Errors.WrittenContent.UnknownIsbnFormat;
        }
        return new Isbn(value, format);
    }

    /// <summary>
    /// Validates an ISBN-10 string.
    /// </summary>
    /// <param name="isbn">The ISBN-10 string to validate, with or without hyphens or spaces.</param>
    /// <returns><see langword="true"/> if the ISBN-10 is valid, <see langword="false"/> otherwise.</returns>
    public static bool IsValidIsbn10(string isbn)
    {
        if (!s_isbn10Regex.IsMatch(isbn))
            return false;
        // remove any hyphens or spaces
        isbn = isbn.Replace("-", "").Replace(" ", "").ToUpper();
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (10 - i) * (isbn[i] - '0');
        char lastChar = isbn[9];
        if (lastChar == 'X')
            sum += 10;
        else
            sum += lastChar - '0';
        return sum % 11 == 0;
    }

    /// <summary>
    /// Validates an ISBN-13 string.
    /// </summary>
    /// <param name="isbn">The ISBN-13 string to validate, with or without hyphens or spaces.</param>
    /// <returns><see langword="true"/> if the ISBN-13 is valid, <see langword="false"/> otherwise.</returns>
    public static bool IsValidIsbn13(string isbn)
    {
        if (!s_isbn13Regex.IsMatch(isbn))
            return false;
        // remove any hyphens or spaces
        isbn = isbn.Replace("-", "").Replace(" ", "");
        int sum = 0;
        for (int i = 0; i < 12; i++)
            sum += (i % 2 == 0) ? isbn[i] - '0' : 3 * (isbn[i] - '0');
        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (isbn[12] - '0');
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Format;
    }
    #endregion
}
