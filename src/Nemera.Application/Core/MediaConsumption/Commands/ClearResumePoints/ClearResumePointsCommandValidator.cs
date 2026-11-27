#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.ClearResumePoints;

/// <summary>
/// Validates the needed validation rules for <see cref="ClearResumePointsCommand"/>.
/// </summary>
public class ClearResumePointsCommandValidator : AbstractValidator<ClearResumePointsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearResumePointsCommandValidator"/> class.
    /// </summary>
    public ClearResumePointsCommandValidator()
    {
        // The target defaults to the current user, so an absent target is valid, but a supplied one must be a real user Id.
        RuleFor(command => command.TargetUserId)
            .Must(targetUserId => targetUserId is null ||
                (Guid.TryParse(targetUserId, out Guid parsedTargetUserId) && parsedTargetUserId != Guid.Empty))
            .WithError(Errors.MediaConsumption.TargetUserIdIsInvalid);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => libraryId is null ||
                (Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty))
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
