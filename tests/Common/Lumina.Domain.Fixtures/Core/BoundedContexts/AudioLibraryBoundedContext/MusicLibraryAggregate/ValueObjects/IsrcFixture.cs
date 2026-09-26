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
/// Fixture class for the <see cref="Isrc"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsrcFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="Isrc"/>.
    /// </summary>
    /// <param name="value">Optional. The value of the ISRC. If not provided, a random valid ISRC is generated.</param>
    /// <returns>The created <see cref="Isrc"/>.</returns>
    public Isrc Create(
        string? value = null)
    {
        Result<Isrc> isrcResult = Isrc.Create(value ?? GenerateRandomValue());

        if (isrcResult.IsFailure)
            throw new InvalidOperationException("Failed to create Isrc: " + string.Join(", ", isrcResult.Errors));
        return isrcResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Isrc"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Isrc"/> instances.</returns>
    public List<Isrc> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Generates a random value matching the <c>^[A-Z]{2}[A-Z0-9]{3}[0-9]{7}$</c> ISRC format.
    /// </summary>
    /// <returns>The generated ISRC value.</returns>
    private string GenerateRandomValue()
    {
        string countryCode = _faker.Random.String2(2, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        string registrantCode = _faker.Random.String2(3, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
        string yearAndDesignationCode = _faker.Random.Number(1000000, 9999999).ToString();
        return countryCode + registrantCode + yearAndDesignationCode;
    }
}
