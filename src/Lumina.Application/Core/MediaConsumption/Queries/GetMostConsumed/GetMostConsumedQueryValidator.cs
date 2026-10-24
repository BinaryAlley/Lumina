#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetMostConsumed;

/// <summary>
/// Validates the needed validation rules for <see cref="GetMostConsumedQuery"/>.
/// </summary>
public class GetMostConsumedQueryValidator : AbstractValidator<GetMostConsumedQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedQueryValidator"/> class.
    /// </summary>
    public GetMostConsumedQueryValidator()
    {
        // The target defaults to the current user, so an absent target is valid, but a supplied one must be a real user Id.
        RuleFor(query => query.TargetUserId)
            .Must(targetUserId => targetUserId is null ||
                (Guid.TryParse(targetUserId, out Guid parsedTargetUserId) && parsedTargetUserId != Guid.Empty))
            .WithError(Errors.MediaConsumption.TargetUserIdIsInvalid);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => libraryId is null ||
                (Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty))
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.MediaKind)
            .IsInEnum()
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind)
            .When(query => query.MediaKind is not null);

        RuleFor(query => query.MediaKind)
            .Must(mediaKind => mediaKind is null || mediaKind != MediaKind.None)
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);
    }
}
