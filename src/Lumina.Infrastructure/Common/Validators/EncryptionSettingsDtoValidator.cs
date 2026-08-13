#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using System;
using System.Buffers.Text;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the EncryptionSettings application configuration settings section.
/// </summary>
public class EncryptionSettingsDtoValidator : AbstractValidator<EncryptionSettingsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptionSettingsDtoValidator"/> class.
    /// </summary>
    public EncryptionSettingsDtoValidator()
    {
        RuleFor(settings => settings.SecretKey)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.EncryptionSecretKeyCannotBeEmpty)
            .Must(value => Base64.IsValid(value.AsSpan()))
            .WithError(Errors.Errors.Configuration.EncryptionSecretKeyMustBeABase64String);
    }
}
