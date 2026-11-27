#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Infrastructure.Common.Models.DTO.Configuration;
#endregion

namespace Nemera.Infrastructure.Common.Validators;

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
