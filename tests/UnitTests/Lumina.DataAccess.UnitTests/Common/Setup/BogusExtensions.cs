#region ========================================================================= USING =====================================================================================
using Bogus;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Setup;

/// <summary>
/// Contains extension methods for the <see cref="Faker"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public static class BogusExtensions
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Adds <see cref="DateOnly"/> random data generation support to the <see cref="Faker"/> class.
    /// </summary>
    /// <param name="faker">The faker instance to extend.</param>
    /// <returns>A random<see cref="DateOnly"/> instance.</returns>
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
    /// <returns>A random<see cref="DateOnly"/> instance.</returns>
    public static DateOnly DateOnlyBetween(this Faker faker, DateOnly start, DateOnly end)
    {
        var startDateTime = start.ToDateTime(TimeOnly.MinValue);
        var endDateTime = end.ToDateTime(TimeOnly.MinValue);
        return System.DateOnly.FromDateTime(faker.Date.Between(startDateTime, endDateTime));
    }
    #endregion
}