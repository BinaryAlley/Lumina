#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement;

/// <summary>
/// Fixture class for generating <see cref="ChangePasswordRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="ChangePasswordRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="username">Optional username.</param>
    /// <param name="currentPassword">Optional current password.</param>
    /// <param name="newPassword">Optional new password.</param>
    /// <param name="newPasswordConfirm">Optional confirmation of the new password.</param>
    /// <returns>A configured <see cref="ChangePasswordRequest"/> instance.</returns>
    public ChangePasswordRequest Create(string? username = null, string? currentPassword = null, string? newPassword = null, string? newPasswordConfirm = null)
    {
        Faker faker = new();
        string generatedNewPassword = newPassword ?? faker.Internet.Password(12);
        return new ChangePasswordRequest(
            Username: username ?? faker.Internet.UserName(),
            CurrentPassword: currentPassword ?? faker.Internet.Password(12),
            NewPassword: generatedNewPassword,
            NewPasswordConfirm: newPasswordConfirm ?? generatedNewPassword
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ChangePasswordRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ChangePasswordRequest"/> instances.</returns>
    public List<ChangePasswordRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
