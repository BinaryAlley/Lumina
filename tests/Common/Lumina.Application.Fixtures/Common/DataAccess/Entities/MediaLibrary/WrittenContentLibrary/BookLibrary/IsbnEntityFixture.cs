#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="IsbnEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="IsbnEntity"/>, with a value that satisfies the ISBN checksum and a format that matches it.
    /// </summary>
    /// <param name="value">Optional. The ISBN value.</param>
    /// <param name="format">Optional. The format of the ISBN.</param>
    /// <param name="includeValue">Whether the value should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFormat">Whether the format should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="IsbnEntity"/>.</returns>
    public IsbnEntity Create(
        string? value = null,
        IsbnFormat? format = null,
        bool includeValue = true,
        bool includeFormat = true)
    {
        string? resolvedValue = includeValue ? value ?? CreateValidIsbn13() : null;
        IsbnFormat? resolvedFormat = includeFormat ? format ?? InferFormat(resolvedValue) : null;
        return new IsbnEntity(resolvedValue, resolvedFormat);
    }

    /// <summary>
    /// Creates a list of <see cref="IsbnEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<IsbnEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Infers the format of an ISBN from the number of digits of its value.
    /// </summary>
    /// <param name="value">The ISBN value whose format is inferred.</param>
    /// <returns>The inferred ISBN format.</returns>
    private static IsbnFormat InferFormat(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return IsbnFormat.Isbn13;

        int digitCount = value.Count(char.IsLetterOrDigit);
        return digitCount is 10 ? IsbnFormat.Isbn10 : IsbnFormat.Isbn13;
    }

    /// <summary>
    /// Generates a random ISBN-13 value with a valid check digit.
    /// </summary>
    /// <returns>A valid ISBN-13 value of 13 digits.</returns>
    private string CreateValidIsbn13()
    {
        string prefix = _faker.Random.Bool() ? "978" : "979";
        string firstTwelveDigits = prefix + _faker.Random.String2(9, "0123456789");
        int sum = 0;
        for (int index = 0; index < 12; index++)
            sum += index % 2 == 0 ? firstTwelveDigits[index] - '0' : 3 * (firstTwelveDigits[index] - '0');
        int checkDigit = (10 - (sum % 10)) % 10;
        return firstTwelveDigits + checkDigit;
    }
}
