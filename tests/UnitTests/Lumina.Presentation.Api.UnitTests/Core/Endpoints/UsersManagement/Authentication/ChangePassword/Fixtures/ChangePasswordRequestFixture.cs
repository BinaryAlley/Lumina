#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.ChangePassword.Fixtures;

/// <summary>
/// Fixture class for the <see cref="ChangePasswordRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="ChangePasswordRequest"/> with default or random values.
    /// </summary>
    /// <param name="username">The username for the password change.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="newPasswordConfirm">The new password confirmation.</param>
    /// <returns>The created <see cref="ChangePasswordRequest"/>.</returns>
    public ChangePasswordRequest Create(
        string? username = null,
        string? currentPassword = null,
        string? newPassword = null,
        string? newPasswordConfirm = null)
    {
        string generatedNewPassword = newPassword ?? _faker.Internet.Password();
        return new ChangePasswordRequest(
            Username: username ?? _faker.Internet.UserName(),
            CurrentPassword: currentPassword ?? _faker.Internet.Password(),
            NewPassword: generatedNewPassword,
            NewPasswordConfirm: newPasswordConfirm ?? generatedNewPassword
        );
    }
}
