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

        RuleFor(command => command.MediaKind)
            .IsInEnum()
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        RuleFor(command => command.MediaKind)
            .Must(mediaKind => mediaKind != MediaKind.None)
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        RuleFor(command => command.MediaItemId)
            .Must(mediaItemId => mediaItemId != Guid.Empty)
            .WithError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => libraryId != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
