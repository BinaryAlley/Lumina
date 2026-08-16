#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;

/// <summary>
/// Represents the context required to evaluate the library ownership authorization policy.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose access is being evaluated.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public sealed record LibraryOwnershipPolicyContext(Guid LibraryId) : PolicyContext;
