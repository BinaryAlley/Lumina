#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Common.Fixtures;

/// <summary>
/// Fixture class for the <see cref="IsbnEntity"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnEntityFixture
{
    private readonly Random _random = new();

    /// <summary>
    /// Creates a valid ISBN-10 entity.
    /// </summary>
    /// <returns>The created ISBN entity.</returns>
    public IsbnEntity CreateIsbn10()
    {
        return new IsbnEntity(
            GenerateValidIsbn10(),
            IsbnFormat.Isbn10
        );
    }

    /// <summary>
    /// Creates a valid ISBN-13 entity.
    /// </summary>
    /// <returns>The created ISBN entity.</returns>
    public IsbnEntity CreateIsbn13()
    {
        return new IsbnEntity(
            GenerateValidIsbn13(),
            IsbnFormat.Isbn13
        );
    }

    /// <summary>
    /// Creates an invalid ISBN entity.
    /// </summary>
    /// <returns>The created invalid ISBN entity.</returns>
    public IsbnEntity CreateInvalid()
    {
        return new IsbnEntity(
            "invalid-isbn",
            IsbnFormat.Isbn13
        );
    }

    /// <summary>
    /// Creates an ISBN entity without format.
    /// </summary>
    /// <returns>The created ISBN entity.</returns>
    public IsbnEntity CreateWithoutFormat()
    {
        return new IsbnEntity(
            GenerateValidIsbn13(),
            null
        );
    }

    private string GenerateValidIsbn10()
    {
        int[] digits = new int[9];
        for (int i = 0; i < 9; i++)
            digits[i] = _random.Next(0, 10);

        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (10 - i) * digits[i];

        int checkDigit = (11 - (sum % 11)) % 11;
        string checkChar = checkDigit == 10 ? "X" : checkDigit.ToString();

        return $"{digits[0]}-{digits[1]}{digits[2]}-{digits[3]}{digits[4]}{digits[5]}{digits[6]}{digits[7]}{digits[8]}-{checkChar}";
    }

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

        int checkDigit = (10 - (sum % 10)) % 10;

        return $"{prefix}-{group[..1]}-{publisher}-{title}-{checkDigit}";
    }
}
