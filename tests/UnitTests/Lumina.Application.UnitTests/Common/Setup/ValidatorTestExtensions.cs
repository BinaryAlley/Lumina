#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Infrastructure.Validation;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.UnitTests.Common.Setup;

/// <summary>
/// Validator extension methods for <see cref="IValidator{TRequest}"/> for unit tests.
/// </summary>
public static class ValidatorTestExtensions
{
    /// <summary>
    /// Executes the validator against the specified request instance and returns the resulting errors.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="validator">The validator instance to execute.</param>
    /// <param name="request">The request instance to validate.</param>
    /// <returns>A list of <see cref="Error"/> produced by the validator. If the request is valid, the list will be empty.</returns>
    public static List<Error> TestValidate<TRequest>(this IValidator<TRequest> validator, TRequest request)
    {
        return validator.Validate(request);
    }

    /// <summary>
    /// Asserts that the validation results do not contain the specified error.
    /// </summary>
    /// <param name="errors">The list of errors returned by the validator.</param>
    /// <param name="expectedError">The error that should not be present in the validation results.</param>
    public static void ShouldNotHaveValidationError(this List<Error> errors, Error expectedError)
    {
        Assert.DoesNotContain(errors, error => error.Description == expectedError.Description);
    }

    /// <summary>
    /// Asserts that the validation results contain the specified error.
    /// </summary>
    /// <param name="errors">The list of errors returned by the validator.</param>
    /// <param name="expectedError">The error that should be present in the validation results.</param>
    public static void ShouldHaveValidationError(this List<Error> errors, Error expectedError)
    {
        Assert.Contains(errors, error => error.Description == expectedError.Description);
    }

    /// <summary>
    /// Asserts that the validation results are completely empty, indicating the request is valid.
    /// </summary>
    /// <param name="errors">The list of errors returned by the validator.</param>
    public static void ShouldNotHaveAnyValidationErrors(this List<Error> errors)
    {
        Assert.Empty(errors);
    }
}
