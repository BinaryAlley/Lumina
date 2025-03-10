#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Presentation.Web.Common.Models.Configuration;
#endregion

namespace Lumina.Presentation.Web.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the ServerConfiguration application configuration settings section.
/// </summary>
public class ServerConfigurationModelValidator : AbstractValidator<ServerConfigurationModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerConfigurationModelValidator"/> class.
    /// </summary>
    public ServerConfigurationModelValidator()
    {
        RuleFor(x => x.BaseAddress).NotEmpty().WithMessage("Base address cannot be empty!");
        RuleFor(x => x.Port).InclusiveBetween((ushort)0, (ushort)65535).WithMessage("Port number must be between 0 and 65535!");
        RuleFor(x => x.ApiVersion).InclusiveBetween((char)0, (char)255).WithMessage("API version must be between 0 and 255!");
    }
}