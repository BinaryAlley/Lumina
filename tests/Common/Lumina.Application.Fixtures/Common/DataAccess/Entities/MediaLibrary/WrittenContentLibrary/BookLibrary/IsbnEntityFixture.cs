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
    /// Creates a random valid <see cref="IsbnEntity"/>.
    /// </summary>
    /// <param name="value">Optional. The ISBN value.</param>
    /// <param name="format">Optional. The format of the ISBN.</param>
    /// <param name="includeValue">Whether the value should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="IsbnEntity"/>.</returns>
    public IsbnEntity Create(
        string? value = null, 
        IsbnFormat? format = null, 
        bool includeValue = true)
    {
        return new IsbnEntity(
            includeValue ? value ?? _faker.Random.String2(13, "0123456789") : null,
            format);
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
}
