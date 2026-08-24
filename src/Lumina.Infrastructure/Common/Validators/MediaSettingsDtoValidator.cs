#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Common.Utilities;
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
        RuleFor(settings => settings.RootDirectory)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.MediaRootDirectoryCannotBeEmpty);
       
        RuleFor(settings => settings.LibrariesDirectory)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.MediaLibrariesDirectoryCannotBeEmpty);

        RuleFor(settings => settings.BooksDirectory)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.MediaBooksDirectoryCannotBeEmpty);
    }
}
