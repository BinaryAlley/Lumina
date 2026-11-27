#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Enums.Authorization;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.DTO.Authorization;

/// <summary>
/// Data transfer object for an authorization permission.
/// </summary>
/// <param name="Id">The unique identifier of the permission.</param>
/// <param name="PermissionName">The name of the permission.</param>
[DebuggerDisplay("Id: {Id}; PermissionName: {PermissionName}")]
public record PermissionDto(
    Guid Id,
    AuthorizationPermission PermissionName
);
