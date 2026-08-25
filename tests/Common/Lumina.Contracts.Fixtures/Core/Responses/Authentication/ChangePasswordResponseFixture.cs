#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Authentication;

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
    /// <param name="isPasswordChanged">Optional. Whether the password was changed.</param>
    /// <returns>The created <see cref="ChangePasswordResponse"/>.</returns>
    public ChangePasswordResponse Create(
        bool? isPasswordChanged = null)
    {
        return new ChangePasswordResponse(isPasswordChanged ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="ChangePasswordResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ChangePasswordResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
