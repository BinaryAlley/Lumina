#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authentication;
using System;
using System.Security.Cryptography;
#endregion

namespace Lumina.Infrastructure.Core.Authentication;

/// <summary>
/// Service for the service for generating secure tokens for accounts confirmation or recovery.
/// </summary>
public class TokenGenerator : ITokenGenerator
{
    /// <summary>
    /// Generates a secure token.
    /// </summary>
    /// <returns>A secure token.</returns>
    public string GenerateToken()
    {
        // allocate an array for the token data
        byte[] tokenData = new byte[32];
        // fill the array with a cryptographically strong sequence of random values
        RandomNumberGenerator.Fill(tokenData);
        // convert to a Base64 string, but replace URL-unsafe characters '+' and '/' with '-' and '_', and remove padding characters ('=')
        return Convert.ToBase64String(tokenData).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
