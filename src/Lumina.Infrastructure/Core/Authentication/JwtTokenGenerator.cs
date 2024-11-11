#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Infrastructure.Common.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
#endregion

namespace Lumina.Infrastructure.Core.Authentication;

/// <summary>
/// Class for generating JWT tokens.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtSettingsModel _jwtSettingsModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenGenerator"/> class.
    /// </summary>
    /// <param name="dateTimeProvider">Injected service for time related functionality.</param>
    /// <param name="jwtSettingsModelOptions">Injected service for retrieving <see cref="JwtSettingsModel"/>.</param>
    public JwtTokenGenerator(IDateTimeProvider dateTimeProvider, IOptions<JwtSettingsModel> jwtSettingsModelOptions)
    {
        _dateTimeProvider = dateTimeProvider;
        _jwtSettingsModel = jwtSettingsModelOptions.Value;
    }

    /// <summary>
    /// Generates a new JWT token.
    /// </summary>
    /// <param name="id">The id of the user for which to generate the token.</param>
    /// <param name="username">The username of the user for which to generate the token.</param>
    /// <returns>The generated JWT token.</returns>
    public string GenerateToken(string id, string username)
    {
        // use a symmetric key approach
        SigningCredentials signingCredentials = new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsModel.SecretKey)), SecurityAlgorithms.HmacSha256);
        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, id),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Jti, id.ToString())
        ];
        JwtSecurityToken securityToken = new(
            issuer: _jwtSettingsModel.Issuer,
            audience: _jwtSettingsModel.Audience,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettingsModel.ExpiryMinutes),
            claims: claims,
            signingCredentials: signingCredentials);
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}
