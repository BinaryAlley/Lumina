#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Primitives;
using System.Collections.Generic;
#endregion

namespace Nemera.Presentation.Web.Common.Validation;

/// <summary>
/// Interface for validating instances of a specific type.
/// </summary>
/// <typeparam name="TRequest">The type of the instance to be validated.</typeparam>
public interface IValidator<in TRequest>
{
    /// <summary>
    /// Validates the specified request instance against the defined rules.
    /// </summary>
    /// <param name="instance">The request instance to validate.</param>
    /// <returns>A list of validation errors. If the instance is valid, the list will be empty.</returns>
    List<Error> Validate(TRequest instance);
}
