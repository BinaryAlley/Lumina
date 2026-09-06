#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for generating <see cref="IsbnDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnDtoFixture
{
    private readonly Random _random = new();

    /// <summary>
    /// Creates a new <see cref="IsbnDto"/> instance with a random or explicitly requested format.
    /// </summary>
    /// <param name="value">Optional. The ISBN value. If not provided, a valid ISBN is generated.</param>
    /// <param name="format">Optional. The ISBN format. If not provided, the format is derived from the value.</param>
    /// <returns>A configured <see cref="IsbnDto"/> instance.</returns>
    public IsbnDto Create(string? value = null, IsbnFormat? format = null)
    {
        string resolvedValue = value ?? (format == IsbnFormat.Isbn10 ? GenerateValidIsbn10() : GenerateValidIsbn13());
        return new IsbnDto
        {
            Value = resolvedValue,
            Format = format ?? (resolvedValue.Length > 13 ? IsbnFormat.Isbn13 : IsbnFormat.Isbn10)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="IsbnDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="IsbnDto"/> instances.</returns>
    public List<IsbnDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Generates a valid ISBN-10 with the correct checksum.
    /// </summary>
    /// <returns>A valid ISBN-10 value.</returns>
    private string GenerateValidIsbn10()
    {
        int[] digits = new int[9];
        for (int i = 0; i < 9; i++)
            digits[i] = _random.Next(0, 10);

        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (10 - i) * digits[i];

        int checkDigit = (11 - sum % 11) % 11;
        string checkChar = checkDigit == 10 ? "X" : checkDigit.ToString();

        return $"{digits[0]}-{digits[1]}{digits[2]}-{digits[3]}{digits[4]}{digits[5]}{digits[6]}{digits[7]}{digits[8]}-{checkChar}";
    }

    /// <summary>
    /// Generates a valid ISBN-13 with the correct checksum.
    /// </summary>
    /// <returns>A valid ISBN-13 value.</returns>
    private string GenerateValidIsbn13()
    {
        string prefix = _random.Next(2) == 0 ? "978" : "979";
        string group = _random.Next(0, 99999).ToString().PadLeft(5, '0');
        string publisher = _random.Next(0, 999999).ToString().PadLeft(6, '0');
        string title = _random.Next(0, 99).ToString().PadLeft(2, '0');

        string isbn = $"{prefix}{group[..1]}{publisher}{title}";
        int sum = 0;
        for (int i = 0; i < 12; i++)
            sum += (i % 2 == 0 ? 1 : 3) * int.Parse(isbn[i].ToString());

        int checkDigit = (10 - sum % 10) % 10;

        return $"{prefix}-{group[..1]}-{publisher}-{title}-{checkDigit}";
    }
}
