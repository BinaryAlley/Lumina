#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Configuration;

/// <summary>
/// Fixture class for the <see cref="EncryptionSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EncryptionSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="EncryptionSettingsDto"/>.
    /// </summary>
    /// <param name="secretKey">Optional. The secret key used to encrypt data.</param>
    /// <returns>The created <see cref="EncryptionSettingsDto"/>.</returns>
    public EncryptionSettingsDto Create(
        string? secretKey = null)
    {
        return new EncryptionSettingsDto
        {
            SecretKey = secretKey ?? _faker.Random.String2(44, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=")
        };
    }

    /// <summary>
    /// Creates a list of <see cref="EncryptionSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="EncryptionSettingsDto"/> instances.</returns>
    public List<EncryptionSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
