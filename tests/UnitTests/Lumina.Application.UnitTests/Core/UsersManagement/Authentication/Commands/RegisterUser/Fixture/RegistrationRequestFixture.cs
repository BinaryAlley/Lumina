#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RegisterUser.Fixture;

/// <summary>
/// Fixture class for the <see cref="RegistrationRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegistrationRequestFixture
{
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationRequestFixture"/> class.
    /// </summary>
    public RegistrationRequestFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid <see cref="RegistrationRequest"/>.
    /// </summary>
    /// <returns>The created <see cref="RegistrationRequest"/>.</returns>
    public RegistrationRequest CreateRegistrationRequest(
        string? username = null,
        string? password = null,
        string? passwordConfirm = null,
        bool? use2fa = null)
    {
        password ??= _faker.Internet.Password();

        return new RegistrationRequest(
            username ?? _faker.Internet.UserName(),
            password,
            passwordConfirm ?? password,
            use2fa ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="RegistrationRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RegistrationRequest> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateRegistrationRequest())
            .ToList();
    }
}
