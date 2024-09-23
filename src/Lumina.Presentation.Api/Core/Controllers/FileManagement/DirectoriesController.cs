﻿#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectories;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectoryTree;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetTreeDirectories;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.Common.ModelBinders;
using Lumina.Presentation.Api.Core.Controllers.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
#endregion

namespace Lumina.Presentation.Api.Core.Controllers.FileManagement;

/// <summary>
/// Controller for managing file system directories.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DirectoriesController : ApiController
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly ISender _mediator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoriesController"/> class.
    /// </summary>
    /// <param name="mediator">Injected service for mediating commands and queries.</param>
    public DirectoriesController(ISender mediator)
    {
        _mediator = mediator;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the tree of expanded directories leading up to <paramref name="path"/>, with the additional list of drives, and children of the last child directory.
    /// </summary>
    /// <param name="path">The path for which to get the directory tree.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-directory-tree")]
    public async IAsyncEnumerable<FileSystemTreeNodeResponse> GetDirectoryTree([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, [FromQuery] bool includeFiles, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _mediator.Send(new GetDirectoryTreeQuery(path, includeFiles), cancellationToken).ConfigureAwait(false);
        if (!result.IsError && result.Value is not null)
        {
            foreach (var treeNode in result.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;
                yield return treeNode;
            }
        }
    }

    /// <summary>
    /// Gets the directories of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the directories.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-tree-directories")]
    public async IAsyncEnumerable<FileSystemTreeNodeResponse> GetTreeDirectories([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _mediator.Send(new GetTreeDirectoriesQuery(path), cancellationToken).ConfigureAwait(false);
        if (!result.IsError && result.Value is not null)
        {
            foreach (var directory in result.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;
                yield return directory;
            }
        }
    }

    /// <summary>
    /// Gets the directories of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to get the directories.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("get-directories")]
    public async IAsyncEnumerable<DirectoryResponse> GetDirectories([FromQuery, ModelBinder(typeof(UrlStringBinder))] string path, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<DirectoryResponse>> result = await _mediator.Send(new GetDirectoriesQuery(path), cancellationToken).ConfigureAwait(false);
        if (!result.IsError && result.Value is not null)
        {
            foreach (var directory in result.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;
                yield return directory;
            }
        }
    }
    #endregion
}