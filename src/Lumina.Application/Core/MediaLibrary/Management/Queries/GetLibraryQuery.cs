#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries;

/// <summary>
/// Query for getting a media library by its Id.
/// </summary>
/// <param name="Id">The Id of the media library to get.</param>
/// <param name="UserId">The Id of the user requesting the media library.</param>
[DebuggerDisplay("Id: {Id}")]
public record GetLibraryQuery(
    Guid Id,
    Guid UserId
) : IRequest<ErrorOr<LibraryResponse>>;
