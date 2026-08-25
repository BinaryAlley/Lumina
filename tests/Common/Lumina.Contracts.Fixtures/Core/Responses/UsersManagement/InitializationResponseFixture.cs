#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.UsersManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.UsersManagement;

/// <summary>
/// Fixture class for the <see cref="InitializationResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class InitializationResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="InitializationResponse"/>.
    /// </summary>
    /// <param name="isInitialized">Optional. Whether the application is initialized.</param>
    /// <returns>The created <see cref="InitializationResponse"/>.</returns>
    public InitializationResponse Create(
        bool? isInitialized = null)
    {
        return new InitializationResponse(isInitialized ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="InitializationResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<InitializationResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
