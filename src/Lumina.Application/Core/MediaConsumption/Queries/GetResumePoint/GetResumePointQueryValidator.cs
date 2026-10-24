#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetResumePoint;

/// <summary>
/// Validates the needed validation rules for <see cref="GetResumePointQuery"/>.
/// </summary>
public class GetResumePointQueryValidator : AbstractValidator<GetResumePointQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetResumePointQueryValidator"/> class.
    /// </summary>
    public GetResumePointQueryValidator()
    {
        // Validates the kind of media to resume, taken from the route.
        RuleFor(query => query.MediaKind)
            .NotEmpty()
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        RuleFor(query => query.MediaKind)
            .Must(mediaKind => Enum.TryParse(mediaKind, ignoreCase: true, out MediaKind parsedMediaKind) && Enum.IsDefined(parsedMediaKind))
            .When(query => query.MediaKind is not null && query.MediaKind.Length > 0)
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        // Validates the identifier of the media item to resume, taken from the route.
        RuleFor(query => query.MediaItemId)
            .NotEmpty()
            .WithError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);

        RuleFor(query => query.MediaItemId)
            .Must(mediaItemId => Guid.TryParse(mediaItemId, out Guid parsedMediaItemId) && parsedMediaItemId != Guid.Empty)
            .When(query => query.MediaItemId is not null && query.MediaItemId.Length > 0)
            .WithError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);
    }
}
