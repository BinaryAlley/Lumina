namespace Lumina.Application.Common.Infrastructure.Authentication;

/// <summary>
/// Interface for generating JWT tokens.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a new JWT token.
    /// </summary>
    /// <param name="id">The id of the user for which to generate the token.</param>
    /// <param name="username">The username for which to generate the token.</param>
    /// <returns>The generated JWT token.</returns>
    string GenerateToken(string id, string username);
}
