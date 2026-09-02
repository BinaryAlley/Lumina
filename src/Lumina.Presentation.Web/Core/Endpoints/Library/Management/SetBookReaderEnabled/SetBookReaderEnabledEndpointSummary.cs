#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.SetBookReaderEnabled;

/// <summary>
/// Class used for providing a textual description for the <see cref="SetBookReaderEnabledEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetBookReaderEnabledEndpointSummary : Summary<SetBookReaderEnabledEndpoint, SetBookReaderEnabledRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetBookReaderEnabledEndpointSummary"/> class.
    /// </summary>
    public SetBookReaderEnabledEndpointSummary()
    {
        Summary = "Enables or disables a book reader of a media library.";
        Description = "Enables or disables the book reader of the media library identified by the request.";
        RequestParam(r => r.LibraryId, "The Id of the media library whose book reader is enabled or disabled. Required.");
        RequestParam(r => r.PluginId, "The unique identifier of the plugin providing the book reader. Required.");
        RequestParam(r => r.IsEnabled, "Whether the book reader should be enabled for the media library, or not. Required.");

        ExampleRequest = new SetBookReaderEnabledRequest
        {
            LibraryId = Guid.NewGuid(),
            PluginId = Guid.NewGuid(),
            IsEnabled = true
        };

        Response(200, "The book reader of the media library is enabled or disabled.", example: new SuccessResponse(true));
    }
}
