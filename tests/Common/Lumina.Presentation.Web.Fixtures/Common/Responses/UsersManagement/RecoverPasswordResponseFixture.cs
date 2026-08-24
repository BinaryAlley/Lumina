#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Responses.UsersManagement;

/// <summary>
/// Fixture class for the <see cref="RecoverPasswordResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RecoverPasswordResponse"/>.
    /// </summary>
    /// <param name="isPasswordReset">Optional. Whether the password was reset or not.</param>
    /// <returns>The created <see cref="RecoverPasswordResponse"/>.</returns>
    public RecoverPasswordResponse Create(
        bool? isPasswordReset = null)
    {
        return new RecoverPasswordResponse(isPasswordReset ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="RecoverPasswordResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RecoverPasswordResponse"/> instances.</returns>
    public List<RecoverPasswordResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
