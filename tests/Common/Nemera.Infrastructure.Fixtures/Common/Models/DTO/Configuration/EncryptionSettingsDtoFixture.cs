#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;

/// <summary>
/// Fixture class for the <see cref="EncryptionSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EncryptionSettingsDtoFixture
{
    /// <summary>
    /// Creates a valid <see cref="EncryptionSettingsDto"/> with a random 256-bit key.
    /// </summary>
    /// <param name="secretKey">Optional. The secret key. If not provided, a valid 256-bit (32 byte) key for AES-256 is generated.</param>
    /// <returns>The created <see cref="EncryptionSettingsDto"/>.</returns>
    public EncryptionSettingsDto Create(string? secretKey = null)
    {
        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);

        return new EncryptionSettingsDto
        {
            SecretKey = secretKey ?? Convert.ToBase64String(key)
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
