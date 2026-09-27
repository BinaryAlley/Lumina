#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="Barcode"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class BarcodeFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="Barcode"/>.
    /// </summary>
    /// <param name="value">Optional. The value of the barcode. If not provided, a random valid barcode is generated.</param>
    /// <returns>The created <see cref="Barcode"/>.</returns>
    public Barcode Create(
        string? value = null)
    {
        Result<Barcode> barcodeResult = Barcode.Create(value ?? GenerateRandomValue());

        if (barcodeResult.IsFailure)
            throw new InvalidOperationException("Failed to create Barcode: " + string.Join(", ", barcodeResult.Errors));
        return barcodeResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Barcode"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Barcode"/> instances.</returns>
    public List<Barcode> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Generates a random value matching the <c>^\d{12,13}$</c> barcode format.
    /// </summary>
    /// <returns>The generated barcode value.</returns>
    private string GenerateRandomValue()
    {
        int digitCount = _faker.Random.Int(12, 13);
        return _faker.Random.String2(digitCount, "0123456789");
    }
}
