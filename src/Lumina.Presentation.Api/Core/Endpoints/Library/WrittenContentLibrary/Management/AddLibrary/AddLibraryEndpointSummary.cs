#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System.Diagnostics.CodeAnalysis;


#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.Management.AddLibrary;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddLibraryEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryEndpointSummary : Summary<AddLibraryEndpoint, AddLibraryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryEndpointSummary"/> class.
    /// </summary>
    public AddLibraryEndpointSummary()
    {
        Summary = "Adds a new media library.";
        Description = "Creates a new media library and returns its details, including the location of the newly created resource.";

        RequestParam(r => r.UserId, "The Id of the user owning the media library.");
        RequestParam(r => r.Title, "The title of the media library.");
        RequestParam(r => r.LibraryType, "The type of the media library.");
        RequestParam(r => r.ContentLocations, ">The file system paths of the directories where the media library elements are located.");

        ResponseParam<LibraryResponse>(r => r.Id, "The unique identifier of the entity.");
        ResponseParam<LibraryResponse>(r => r.UserId, "The Id of the user owning the media library.");
        ResponseParam<LibraryResponse>(r => r.Title, "The title of the media library.");
        ResponseParam<LibraryResponse>(r => r.LibraryType, "The type of the media library.");
        ResponseParam<LibraryResponse>(r => r.ContentLocations, "The file system paths of the directories where the media library elements are located.");
        ResponseParam<LibraryResponse>(r => r.Created, "The date and time when the entity was created.");
        ResponseParam<LibraryResponse>(r => r.Updated, "The date and time when the entity was last updated.");
        Response<LibraryResponse>(201, "The media library was successfully created.", "The location of the created resource is provided in the Location header.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
