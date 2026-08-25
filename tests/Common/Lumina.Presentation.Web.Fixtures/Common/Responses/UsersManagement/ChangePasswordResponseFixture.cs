#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Responses.UsersManagement;

/// <summary>
/// Fixture class for the <see cref="ChangePasswordResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ChangePasswordResponse"/>.
    /// </summary>
    /// <param name="isPasswordChanged">Optional. Whether the password was changed or not.</param>
    /// <returns>The created <see cref="ChangePasswordResponse"/>.</returns>
    public ChangePasswordResponse Create(
        bool? isPasswordChanged = null)
    {
        return new ChangePasswordResponse(isPasswordChanged ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="ChangePasswordResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ChangePasswordResponse"/> instances.</returns>
    public List<ChangePasswordResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
