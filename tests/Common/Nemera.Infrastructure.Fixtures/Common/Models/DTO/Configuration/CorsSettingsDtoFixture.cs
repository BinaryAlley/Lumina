#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;

/// <summary>
/// Fixture class for the <see cref="CorsSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CorsSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid <see cref="CorsSettingsDto"/>.
    /// </summary>
    /// <param name="allowedOrigins">Optional. The allowed origins, or <see langword="null"/> to generate valid origins.</param>
    /// <returns>The created <see cref="CorsSettingsDto"/>.</returns>
    public CorsSettingsDto Create(string[]? allowedOrigins = null)
    {
        return new CorsSettingsDto
        {
            AllowedOrigins = allowedOrigins ?? [$"https://{_faker.Internet.DomainName()}", $"http://{_faker.Internet.DomainName()}"]
        };
    }

    /// <summary>
    /// Creates a list of <see cref="CorsSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="CorsSettingsDto"/> instances.</returns>
    public List<CorsSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
