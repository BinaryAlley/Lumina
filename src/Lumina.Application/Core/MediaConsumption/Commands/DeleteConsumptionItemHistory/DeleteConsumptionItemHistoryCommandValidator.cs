#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;

/// <summary>
/// Validates the needed validation rules for <see cref="DeleteConsumptionItemHistoryCommand"/>.
/// </summary>
public class DeleteConsumptionItemHistoryCommandValidator : AbstractValidator<DeleteConsumptionItemHistoryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionItemHistoryCommandValidator"/> class.
    /// </summary>
    public DeleteConsumptionItemHistoryCommandValidator()
    {
        // The target defaults to the current user, so an absent target is valid, but a supplied one must be a real user Id.
        RuleFor(command => command.TargetUserId)
            .Must(targetUserId => targetUserId is null ||
                (Guid.TryParse(targetUserId, out Guid parsedTargetUserId) && parsedTargetUserId != Guid.Empty))
            .WithError(Errors.MediaConsumption.TargetUserIdIsInvalid);

        // Only the member names of the media kind enumeration are accepted, so a numeric value that happens to parse as a kind is rejected.
        RuleFor(command => command.MediaKind)
            .NotEmpty()
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        RuleFor(command => command.MediaKind)
            .IsInEnum<DeleteConsumptionItemHistoryCommand, MediaKind>()
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        RuleFor(command => command.MediaKind)
            .Must(mediaKind => mediaKind is null ||
                !string.Equals(mediaKind, nameof(MediaKind.None), StringComparison.OrdinalIgnoreCase))
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        RuleFor(command => command.MediaItemId)
            .NotEmpty()
            .WithError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);

        RuleFor(command => command.MediaItemId)
            .Must(mediaItemId => Guid.TryParse(mediaItemId, out Guid parsedMediaItemId) && parsedMediaItemId != Guid.Empty)
            .When(command => command.MediaItemId is not null && command.MediaItemId.Length > 0)
            .WithError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(command => command.LibraryId is not null && command.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
