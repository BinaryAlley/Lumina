#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement;

/// <summary>
/// Fixture class for generating <see cref="RecoverPasswordRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="RecoverPasswordRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="username">Optional username.</param>
    /// <param name="totpCode">Optional TOTP code.</param>
    /// <returns>A configured <see cref="RecoverPasswordRequest"/> instance.</returns>
    public RecoverPasswordRequest Create(string? username = null, string? totpCode = null)
    {
        Faker faker = new();
        return new RecoverPasswordRequest(
            Username: username ?? faker.Internet.UserName(),
            TotpCode: totpCode
        );
    }

    /// <summary>
    /// Creates multiple <see cref="RecoverPasswordRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RecoverPasswordRequest"/> instances.</returns>
    public List<RecoverPasswordRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
