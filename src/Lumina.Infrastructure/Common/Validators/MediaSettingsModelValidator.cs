#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Infrastructure.Common.Models.Configuration;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the MediaSettings application configuration settings section.
/// </summary>
public class MediaSettingsModelValidator : AbstractValidator<MediaSettingsModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSettingsModelValidator"/> class.
    /// </summary>
    public MediaSettingsModelValidator()
    {
        RuleFor(x => x.RootDirectory).NotEmpty().WithMessage(Errors.Errors.Configuration.MediaRootDirectoryCannotBeEmpty.Description);
    }
}
