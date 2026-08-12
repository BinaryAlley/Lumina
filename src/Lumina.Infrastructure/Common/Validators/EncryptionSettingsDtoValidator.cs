#region ========================================================================= USING =====================================================================================
using FluentValidation;
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
        RuleFor(x => x.SecretKey)
            .NotEmpty().WithMessage(Errors.Errors.Configuration.EncryptionSecretKeyCannotBeEmpty.Description)
            .Must(value => Base64.IsValid(value.AsSpan())).WithMessage(Errors.Errors.Configuration.EncryptionSecretKeyMustBeABase64String.Description);
    }
}
