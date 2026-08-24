#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.UpdateUserSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateUserSettingsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsEndpointSummary : Summary<UpdateUserSettingsEndpoint, UserSettingsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpointSummary"/> class.
    /// </summary>
    public UpdateUserSettingsEndpointSummary()
    {
        Summary = "Updates the settings of the current user.";
        Description = "Updates the settings of the current user.";
        RequestParam(r => r.UserId, "The unique identifier of the user that owns the settings. Optional.");
        RequestParam(r => r.IsPaginationEnabled, "Whether pagination is enabled for the user, or not. Optional.");
        RequestParam(r => r.ItemsPerPage, "The number of library items displayed per page when pagination is enabled. Optional.");
        RequestParam(r => r.ShouldIgnoreThePrefixForAlphaPicker, "Whether the leading 'The' prefix of library item titles is ignored by the alpha picker, or not. Optional.");

        ExampleRequest = new UserSettingsDto();

        Response(200, "The settings of the current user are updated.", example: new { success = true, data = new { isUpdated = true } });
    }
}
