#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Attributes;
using Lumina.Presentation.Web.Common.Models.Authorization;
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
        ViewData["roles"] = await _apiHttpClient.GetAsync<RoleModel[]>($"roles/", cancellationToken).ConfigureAwait(false);
        ViewData["rolePermissions"] = await _apiHttpClient.GetAsync<PermissionModel[]>($"permissions/", cancellationToken).ConfigureAwait(false);
        return View();
    }

    /// <summary>
    /// Displays the view for managing authorization permissions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("manage-permissions")]
    public async Task<IActionResult> ManagePermissions(CancellationToken cancellationToken)
    {
        ViewData["Roles"] = await _apiHttpClient.GetAsync<RoleModel>($"roles/", cancellationToken).ConfigureAwait(false);
        return View();
    }

    /// <summary>
    /// Gets the authorization permissions of a role identified by <paramref name="roleId"/>.
    /// </summary>
    /// <param name="roleId">The Id of the role for which to get the list of permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-permissions-by-role-id/{roleId}")]
    public async Task<IActionResult> GetPermissionsByRoleId(Guid roleId, CancellationToken cancellationToken)
    {
        RolePermissionsModel response = await _apiHttpClient.GetAsync<RolePermissionsModel>($"roles/{roleId}/permissions", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Gets the collection of authorization roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        RoleModel[] response = await _apiHttpClient.GetAsync<RoleModel[]>($"roles/", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Creates an authorization role with the specified name and associated permissions.
    /// </summary>
    /// <param name="model">The model containing the data of the role to be created.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPost("create-role")]
    public async Task<IActionResult> CreateRole([FromBody] AddRoleRequestModel model, CancellationToken cancellationToken)
    {
        RolePermissionsModel response = await _apiHttpClient.PostAsync<RolePermissionsModel, AddRoleRequestModel>($"roles/", model, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Updates an authorization role with the specified name and associated permissions.
    /// </summary>
    /// <param name="model">The model containing the data of the role to be updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpPost("update-role")]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequestModel model, CancellationToken cancellationToken)
    {
        RolePermissionsModel response = await _apiHttpClient.PutAsync<RolePermissionsModel, UpdateRoleRequestModel>($"roles/", model, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Deletes an authorization role.
    /// </summary>
    /// <param name="roleId">The Id of the role to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpDelete("delete-role/{roleId}")]
    public async Task<IActionResult> DeleteRole(Guid roleId, CancellationToken cancellationToken)
    {
        await _apiHttpClient.DeleteAsync($"roles/{roleId}", cancellationToken).ConfigureAwait(false);
        return Json(new { success = true });
    }
}
