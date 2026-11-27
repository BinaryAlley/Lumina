#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Primitives;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Validation;

/// <summary>
/// Interface for triggering validator evaluation.
/// </summary>
/// <typeparam name="TRequest">The type of the instance being evaluated.</typeparam>
internal interface IValidationRule<in TRequest>
{
    /// <summary>
    /// Evaluates the validation rule against the specified request instance.
    /// </summary>
    /// <param name="instance">The request instance to validate.</param>
    /// <returns>The <see cref="Error"/>s produced by this rule; empty if the rule passes.</returns>
    IEnumerable<Error> Validate(TRequest instance);
}
