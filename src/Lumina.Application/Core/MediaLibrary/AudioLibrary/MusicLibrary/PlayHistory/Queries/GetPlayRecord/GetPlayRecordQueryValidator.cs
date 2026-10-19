#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetPlayRecord;

/// <summary>
/// Validates the needed validation rules for <see cref="GetPlayRecordQuery"/>.
/// </summary>
public class GetPlayRecordQueryValidator : AbstractValidator<GetPlayRecordQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlayRecordQueryValidator"/> class.
    /// </summary>
    public GetPlayRecordQueryValidator()
    {
        RuleFor(query => query.PlayRecordId)
            .NotEmpty()
            .WithError(Errors.MusicPlayHistory.PlayRecordIdCannotBeEmpty);

        RuleFor(query => query.PlayRecordId)
            .Must(playRecordId => Guid.TryParse(playRecordId, out Guid parsedPlayRecordId) && parsedPlayRecordId != Guid.Empty)
            .When(query => query.PlayRecordId is not null && query.PlayRecordId.Length > 0)
            .WithError(Errors.MusicPlayHistory.PlayRecordIdCannotBeEmpty);
    }
}
