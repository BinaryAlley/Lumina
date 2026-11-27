#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.Networking;
#endregion

namespace Nemera.Infrastructure.Common.Networking;

/// <summary>
/// Represents the outcome of validating a host against the outbound address policy.
/// </summary>
/// <param name="IsAllowed">Whether the host may be reached.</param>
/// <param name="RejectionReason">The reason the host may not be reached, or <see cref="OutboundRequestRejectionReason.None"/> when it may.</param>
internal readonly record struct PublicHostValidationResult(
    bool IsAllowed,
    OutboundRequestRejectionReason RejectionReason
)
{
    /// <summary>
    /// Gets the result of a host that passed the validation.
    /// </summary>
    public static PublicHostValidationResult Allowed => new(true, OutboundRequestRejectionReason.None);

    /// <summary>
    /// Creates the result of a host that failed the validation.
    /// </summary>
    /// <param name="rejectionReason">The reason the host may not be reached.</param>
    /// <returns>The result of the rejected host.</returns>
    public static PublicHostValidationResult Rejected(OutboundRequestRejectionReason rejectionReason)
    {
        return new(false, rejectionReason);
    }
}
