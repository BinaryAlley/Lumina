#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetTagConsumption;

/// <summary>
/// Validates the needed validation rules for <see cref="GetTagConsumptionQuery"/>.
/// </summary>
public class GetTagConsumptionQueryValidator : AbstractValidator<GetTagConsumptionQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagConsumptionQueryValidator"/> class.
    /// </summary>
    public GetTagConsumptionQueryValidator()
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

        RuleFor(query => query.TagKind)
            .IsInEnum()
            .WithError(Errors.MediaConsumption.TagKindIsInvalid)
            .When(query => query.TagKind is not null);

        // An inverted range would silently return no records, so it is rejected as an invalid request instead.
        RuleFor(query => query.FromUtc)
            .Must((query, fromUtc) => !fromUtc.HasValue || !query.ToUtc.HasValue || fromUtc.Value <= query.ToUtc.Value)
            .WithError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }
}
