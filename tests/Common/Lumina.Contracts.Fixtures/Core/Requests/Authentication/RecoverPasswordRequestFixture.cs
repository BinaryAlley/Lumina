#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Authentication;

/// <summary>
/// Fixture class for the <see cref="RecoverPasswordRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RecoverPasswordRequest"/>.
    /// </summary>
    /// <param name="username">Optional. The username for password recovery.</param>
    /// <param name="totpCode">Optional. The TOTP code for verification.</param>
    /// <returns>The created <see cref="RecoverPasswordRequest"/>.</returns>
    public RecoverPasswordRequest Create(
        string? username = null,
        string? totpCode = null)
    {
        return new RecoverPasswordRequest(
            username ?? _faker.Internet.UserName(),
            totpCode ?? _faker.Random.Number(100000, 999999).ToString()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="RecoverPasswordRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RecoverPasswordRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
