#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Common.Validation;

/// <summary>
/// Provides options for fluent validation of a specific options type.
/// </summary>
/// <typeparam name="TOptions">The type of options being validated.</typeparam>
public class FluentValidationOptions<TOptions> : IValidateOptions<TOptions> where TOptions : class
{
    private readonly IValidator<TOptions> _validator;

    /// <summary>
    /// Gets the name of the options being validated.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationOptions{TOptions}"/> class.
    /// </summary>
    /// <param name="name">The name of the options being validated. This can be <see langword="null"/>.</param>
    /// <param name="validator">The validator instance used to validate the options.</param>
    public FluentValidationOptions(string? name, IValidator<TOptions> validator)
    {
        Name = name;
        _validator = validator;
    }

    /// <summary>
    /// Validates the specified <paramref name="options"/>.
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> that indicates whether the validation was successful or not.</returns>
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        // check if the name of the options is specified and matches the provided one
        if (Name is not null && Name != name)
            return ValidateOptionsResult.Skip;

        ArgumentNullException.ThrowIfNull(options, nameof(options)); // don't go further if the options are null

        FluentValidation.Results.ValidationResult validationResult = _validator.Validate(options); // validate the options using the validator

        // return a response based on the validation result
        if (validationResult.IsValid)
            return ValidateOptionsResult.Success;

        // if the validation fails, collect the error messages
        // TODO: add translation support for the error messages
        string[] errors = validationResult.Errors.Select(x => $"Options validation failed for '{x.PropertyName}' with error: '{x.ErrorMessage}'")
                                                 .ToArray();

        // and return them with a failure result
        return ValidateOptionsResult.Fail(errors);
    }
}
