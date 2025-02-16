#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibrary;

/// <summary>
/// Query for getting a media library by its Id.
/// </summary>
/// <param name="Id">The unique identifier of the media library to get.</param>
[DebuggerDisplay("Id: {Id}")]
public record GetLibraryQuery(
    Guid Id
) : IRequest<ErrorOr<LibraryResponse>>;
