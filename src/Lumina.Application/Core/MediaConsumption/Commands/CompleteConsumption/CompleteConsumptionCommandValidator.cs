#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.CompleteConsumption;

/// <summary>
/// Validates the needed validation rules for <see cref="CompleteConsumptionCommand"/>.
/// </summary>
public class CompleteConsumptionCommandValidator : AbstractValidator<CompleteConsumptionCommand>
{
    /// <summary>
    /// The maximum active duration, in seconds, that a client can report for one session or as a position within a media item. The limit keeps
    /// accumulated totals representable as a <see cref="TimeSpan"/> and rejects durations that cannot describe real media.
    /// </summary>
    private const double MAXIMUM_REPORTED_DURATION_IN_SECONDS = 31 * 24 * 60 * 60;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteConsumptionCommandValidator"/> class.
    /// </summary>
    public CompleteConsumptionCommandValidator()
    {
        // Validates the identifier of the consumption record, taken from the route.
        RuleFor(command => command.ConsumptionRecordId)
            .NotEmpty()
            .WithError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);

        RuleFor(command => command.ConsumptionRecordId)
            .Must(consumptionRecordId => Guid.TryParse(consumptionRecordId, out Guid parsedConsumptionRecordId) && parsedConsumptionRecordId != Guid.Empty)
            .When(command => command.ConsumptionRecordId is not null && command.ConsumptionRecordId.Length > 0)
            .WithError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);

        // Validates that the reported position describes exactly one position, using the representation its kind names.
        RuleFor(command => command.PositionKind)
            .Must((command, positionKind) => IsPositionShapeValid(
                positionKind, command.PositionTimeOffsetInSeconds, command.PositionLocation))
            .WithError(Errors.MediaConsumption.InvalidMediaPosition);

        RuleFor(command => command.ProgressFraction)
            .NotNull()
            .WithError(Errors.MediaConsumption.InvalidProgressFraction);

        RuleFor(command => command.ProgressFraction)
            .Must(progressFraction => progressFraction is null ||
                (double.IsFinite(progressFraction.Value) && progressFraction.Value >= 0 && progressFraction.Value <= 1))
            .WithError(Errors.MediaConsumption.InvalidProgressFraction);

        RuleFor(command => command.ConsumedDurationInSeconds)
            .NotNull()
            .WithError(Errors.MediaConsumption.ConsumedDurationMustBeNonNegative);

        RuleFor(command => command.ConsumedDurationInSeconds)
            .Must(consumedDuration => consumedDuration is null ||
                (double.IsFinite(consumedDuration.Value) && consumedDuration.Value >= 0))
            .WithError(Errors.MediaConsumption.ConsumedDurationMustBeNonNegative);

        RuleFor(command => command.ConsumedDurationInSeconds)
            .Must(consumedDuration => consumedDuration is null ||
                consumedDuration.Value <= MAXIMUM_REPORTED_DURATION_IN_SECONDS)
            .WithError(Errors.MediaConsumption.ConsumedDurationIsOutOfRange);

        RuleFor(command => command.ConsecutivePagesRead)
            .Must(consecutivePagesRead => consecutivePagesRead is null || consecutivePagesRead.Value >= 0)
            .WithError(Errors.MediaConsumption.ConsecutivePagesReadMustBeNonNegative);
    }

    /// <summary>
    /// Determines whether the reported position fields describe exactly one valid position.
    /// </summary>
    /// <param name="positionKind">The name of the kind of the reported position, or <see langword="null"/> when no position was reported.</param>
    /// <param name="timeOffsetInSeconds">The reported position within the media item, in seconds, when the position is a time offset.</param>
    /// <param name="location">The reported opaque locator within the media item, when the position is not a time offset.</param>
    /// <returns><see langword="true"/> if the position fields are valid, <see langword="false"/> otherwise.</returns>
    private static bool IsPositionShapeValid(string? positionKind, double? timeOffsetInSeconds, string? location)
    {
        // No kind means no position was reported, so a time offset or a locator without a kind would make the representation ambiguous.
        if (string.IsNullOrWhiteSpace(positionKind))
            return !timeOffsetInSeconds.HasValue && string.IsNullOrWhiteSpace(location);
        // Unknown and numeric kind names are not valid positions, even when the other fields look well formed.
        if (!Enum.TryParse(positionKind, ignoreCase: true, out MediaPositionKind parsedPositionKind) || !Enum.IsDefined(parsedPositionKind))
            return false;

        // A position uses exactly one representation, because MediaPosition stores one kind and one value, so the fields of the other representation must be absent instead of ignored.
        return parsedPositionKind switch
        {
            MediaPositionKind.TimeOffset => timeOffsetInSeconds.HasValue &&
                double.IsFinite(timeOffsetInSeconds.Value) &&
                timeOffsetInSeconds.Value >= 0 &&
                timeOffsetInSeconds.Value <= MAXIMUM_REPORTED_DURATION_IN_SECONDS &&
                string.IsNullOrWhiteSpace(location),
            MediaPositionKind.Location => !timeOffsetInSeconds.HasValue &&
                !string.IsNullOrWhiteSpace(location) &&
                location.Length <= 4096,
            _ => false
        };
    }
}
