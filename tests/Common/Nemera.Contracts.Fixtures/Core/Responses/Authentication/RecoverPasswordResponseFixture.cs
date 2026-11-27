#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Authentication;

/// <summary>
/// Fixture class for the <see cref="RecoverPasswordResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RecoverPasswordResponse"/>.
    /// </summary>
    /// <param name="isPasswordReset">Optional. Whether the password was reset.</param>
    /// <returns>The created <see cref="RecoverPasswordResponse"/>.</returns>
    public RecoverPasswordResponse Create(
        bool? isPasswordReset = null)
    {
        return new RecoverPasswordResponse(isPasswordReset ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="RecoverPasswordResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RecoverPasswordResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
