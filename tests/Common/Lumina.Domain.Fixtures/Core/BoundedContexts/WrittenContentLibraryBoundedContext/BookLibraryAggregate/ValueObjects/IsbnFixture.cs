#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="Isbn"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnFixture
{
    private readonly Random _random = new();

    /// <summary>
    /// Creates a valid <see cref="Isbn"/> with a random or explicitly requested format.
    /// </summary>
    /// <param name="format">Optional. The requested <see cref="IsbnFormat"/>. If not provided, a random format is selected.</param>
    /// <returns>The created <see cref="Isbn"/>.</returns>
    public Isbn Create(IsbnFormat? format = null)
    {
        IsbnFormat targetFormat = format ?? (_random.Next(2) == 0 ? IsbnFormat.Isbn10 : IsbnFormat.Isbn13);
        string isbn = targetFormat == IsbnFormat.Isbn10 ? GenerateValidIsbn10() : GenerateValidIsbn13();
        return Isbn.Create(isbn, targetFormat).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Isbn"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Isbn"/> instances.</returns>
    public List<Isbn> CreateMany(int count = 3)
    {
        List<Isbn> result = [];
        for (int i = 0; i < count; i++)
            result.Add(Create());
        return result;
    }

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
