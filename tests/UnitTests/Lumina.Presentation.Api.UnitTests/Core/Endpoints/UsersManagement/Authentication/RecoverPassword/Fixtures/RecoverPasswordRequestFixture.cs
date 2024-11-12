#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.RecoverPassword.Fixtures;

/// <summary>
/// Fixture class for the <see cref="RecoverPasswordRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="RecoverPasswordRequest"/> with default or random values.
    /// </summary>
    /// <param name="username">The username for password recovery.</param>
    /// <param name="totpCode">The TOTP code for verification.</param>
    /// <returns>The created <see cref="RecoverPasswordRequest"/>.</returns>
    public RecoverPasswordRequest Create(
        string? username = null,
        string? totpCode = null)
    {
        return new RecoverPasswordRequest(
            Username: username ?? _faker.Internet.UserName(),
            TotpCode: totpCode ?? _faker.Random.Number(100000, 999999).ToString()
        );
    }
}
