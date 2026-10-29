#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.StartConsumption;

/// <summary>
/// Validates the needed validation rules for <see cref="StartConsumptionCommand"/>.
/// </summary>
public class StartConsumptionCommandValidator : AbstractValidator<StartConsumptionCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionCommandValidator"/> class.
    /// </summary>
    public StartConsumptionCommandValidator()
    {
        // Validates the identifier of the media library that contains the media item, taken from the route.
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(command => command.LibraryId is not null && command.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the kind of media that is consumed, taken from the route. Only the member names are accepted, so a numeric route value that happens to parse as an enumeration value is rejected. Kinds that are not supported by the current catalogues are rejected by the handler, because that is a business rule.
        RuleFor(command => command.MediaKind)
            .NotEmpty()
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        RuleFor(command => command.MediaKind)
            .Must(mediaKind => Enum.GetNames<MediaKind>().Contains(mediaKind, StringComparer.OrdinalIgnoreCase))
            .When(command => command.MediaKind is not null && command.MediaKind.Length > 0)
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        // Validates the identifier of the consumed media item, taken from the route.
        RuleFor(command => command.MediaItemId)
            .NotEmpty()
            .WithError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);

        RuleFor(command => command.MediaItemId)
            .Must(mediaItemId => Guid.TryParse(mediaItemId, out Guid parsedMediaItemId) && parsedMediaItemId != Guid.Empty)
            .When(command => command.MediaItemId is not null && command.MediaItemId.Length > 0)
            .WithError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);
    }
}
