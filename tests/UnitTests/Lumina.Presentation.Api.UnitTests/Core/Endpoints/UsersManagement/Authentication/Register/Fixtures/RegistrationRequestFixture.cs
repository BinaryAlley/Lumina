#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.Register.Fixtures;

/// <summary>
/// Fixture class for the <see cref="RegistrationRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegistrationRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="RegistrationRequest"/> with default or random values.
    /// </summary>
    /// <param name="username">The username for registration.</param>
    /// <param name="password">The password for registration.</param>
    /// <param name="passwordConfirm">The password confirmation.</param>
    /// <param name="use2fa">Whether to use two-factor authentication.</param>
    /// <returns>The created <see cref="RegistrationRequest"/>.</returns>
    public RegistrationRequest Create(
        string? username = null,
        string? password = null,
        string? passwordConfirm = null,
        bool? use2fa = null)
    {
        string generatedPassword = password ?? _faker.Internet.Password();
        return new RegistrationRequest(
            Username: username ?? _faker.Internet.UserName(),
            Password: generatedPassword,
            PasswordConfirm: passwordConfirm ?? generatedPassword,
            Use2fa: use2fa ?? _faker.Random.Bool()
        );
    }
}
