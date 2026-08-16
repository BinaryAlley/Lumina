#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.UpdateUserSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateUserSettingsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsEndpointSummary : Summary<UpdateUserSettingsEndpoint, UpdateUserSettingsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpointSummary"/> class.
    /// </summary>
    public UpdateUserSettingsEndpointSummary()
    {
        Summary = "Updates the settings of the current user.";
        Description = "Updates the settings of the current user. When no settings are stored for the user yet, they are created with the provided values.";

        ExampleRequest = new UpdateUserSettingsRequest(
            IsPaginationEnabled: true,
            ItemsPerPage: 48,
            IgnoreThePrefixForAlphaPicker: false
        );

        Response(200, "The settings of the current user were successfully updated.");

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

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/users/me/settings",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ItemsPerPageMustBeGreaterThanZero"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
