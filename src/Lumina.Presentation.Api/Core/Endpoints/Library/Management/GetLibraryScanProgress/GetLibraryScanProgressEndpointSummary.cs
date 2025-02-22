#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibraryScanProgress;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryScanProgressEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryScanProgressEndpointSummary : Summary<GetLibraryScanProgressEndpoint, GetLibraryScanProgressRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryScanProgressEndpointSummary"/> class.
    /// </summary>
    public GetLibraryScanProgressEndpointSummary()
    {
        Summary = "Retrieves the progress of a media library scan.";
        Description = "Retrieves the progress of a media library scan, if the user making the request is an Admin, or the owner of the library.";

        ExampleRequest = new GetLibraryScanProgressRequest(
            LibraryId: Guid.NewGuid(),
            ScanId: Guid.NewGuid()
        );

        RequestParam(r => r.LibraryId, "The Id of the media library whose scan progress is requested. Required.");
        RequestParam(r => r.ScanId, "The Id of the media library scan whose progress is requested. Required.");

        ResponseParam<MediaLibraryScanProgressResponse>(r => r.ScanId, "The object representing the unique identifier of the media library scan.");
        ResponseParam<MediaLibraryScanProgressResponse>(r => r.UserId, "The object representing the unique identifier of the user initiating this media library scan.");
        ResponseParam<MediaLibraryScanProgressResponse>(r => r.LibraryId, "The object representing the unique identifier of the media library that is scanned.");
        ResponseParam<MediaLibraryScanProgressResponse>(r => r.TotalJobs, "The total number of jobs to be processed by the scan.");
        ResponseParam<MediaLibraryScanProgressResponse>(r => r.CompletedJobs, "The number of jobs that have been processed.");
        ResponseParam<MediaLibraryScanProgressResponse>(r => r.CurrentJobProgress, "The progress of the currently processing job.");
        ResponseParam<MediaLibraryScanProgressResponse>(r => r.Status, "The status of the scan.");
        ResponseParam<MediaLibraryScanProgressResponse>(r => r.OverallProgressPercentage, "The ratio between the number of processed jobs and the total number of jobs to process, as percentage.");

        Response(200, "The progress of the media library scan is returned.", example: new MediaLibraryScanProgressResponse(
            ScanId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            LibraryId: Guid.NewGuid(),
            TotalJobs: 10,
            CompletedJobs: 4,
            CurrentJobProgress: new MediaLibraryScanJobProgressResponse(
                TotalItems: 1536,
                CompletedItems: 768,
                CurrentOperation: "Comparing file hashes",
                ProgressPercentage: 50
            ),
            Status: LibraryScanJobStatus.Running.ToString(),
            OverallProgressPercentage: 40
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
                    instance = "/api/v1/libraries/{libraryId}/scans/{scanId}/progress"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/{libraryId}/scans/{scanId}/progress"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/{libraryId}/scans/{scanId}/progress"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/libraries/{libraryId}/scans/{scanId}/progress",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested media library has no scan started.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "LibraryScanNotFound",
                instance = "/api/v1/libraries/{libraryId}/scans/{scanId}/progress",
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
                instance = "/api/v1/libraries/{libraryId}/scans/{scanId}/progress",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "LibraryScanIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
