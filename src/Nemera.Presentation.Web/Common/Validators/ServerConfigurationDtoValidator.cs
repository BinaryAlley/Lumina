#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Configuration;
using Nemera.Presentation.Web.Common.Primitives;
using Nemera.Presentation.Web.Common.Utilities;
using Nemera.Presentation.Web.Common.Validation;
#endregion

namespace Nemera.Presentation.Web.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the ServerConfiguration application configuration settings section.
/// </summary>
public class ServerConfigurationDtoValidator : AbstractValidator<ServerConfigurationDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerConfigurationDtoValidator"/> class.
    /// </summary>
    public ServerConfigurationDtoValidator()
    {
        RuleFor(configuration => configuration.BaseAddress)
            .NotEmpty()
            .WithError(Error.Validation(description: "Base address cannot be empty!"));

        RuleFor(configuration => configuration.Port)
            .InclusiveBetween((ushort)0, (ushort)65535)
            .WithError(Error.Validation(description: "Port number must be between 0 and 65535!"));

        RuleFor(configuration => configuration.ApiVersion)
            .InclusiveBetween((char)0, (char)255)
            .WithError(Error.Validation(description: "API version must be between 0 and 255!"));
    }
}
