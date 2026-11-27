namespace Lumina.Application.Common.Infrastructure.Authentication;

/// <summary>
/// Interface for the service for generating secure tokens for accounts confirmation or recovery.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates a secure token.
    /// </summary>
    /// <returns>A secure token.</returns>
    string GenerateToken();
}
