#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Configuration;

/// <summary>
/// Fixture class for the <see cref="ServerConfigurationDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ServerConfigurationDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ServerConfigurationDto"/>.
    /// </summary>
    /// <param name="apiVersion">Optional. The remote API server path version.</param>
    /// <param name="baseAddress">Optional. The remote API server path.</param>
    /// <param name="port">Optional. The remote API server port.</param>
    /// <returns>The created <see cref="ServerConfigurationDto"/>.</returns>
    public ServerConfigurationDto Create(
        char? apiVersion = null, 
        string? baseAddress = null, 
        ushort? port = null)
    {
        return new ServerConfigurationDto
        {
            ApiVersion = apiVersion ?? _faker.Random.Char('1', '9'),
            BaseAddress = baseAddress ?? "http://localhost",
            Port = port ?? _faker.Random.UShort()
        };
    }

    /// <summary>
    /// Creates a list of <see cref="ServerConfigurationDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ServerConfigurationDto"/> instances.</returns>
    public List<ServerConfigurationDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
