#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the MediaSettings application configuration settings section.
/// </summary>
public class MediaSettingsDtoValidator : AbstractValidator<MediaSettingsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSettingsDtoValidator"/> class.
    /// </summary>
    public MediaSettingsDtoValidator()
    {
        RuleFor(x => x.RootDirectory).NotEmpty().WithMessage(Errors.Errors.Configuration.MediaRootDirectoryCannotBeEmpty.Description);
        RuleFor(x => x.LibrariesDirectory).NotEmpty().WithMessage(Errors.Errors.Configuration.MediaLibrariesDirectoryCannotBeEmpty.Description);
    }
}
