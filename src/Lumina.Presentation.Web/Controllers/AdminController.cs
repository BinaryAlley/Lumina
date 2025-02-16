#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Attributes;
using Lumina.Presentation.Web.Common.Models.Authorization;
using Lumina.Presentation.Web.Common.Models.UsersManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers;

/// <summary>
/// Controller for administrator related operations.
/// </summary>
[Authorize]
[RequireRole("Admin")]
[Route("{culture}/admin")]
public class AdminController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public AdminController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Displays the view for managing authorization roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("manage-roles")]
    public async Task<IActionResult> ManageRoles(CancellationToken cancellationToken)
    {
        // get the list of roles and permissions from the API
        ViewData["roles"] = await _apiHttpClient.GetAsync<RoleModel[]>($"auth/roles/", cancellationToken).ConfigureAwait(false);
        ViewData["permissions"] = await _apiHttpClient.GetAsync<PermissionModel[]>($"auth/permissions/", cancellationToken).ConfigureAwait(false);
        return View();
    }

    /// <summary>
    /// Displays the view for managing authorization permissions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("manage-permissions")]
    public async Task<IActionResult> ManagePermissions(CancellationToken cancellationToken)
    {
        ViewData["users"] = await _apiHttpClient.GetAsync<UserModel[]>($"auth/users", cancellationToken).ConfigureAwait(false);
        ViewData["roles"] = await _apiHttpClient.GetAsync<RoleModel[]>($"auth/roles/", cancellationToken).ConfigureAwait(false);
        ViewData["permissions"] = await _apiHttpClient.GetAsync<PermissionModel[]>($"auth/permissions/", cancellationToken).ConfigureAwait(false);
        return View();
    }

    /// <summary>
    /// Gets the authorization permissions of a role identified by <paramref name="roleId"/>.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role for which to get the list of permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-permissions-by-role-id/{roleId}")]
    public async Task<IActionResult> GetPermissionsByRoleId(Guid roleId, CancellationToken cancellationToken)
    {
        RolePermissionsModel response = await _apiHttpClient.GetAsync<RolePermissionsModel>($"auth/roles/{roleId}/permissions", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Gets the authorization permissions of a user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to get the list of permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-permissions-by-user-id/{userId}")]
    public async Task<IActionResult> GetPermissionsByUserId(Guid userId, CancellationToken cancellationToken)
    {
        PermissionModel[] response = await _apiHttpClient.GetAsync<PermissionModel[]>($"auth/users/{userId}/permissions", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Gets the authorization role of a user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to get the role.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-role-by-user-id/{userId}")]
    public async Task<IActionResult> GetRoleByUserId(Guid userId, CancellationToken cancellationToken)
    {
        RoleModel? response = await _apiHttpClient.GetAsync<RoleModel?>($"auth/users/{userId}/role", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Gets the collection of authorization roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        RoleModel[] response = await _apiHttpClient.GetAsync<RoleModel[]>($"auth/roles/", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Creates an authorization role with the specified name and associated permissions.
    /// </summary>
    /// <param name="model">The model containing the data of the role to be created.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPost("api-create-role")]
    public async Task<IActionResult> CreateRole([FromBody] AddRoleRequestModel model, CancellationToken cancellationToken)
    {
        RolePermissionsModel response = await _apiHttpClient.PostAsync<RolePermissionsModel, AddRoleRequestModel>($"auth/roles/", model, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Updates an authorization role with the specified name and associated permissions.
    /// </summary>
    /// <param name="model">The model containing the data of the role to be updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPut("api-update-role")]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequestModel model, CancellationToken cancellationToken)
    {
        RolePermissionsModel response = await _apiHttpClient.PutAsync<RolePermissionsModel, UpdateRoleRequestModel>($"auth/roles/", model, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Deletes an authorization role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpDelete("api-delete-role/{roleId}")]
    public async Task<IActionResult> DeleteRole(Guid roleId, CancellationToken cancellationToken)
    {
        await _apiHttpClient.DeleteAsync($"auth/roles/{roleId}", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true });
    }

    /// <summary>
    /// Updates an authorization role with the specified name and associated permissions.
    /// </summary>
    /// <param name="model">The model containing the data of the role to be updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPut("api-update-user-authorization")]
    public async Task<IActionResult> UpdateUserAuthorization([FromBody] UpdateUserRoleAndPermissionsRequestModel model, CancellationToken cancellationToken)
    {
        GetAuthorizationResponse response = await _apiHttpClient.PutAsync<GetAuthorizationResponse, UpdateUserRoleAndPermissionsRequestModel>($"auth/users/{model.UserId}/role-and-permissions", model, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }
}
