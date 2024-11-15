#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Presentation.Api.Common.Http;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Common;

/// <summary>
/// Base class for API endpoints.
/// </summary>
/// <typeparam name="TRequest">The type of the request object.</typeparam>
/// <typeparam name="TResponse">The type of the response object.</typeparam>
public abstract class BaseEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse> where TRequest : notnull
                                                                                        where TResponse : notnull
{
    /// <summary>
    /// Handles controllers errors.
    /// </summary>
    /// <param name="errors">The error to handle.</param>
    protected IResult Problem(List<Error> errors)
    {
        if (errors.Count is 0) // if there are no custom errors given, return a generic problem
            return Results.Problem();
        // try to make the problem type more specific
        if (errors.All(error => error.Type == ErrorType.Validation))
            return ValidationProblem(errors);
        // here we could handle custom error types, for instance: if (errors.All(error => error.NumericType == 23)), where 23 could be "domain errors"

        // share the list of errors within the scope of this HTTP request
        HttpContext.Items[HttpContextItemKeys.ERRORS] = errors;
        // get a HTTP error status code for the selected domain error type
        return Problem(errors.First());
    }

    /// <summary>
    /// Converts an <see cref="Error"/> to a standard problem response.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    protected IResult Problem(Error error)
    {
        Dictionary<string, object?> extensionsDictionary = [];
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden, // DO NOT return Status401Unauthorized, that actually means lack of valid authentication credentials (not logged in)
            ErrorType.Failure => StatusCodes.Status403Forbidden,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        extensionsDictionary["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        // return the HTTP error status and the domain error message
        return Results.Problem(
            detail: error.Description,
            instance: HttpContext.Request.Path,
            statusCode: statusCode,
            title: error.Code,
            extensions: extensionsDictionary
        );
    }

    /// <summary>
    /// Converts a list of <see cref="Error"/> into a standard validation problem response.
    /// </summary>
    /// <param name="errors">The list of errors to convert.</param>
    protected IResult ValidationProblem(List<Error> errors)
    {
        Dictionary<string, string[]> errorsDictionary = [];
        Dictionary<string, object?> extensionsDictionary = [];

        // populate the errors dictionary with error codes and their descriptions
        foreach (Error error in errors)
        {
            if (!errorsDictionary.TryGetValue(error.Code, out string[]? value))
                errorsDictionary.Add(error.Code, [error.Description]);
            else
            {
                if (!errorsDictionary[error.Code].Contains(error.Description))
                    errorsDictionary[error.Code] = [.. value, error.Description];
            }
        }

        // add trace identifier to the extensions dictionary for tracing purposes
        extensionsDictionary["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        // return a validation problem result with the constructed error details
        return Results.ValidationProblem(
            errorsDictionary,
            detail: "OneOrMoreValidationErrorsOccurred", 
            instance: HttpContext.Request.Path,
            statusCode: 422,
            title: "General.Validation",
            extensions: extensionsDictionary
        );
    }
}
