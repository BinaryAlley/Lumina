#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Authentication;

/// <summary>
/// Data transfer object for an authorization permission.
/// </summary>
/// <param name="Id">The Id of the permission.</param>
/// <param name="PermissionName">The name of the permission.</param>
[DebuggerDisplay("Id: {Id}, PermissionName: {PermissionName}")]
public record PermissionDto(
    Guid Id,
    AuthorizationPermission PermissionName
);
