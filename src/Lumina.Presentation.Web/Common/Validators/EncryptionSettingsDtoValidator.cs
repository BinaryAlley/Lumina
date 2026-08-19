#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Utilities;
using Lumina.Presentation.Web.Common.Validation;
using System;
using System.Buffers.Text;
#endregion

namespace Lumina.Presentation.Web.Common.Validators;

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
            .WithError(Error.Validation(description: "Encryption secret key cannot be empty!"))
            .Must(secretKey => Base64.IsValid(secretKey.AsSpan()))
            .WithError(Error.Validation(description: "Encryption secret key must be a base64 string!"));
    }
}
