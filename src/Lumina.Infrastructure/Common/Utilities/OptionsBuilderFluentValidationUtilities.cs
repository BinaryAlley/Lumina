#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Infrastructure.Common.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#endregion

namespace Lumina.Infrastructure.Common.Utilities;

/// <summary>
/// Extension methods for adding configuration related options services to the DI container via <see cref="OptionsBuilder{TOptions}"/>.
/// </summary>
public static class OptionsBuilderFluentValidationUtilities
{
    #region ================================================================== METHODS ===================================================================================
    /// <summary>
    /// Extension method to add fluent validation to the options validation pipeline.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being validated.</typeparam>
    /// <param name="optionsBuilder">The options builder to add the validation to.</param>
    /// <remarks>
    /// This method allows validating options of type <typeparamref name="TOptions"/> using a FluentValidation validator registered in the DI container.
    /// </remarks>
    /// <returns>The same <see cref="OptionsBuilder{TOptions}"/> instance, to allow for chaining.</returns>
    public static OptionsBuilder<TOptions> ValidateFluently<TOptions>(this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        // register a singleton instance of IValidateOptions<TOptions> using FluentValidation
        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(
            serviceProvider => new FluentValidationOptions<TOptions>(optionsBuilder.Name, serviceProvider.GetRequiredService<IValidator<TOptions>>()));
        return optionsBuilder;
    }
    #endregion
}