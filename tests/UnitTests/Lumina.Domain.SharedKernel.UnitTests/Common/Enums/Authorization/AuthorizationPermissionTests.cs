#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AuthorizationPermission"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationPermissionTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void AuthorizationPermission_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        AuthorizationPermission[] values = Enum.GetValues<AuthorizationPermission>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.All(values, value => Assert.True(Enum.IsDefined(value)));
    }

    [Fact]
    public void None_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)AuthorizationPermission.None;

        // Assert
        Assert.Equal(0, value);
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithCamelCaseConverter_ShouldPreserveEnumValue()
    {
        // Arrange
        foreach (AuthorizationPermission value in Enum.GetValues<AuthorizationPermission>())
        {
            // Act
            string json = JsonSerializer.Serialize(value, _jsonOptions);
            AuthorizationPermission deserialized = JsonSerializer.Deserialize<AuthorizationPermission>(json, _jsonOptions);

            // Assert
            Assert.Equal(value, deserialized);
            Assert.StartsWith("\"", json, StringComparison.Ordinal);
        }
    }
}
