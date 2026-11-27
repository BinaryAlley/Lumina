#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the ConnectionStrings application configuration settings section.
/// </summary>
public class DatabaseSettingsDtoValidator : AbstractValidator<DatabaseSettingsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSettingsDtoValidator"/> class.
    /// </summary>
    public DatabaseSettingsDtoValidator()
    {
        RuleFor(settings => settings.DefaultConnection)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.DatabaseConnectionStringCannotBeEmpty);
    }
}
