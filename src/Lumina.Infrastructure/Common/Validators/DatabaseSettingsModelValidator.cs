#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Infrastructure.Common.Models.Configuration;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the ConnectionStrings application configuration settings section.
/// </summary>
public class DatabaseSettingsModelValidator : AbstractValidator<DatabaseSettingsModel>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSettingsModelValidator"/> class.
    /// </summary>
    public DatabaseSettingsModelValidator()
    {
        RuleFor(x => x.DefaultConnection).NotEmpty().WithMessage(Errors.Errors.Configuration.DatabaseConnectionStringCannotBeEmpty.Code);
    }
    #endregion
}