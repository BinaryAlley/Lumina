#region ========================================================================= USING =====================================================================================
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Fixtures;

/// <summary>
/// Provides JWT token generation functionality for testing purposes.
/// </summary>
[ExcludeFromCodeCoverage]
public class JwtFixture
{
    private static readonly string s_secretKey = "test-key-thats-at-least-32-chars-long-for-jwt";

    /// <summary>
    /// Generates a JWT token for testing with the specified username and optional roles.
    /// </summary>
    /// <param name="username">The username to include in the token claims.</param>
    /// <param name="roles">Optional array of roles to include in the token claims.</param>
    /// <returns>A JWT token string that can be used for authentication in tests.</returns>
    public static string GenerateJwtToken(string username, string[]? roles = null)
    {
        byte[] key = Encoding.ASCII.GetBytes(s_secretKey);
        List<Claim> claims =
        [
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, username)
        ];

        if (roles?.Length > 0)
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        JwtSecurityToken token = new(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
