#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
#endregion

namespace Lumina.Presentation.Api.Common.Authentication;

/// <summary>
/// Service for retrieving the current user's information from the HTTP context.
/// </summary>
public class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpContextCurrentUserService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> used to access the current HTTP context.</param>
    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the unique identifier of the currently authenticated user. Returns <see langword="null"/> if no user is authenticated or if the claim is not present.
    /// </summary>
    public Guid? UserId
    {
        get
        {
            string? userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out Guid userId) ? userId : null;
        }
    }
}
