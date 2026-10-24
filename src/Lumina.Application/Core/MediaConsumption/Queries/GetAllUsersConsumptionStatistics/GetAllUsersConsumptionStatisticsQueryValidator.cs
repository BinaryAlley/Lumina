#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;

/// <summary>
/// Validates the needed validation rules for <see cref="GetAllUsersConsumptionStatisticsQuery"/>.
/// </summary>
public class GetAllUsersConsumptionStatisticsQueryValidator : AbstractValidator<GetAllUsersConsumptionStatisticsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsQueryValidator"/> class.
    /// </summary>
    public GetAllUsersConsumptionStatisticsQueryValidator()
    {
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

        // An inverted range would silently return no records, so it is rejected as an invalid request instead.
        RuleFor(query => query.FromUtc)
            .Must((query, fromUtc) => !fromUtc.HasValue || !query.ToUtc.HasValue || fromUtc.Value <= query.ToUtc.Value)
            .WithError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }
}
