#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetPlugins;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPluginsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginsEndpointSummary : Summary<GetPluginsEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsEndpointSummary"/> class.
    /// </summary>
    public GetPluginsEndpointSummary()
    {
        Summary = "Retrieves the list of detected plugins.";
        Description = "Retrieves the list of all the plugins detected by the host application, including their load status and settings.";

        ResponseParam<PluginResponse>(r => r.Id, "The unique identifier of the plugin.");
        ResponseParam<PluginResponse>(r => r.Name, "The display name of the plugin.");
        ResponseParam<PluginResponse>(r => r.Author, "The author of the plugin.");
        ResponseParam<PluginResponse>(r => r.Version, "The version of the plugin.");
        ResponseParam<PluginResponse>(r => r.Description, "The description of the plugin.");
        ResponseParam<PluginResponse>(r => r.LoadStatus, "The load status of the plugin.");
        ResponseParam<PluginResponse>(r => r.LoadError, "The error message when the plugin failed to load, if applicable.");
        ResponseParam<PluginResponse>(r => r.Settings, "The settings of the plugin.");

        Response(200, "The list of detected plugins is returned.",
            example: new PluginResponse[] {
                new(
                    Id: Guid.NewGuid(),
                    Name: "Goodreads",
                    Author: "Lumina",
                    Version: "1.0.0",
                    Description: "Provides book metadata from Goodreads.",
                    LoadStatus: PluginLoadStatus.Loaded,
                    LoadError: null,
                    Settings: new Dictionary<string, string>
                    {
                        { "preferredLanguage", "en" },
                        { "selectionStrategy", "first" }
                    }
                )
            }
        );


        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/plugins"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/plugins"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/plugins"
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
                instance = "/api/v1/plugins",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );
    }
}
