#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Authentication;

/// <summary>
/// Fixture class for the <see cref="ChangePasswordRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ChangePasswordRequest"/>.
    /// </summary>
    /// <param name="username">Optional. The username for the password change.</param>
    /// <param name="currentPassword">Optional. The current password.</param>
    /// <param name="newPassword">Optional. The new password.</param>
    /// <param name="newPasswordConfirm">Optional. The new password confirmation.</param>
    /// <param name="includeUsername">Whether the username should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCurrentPassword">Whether the current password should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeNewPassword">Whether the new password should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeNewPasswordConfirm">Whether the new password confirmation should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="ChangePasswordRequest"/>.</returns>
    public ChangePasswordRequest Create(
        string? username = null,
        string? currentPassword = null,
        string? newPassword = null,
        string? newPasswordConfirm = null,
        bool includeUsername = true,
        bool includeCurrentPassword = true,
        bool includeNewPassword = true,
        bool includeNewPasswordConfirm = true)
    {
        string generatedNewPassword = newPassword ?? _faker.Internet.Password();
        return new ChangePasswordRequest(
            includeUsername ? (username ?? _faker.Internet.UserName()) : null,
            includeCurrentPassword ? (currentPassword ?? _faker.Internet.Password()) : null,
            includeNewPassword ? generatedNewPassword : null,
            includeNewPasswordConfirm ? (newPasswordConfirm ?? generatedNewPassword) : null
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ChangePasswordRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ChangePasswordRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
