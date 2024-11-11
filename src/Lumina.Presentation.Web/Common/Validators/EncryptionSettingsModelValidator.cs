#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Presentation.Web.Common.Models.Configuration;
using System;
using System.Buffers.Text;
#endregion

namespace Lumina.Presentation.Web.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the EncryptionSettings application configuration settings section.
/// </summary>
public class EncryptionSettingsModelValidator : AbstractValidator<EncryptionSettingsModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptionSettingsModelValidator"/> class.
    /// </summary>
    public EncryptionSettingsModelValidator()
    {
        RuleFor(x => x.SecretKey)
            .NotEmpty().WithMessage("Encryption secret key cannot be empty!")
            .Must(value => Base64.IsValid(value.AsSpan())).WithMessage("Encryption secret key must be a base64 string!");
    }
}
