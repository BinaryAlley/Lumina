#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;

/// <summary>
/// Validates the needed validation rules for <see cref="AddLibraryCommand"/>.
/// </summary>
public class AddLibraryCommandValidator : AbstractValidator<AddLibraryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryCommandValidator"/> class.
    /// </summary>
    public AddLibraryCommandValidator()
    {
        RuleFor(command => command.LibraryType)
            .NotNull()
            .WithError(Errors.Library.LibraryTypeCannotBeNull)
            .Must(libraryType => Enum.TryParse<LibraryType>(libraryType, out _))
            .WithError(Errors.Library.UnknownLibraryType);

        RuleFor(command => command.ContentLocations)
            .NotNull()
            .WithError(Errors.Library.PathsListCannotBeNull)
            .NotEmpty()
            .WithError(Errors.Library.PathsListCannotBeEmpty);

        RuleForEach(command => command.ContentLocations)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty)
            .MaximumLength(260)
            .WithError(Errors.FileSystemManagement.PathMustBeMaximum260CharactersLong);

        RuleFor(command => command.Title)
            .NotEmpty()
            .WithError(Errors.Library.TitleCannotBeEmpty)
            .MaximumLength(255)
            .WithError(Errors.Library.TitleMustBeMaximum255CharactersLong);

        RuleFor(command => command.CompletionThresholdPercentage)
            .Must((command, completionThresholdPercentage) => command.ShouldTrackConsumptionProgress != false || completionThresholdPercentage is null)
            .WithError(Errors.Library.CompletionThresholdPercentageMustBeNullWhenProgressTrackingIsOff);

        RuleFor(command => command.CompletionThresholdPercentage)
            .InclusiveBetween(0, 100)
            .WithError(Errors.Library.CompletionThresholdPercentageMustBeBetweenZeroAndOneHundred);

        RuleForEach(command => command.PathTemplateParts)
            .ChildRules(part =>
            {
                part.RuleFor(pathPart => pathPart.Kind)
                    .NotEmpty()
                    .WithError(Errors.Library.PathTemplatePartKindNotSupportedForLibraryType);
                part.RuleFor(pathPart => pathPart.Representation)
                    .NotEmpty()
                    .When(pathPart => !string.Equals(pathPart.Kind, nameof(LibraryPathPartKind.Separator), StringComparison.OrdinalIgnoreCase))
                    .WithError(Errors.Library.PathTemplateLiteralCannotBeEmpty);
            });
    }
}
