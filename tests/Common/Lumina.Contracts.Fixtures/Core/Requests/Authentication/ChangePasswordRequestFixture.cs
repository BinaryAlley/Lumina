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
    /// <returns>The created <see cref="ChangePasswordRequest"/>.</returns>
    public ChangePasswordRequest Create(
        string? username = null,
        string? currentPassword = null,
        string? newPassword = null,
        string? newPasswordConfirm = null)
    {
        string generatedNewPassword = newPassword ?? _faker.Internet.Password();
        return new ChangePasswordRequest(
            username ?? _faker.Internet.UserName(),
            currentPassword ?? _faker.Internet.Password(),
            generatedNewPassword,
            newPasswordConfirm ?? generatedNewPassword
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
