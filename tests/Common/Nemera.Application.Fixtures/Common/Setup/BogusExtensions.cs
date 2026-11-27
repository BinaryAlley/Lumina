#region ========================================================================= USING =====================================================================================
using Bogus;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.Fixtures.Common.Setup;

/// <summary>
/// Contains extension methods for the <see cref="Faker"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public static class BogusExtensions
{
    /// <summary>
    /// Adds <see cref="DateOnly"/> random data generation support to the <see cref="Faker"/> class.
    /// </summary>
    /// <param name="faker">The faker instance to extend.</param>
    /// <returns>A random <see cref="DateOnly"/> instance.</returns>
    public static DateOnly DateOnly(this Faker faker)
    {
        return System.DateOnly.FromDateTime(faker.Date.Past());
    }

    /// <summary>
    /// Adds <see cref="DateOnly"/> random data generation within a specified interval support to the <see cref="Faker"/> class.
    /// </summary>
    /// <param name="faker">The faker instance to extend.</param>
    /// <param name="start">The start of the interval.</param>
    /// <param name="end">The end of the interval.</param>
    /// <returns>A random <see cref="DateOnly"/> instance.</returns>
    public static DateOnly DateOnlyBetween(this Faker faker, DateOnly start, DateOnly end)
    {
        DateTime startDateTime = start.ToDateTime(TimeOnly.MinValue);
        DateTime endDateTime = end.ToDateTime(TimeOnly.MinValue);
        return System.DateOnly.FromDateTime(faker.Date.Between(startDateTime, endDateTime));
    }

    /// <summary>
    /// Adds unique name generation support to the <see cref="Faker"/> class, so that shared lookup tables never receive duplicate primary keys.
    /// </summary>
    /// <param name="faker">The faker instance to extend.</param>
    /// <returns>A unique name of at most 50 characters.</returns>
    public static string UniqueName(this Faker faker)
    {
        string uniqueSuffix = Guid.NewGuid().ToString("N")[..12];
        int maximumWordLength = 50 - uniqueSuffix.Length - 1;
        string word = faker.Lorem.Word();
        string truncatedWord = word.Length > maximumWordLength ? word[..maximumWordLength] : word;
        return $"{truncatedWord}-{uniqueSuffix}";
    }
}
