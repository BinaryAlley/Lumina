#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetUserSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetUserSettingsEndpoint"/> API endpoint, for OpenAPI.
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
        Description = "Retrieves the settings of the current user. When no settings are stored for the user yet, the default settings are returned.";

        Response(200, "The settings of the current user are returned.",
            example: new UserSettingsResponse(
                UserId: Guid.NewGuid(),
                IsPaginationEnabled: true,
                ItemsPerPage: 48,
                IgnoreThePrefixForAlphaPicker: false,
                IsThemeCachingEnabled: true
            ));

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/users/me/settings"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/users/me/settings"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/settings"
                }
            }
        );
    }
}
