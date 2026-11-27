#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.GetUserSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetUserSettingsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsEndpointSummary : Summary<GetUserSettingsEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsEndpointSummary"/> class.
    /// </summary>
    public GetUserSettingsEndpointSummary()
    {
        Summary = "Retrieves the settings of the current user.";
        Description = "Retrieves the settings of the current user.";

        Response(200, "The settings of the current user are returned.", example: new SuccessResponse<UserSettingsDto>(true, default));
    }
}
