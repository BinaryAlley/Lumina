#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.ChangePassword.Fixtures;

/// <summary>
/// Fixture class for the <see cref="ChangePasswordRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordRequestFixture
{
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordRequestFixture"/> class.
    /// </summary>
    public ChangePasswordRequestFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid <see cref="ChangePasswordRequest"/>.
    /// </summary>
    /// <returns>The created <see cref="ChangePasswordRequest"/>.</returns>
    public ChangePasswordRequest CreateChangePasswordRequest(
        string? username = null,
        string? currentPassword = null,
        string? newPassword = null,
        string? newPasswordConfirm = null)
    {
        return new ChangePasswordRequest(
            username ?? _faker.Internet.UserName(),
            currentPassword ?? _faker.Internet.Password(),
            newPassword ?? _faker.Internet.Password(),
            newPasswordConfirm ?? _faker.Internet.Password()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ChangePasswordRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ChangePasswordRequest> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateChangePasswordRequest())
            .ToList();
    }
}
