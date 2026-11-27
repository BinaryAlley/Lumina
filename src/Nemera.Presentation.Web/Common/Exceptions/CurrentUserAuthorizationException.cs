#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Net;
#endregion

namespace Lumina.Presentation.Web.Common.Exceptions;

/// <summary>
/// Exception for an unsuccessful current-user authorization check, which means that the stored session refers to a user the API no longer knows.
/// </summary>
public class CurrentUserAuthorizationException : ApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserAuthorizationException"/> class.
    /// </summary>
    /// <param name="problemDetails">The problem details object returned by the API.</param>
    /// <param name="httpStatusCode">The HTTP status code returned by the API.</param>
    /// <param name="requestPath">The absolute path of the API endpoint that produced this exception.</param>
    public CurrentUserAuthorizationException(ProblemDetailsDto? problemDetails, HttpStatusCode httpStatusCode, string? requestPath = null)
        : base(problemDetails, httpStatusCode, requestPath)
    {
    }
}
