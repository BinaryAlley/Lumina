#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetPluginSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPluginSettingsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsEndpointSummary : Summary<GetPluginSettingsEndpoint, GetPluginSettingsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsEndpointSummary"/> class.
    /// </summary>
    public GetPluginSettingsEndpointSummary()
    {
        Summary = "Retrieves the settings of a plugin and their schema.";
        Description = "Retrieves the settings of a plugin and their schema, used to render the plugin settings page.";

        ExampleRequest = new GetPluginSettingsRequest(
            PluginId: Guid.NewGuid()
        );

        RequestParam(r => r.PluginId, "The unique identifier of the plugin. Required.");

        ResponseParam<PluginSettingsResponse>(r => r.PluginId, "The unique identifier of the plugin.");
        ResponseParam<PluginSettingsResponse>(r => r.Schema, "The schema of the plugin settings, used to render the settings form.");
        ResponseParam<PluginSettingsResponse>(r => r.Settings, "The current values of the plugin settings.");

        Response(200, "The settings of the plugin and their schema are returned.", example: new PluginSettingsResponse(
            PluginId: Guid.NewGuid(),
            Schema:
            [
                new PluginSettingDescriptorResponse(
                    Key: "preferredLanguage",
                    Label: "Preferred Language",
                    Type: PluginSettingType.Text,
                    DefaultValue: "en",
                    AllowedValues: null
                ),
                new PluginSettingDescriptorResponse(
                    Key: "selectionStrategy",
                    Label: "Selection Strategy",
                    Type: PluginSettingType.Select,
                    DefaultValue: "first",
                    AllowedValues: ["first", "best"]
                )
            ],
            Settings: new Dictionary<string, string>
            {
                { "preferredLanguage", "fr" },
                { "selectionStrategy", "first" }
            }
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
                    instance = "/api/v1/plugins/{pluginId}/settings"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/plugins/{pluginId}/settings"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/plugins/{pluginId}/settings"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/plugins/{pluginId}/settings",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested plugin does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "PluginNotFound",
                instance = "/api/v1/plugins/{pluginId}/settings",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/plugins/{pluginId}/settings",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PluginIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
