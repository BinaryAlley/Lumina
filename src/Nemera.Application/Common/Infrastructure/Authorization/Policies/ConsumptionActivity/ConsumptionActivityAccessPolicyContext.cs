#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;

/// <summary>
/// Represents the context required to evaluate the consumption activity authorization policy.
/// </summary>
/// <param name="TargetUserId">The Id of the user whose consumption activity is being accessed.</param>
[DebuggerDisplay("TargetUserId: {TargetUserId}")]
public sealed record ConsumptionActivityAccessPolicyContext(Guid TargetUserId) : PolicyContext;
