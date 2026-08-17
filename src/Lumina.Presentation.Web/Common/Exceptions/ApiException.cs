#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using System;
using System.Net;
#endregion

namespace Lumina.Presentation.Web.Common.Exceptions;

/// <summary>
/// Custom exception for interaction with the remote API.
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// Gets or sets the values of status codes defined for HTTP defined in RFC 2616 for HTTP 1.1.
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the problem details represented by this exception.
    /// </summary>
    public ProblemDetailsDto? ProblemDetails { get; }

    /// <summary>
    /// Gets or sets the absolute path of the API endpoint that produced this exception.
    /// </summary>
    public string? RequestPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    /// <param name="problemDetails">The problem details object returned by the API.</param>
    /// <param name="httpStatusCode">The HTTP status code returned by the API.</param>
    /// <param name="requestPath">The absolute path of the API endpoint that produced this exception.</param>
    public ApiException(ProblemDetailsDto? problemDetails, HttpStatusCode httpStatusCode, string? requestPath = null) : base(problemDetails?.Title ?? "An error occurred.")
    {
        ProblemDetails = problemDetails;
        HttpStatusCode = httpStatusCode;
        RequestPath = requestPath;
    }
}
