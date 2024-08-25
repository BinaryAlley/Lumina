#region ========================================================================= USING =====================================================================================
using System.Text.RegularExpressions;
using ErrorOr;
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the ISBN of a book.
/// </summary>
public sealed class Isbn : ValueObject
{
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
    /// <param name="isbn">The ISBN-10 string to validate, without hyphens or spaces.</param>
    /// <returns><see langword="true"/> if the ISBN-10 is valid, <see langword="false"/> otherwise.</returns>
    public static bool IsValidIsbn10(string isbn)
    {
        // ISBN-10 must be exactly 10 characters long
        if (isbn.Length != 10)
            return false;
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            // each of the first 9 characters must be a digit
            if (!char.IsDigit(isbn[i]))
                return false;
            // calculate the sum: each digit is multiplied by its position (10 to 2) and added
            sum += (10 - i) * (isbn[i] - '0');
        }
        // check the last character, which can be a digit or 'X'
        char lastChar = isbn[9];
        if (lastChar == 'X' || lastChar == 'x')
            sum += 10;
        else if (char.IsDigit(lastChar))
            sum += lastChar - '0';
        else
            return false;
        // the ISBN-10 is valid if the sum is divisible by 11
        return sum % 11 == 0;
    }

    /// <summary>
    /// Validates an ISBN-13 string.
    /// </summary>
    /// <param name="isbn">The ISBN-13 string to validate, without hyphens or spaces.</param>
    /// <returns><see langword="true"/> if the ISBN-13 is valid, <see langword="false"/> otherwise.</returns>
    public static bool IsValidIsbn13(string isbn)
    {
        // ISBN-13 must be exactly 13 characters long
        if (isbn.Length != 13)
            return false;
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            // each of the first 12 characters must be a digit
            if (!char.IsDigit(isbn[i]))
                return false;
            // calculate the sum: alternate digits are multiplied by 3
            sum += (i % 2 == 0) ? isbn[i] - '0' : 3 * (isbn[i] - '0');
        }
        // calculate the check digit
        int checkDigit = 10 - (sum % 10);
        if (checkDigit == 10)
            checkDigit = 0;
        // the ISBN-13 is valid if the calculated check digit matches the last digit
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