#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Infrastructure.Common.Models.Configuration;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the CommonSettings application configuration settings section.
/// </summary>
public class CommonSettingsModelValidator : AbstractValidator<CommonSettingsModel>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="CommonSettingsModelValidator"/> class.
    /// </summary>
    public CommonSettingsModelValidator()
    {
        RuleFor(x => x.Theme).NotEmpty().WithMessage(Errors.Errors.Configuration.ApplicationThemeCannotBeEmpty.Code);
    }
    #endregion
}