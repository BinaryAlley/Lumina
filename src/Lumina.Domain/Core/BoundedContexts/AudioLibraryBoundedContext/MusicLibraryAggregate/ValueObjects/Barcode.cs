#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the barcode of an album.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class Barcode : ValueObject
{
    private static readonly Regex s_barcodeRegex = new(@"^\d{12,13}$", RegexOptions.Compiled);

    /// <summary>
    /// Gets the value of the barcode.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Barcode"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private Barcode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Barcode"/> class.
    /// </summary>
    /// <param name="value">The value of the barcode.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Barcode"/>, or an error message.
    /// </returns>
    public static Result<Barcode> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Errors.Music.BarcodeValueCannotBeEmpty;

        string cleanedValue = value.Trim();
        if (!s_barcodeRegex.IsMatch(cleanedValue))
            return Errors.Music.InvalidFormatForBarcode;

        return new Barcode(cleanedValue);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
