#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Nemera.Application.Common.DataAccess.Entities.Common;

/// <summary>
/// Defines the fixed audit identities used by the persistence layer.
/// </summary>
/// <remarks>
/// The SYSTEM identity is only for the explicit one-time application initialization save. Ordinary anonymous creation and modification remain
/// unattributed with <see cref="Guid.Empty"/>; the SYSTEM identity is never inferred from a missing authenticated user.
/// </remarks>
public static class AuditIdentities
{
    /// <summary>
    /// The fixed identifier of the SYSTEM audit user, used for rows created during application initialization, before any user account exists.
    /// </summary>
    public static readonly Guid s_systemUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
}
