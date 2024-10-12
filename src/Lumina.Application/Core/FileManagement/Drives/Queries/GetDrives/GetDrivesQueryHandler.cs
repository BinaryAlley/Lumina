#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Mapster;
using MapsterMapper;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.Drives.Queries.GetDrives;

/// <summary>
/// Handler for the query to get all drives.
/// </summary>
public class GetDrivesQueryHandler : IRequestHandler<GetDrivesQuery, ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>
{
    private readonly IDriveService _driveService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesQueryHandler"/> class.
    /// </summary>
    /// <param name="driveService">Injected service for handling file system drives.</param>
    /// <param name="mapper">Injected service for mapping objects.</param>
    public GetDrivesQueryHandler(IDriveService driveService, IMapper mapper)
    {
        _driveService = driveService;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets the list of file system drives.
    /// </summary>
    /// <param name="request">The query containing the requested path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="FileSystemTreeNodeResponse"/>, or an error message.
    /// </returns>
    public ValueTask<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>> Handle(GetDrivesQuery request, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<FileSystemItem>> result = _driveService.GetDrives();
        return ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(_mapper.Map<IEnumerable<FileSystemTreeNodeResponse>>(values)), errors => errors));
    }
}
