#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Globalization;
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

        // Only the member names of the media kind enumeration are accepted, so a numeric value that happens to parse as a kind is rejected.
        RuleFor(query => query.MediaKind)
            .IsInEnum<GetAllUsersConsumptionStatisticsQuery, MediaKind>()
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        RuleFor(query => query.MediaKind)
            .Must(mediaKind => mediaKind is null ||
                !string.Equals(mediaKind, nameof(MediaKind.None), StringComparison.OrdinalIgnoreCase))
            .WithError(Errors.MediaConsumption.UnsupportedMediaKind);

        // A malformed bound, or an inverted range, would silently return no records, so both are rejected as an invalid request instead.
        RuleFor(query => query.FromUtc)
            .Must(fromUtc => fromUtc is null || DateTime.TryParse(
                fromUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out _))
            .WithError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);

        RuleFor(query => query.ToUtc)
            .Must(toUtc => toUtc is null || DateTime.TryParse(
                toUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out _))
            .WithError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);

        RuleFor(query => query.FromUtc)
            .Must((query, fromUtc) =>
            {
                if (fromUtc is null || query.ToUtc is null ||
                    !DateTime.TryParse(fromUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime parsedFromUtc) ||
                    !DateTime.TryParse(query.ToUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime parsedToUtc))
                    return true;
                return parsedFromUtc <= parsedToUtc;
            })
            .WithError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }
}
