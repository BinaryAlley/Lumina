#region ========================================================================= USING =====================================================================================
using ErrorOr;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Infrastructure.Validation;

/// <summary>
/// Interface for validating requests of commands and queries.
/// </summary>
/// <typeparam name="TRequest">The type of the request (command or query) to be validated.</typeparam>
public interface IValidator<in TRequest>
{
    /// <summary>
    /// Validates the specified request instance against the defined rules.
    /// </summary>
    /// <param name="instance">The request instance to validate.</param>
    /// <returns>A list of validation errors. If the instance is valid, the list will be empty.</returns>
    List<Error> Validate(TRequest instance);
}
