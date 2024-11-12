#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RecoverPassword.Fixtures;

/// <summary>
/// Fixture class for the <see cref="RecoverPasswordRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordRequestFixture
{
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordRequestFixture"/> class.
    /// </summary>
    public RecoverPasswordRequestFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid <see cref="RecoverPasswordRequest"/>.
    /// </summary>
    /// <returns>The created <see cref="RecoverPasswordRequest"/>.</returns>
    public RecoverPasswordRequest CreateRecoverPasswordRequest(
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
        return Enumerable.Range(0, count)
            .Select(_ => CreateRecoverPasswordRequest())
            .ToList();
    }
}
