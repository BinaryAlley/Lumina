#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Common.Fixtures;

/// <summary>
/// Fixture class for the <see cref="Isbn"/> domain entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnFixture
{
    private readonly Random _random = new();

    /// <summary>
    /// Creates a valid ISBN-10.
    /// </summary>
    /// <returns>The created ISBN.</returns>
    public Isbn CreateIsbn10()
    {
        string isbn = GenerateValidIsbn10();
        return Isbn.Create(isbn, IsbnFormat.Isbn10).Value;
    }

    /// <summary>
    /// Creates a valid ISBN-13.
    /// </summary>
    /// <returns>The created ISBN.</returns>
    public Isbn CreateIsbn13()
    {
        string isbn = GenerateValidIsbn13();
        return Isbn.Create(isbn, IsbnFormat.Isbn13).Value;
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
