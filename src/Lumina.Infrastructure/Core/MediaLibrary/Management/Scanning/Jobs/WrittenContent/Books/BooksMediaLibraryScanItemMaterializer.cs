#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Materializer for the media library items of the books media library type.
/// </summary>
internal sealed class BooksMediaLibraryScanItemMaterializer : IMediaLibraryScanItemMaterializer
{
    private readonly ILibraryPathTemplateService _pathTemplateService;
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksMediaLibraryScanItemMaterializer"/> class.
    /// </summary>
    /// <param name="pathTemplateService">Injected service for deriving the metadata of the books from their paths.</param>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    public BooksMediaLibraryScanItemMaterializer(ILibraryPathTemplateService pathTemplateService, IPathService pathService)
    {
        _pathTemplateService = pathTemplateService;
        _pathService = pathService;
    }

    /// <summary>
    /// The media library type that this materializer supports.
    /// </summary>
    public LibraryType SupportedLibraryType => LibraryType.Book;

    /// <summary>
    /// Resets the enrichment state of the books stored at the provided <paramref name="paths"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose books are reset.</param>
    /// <param name="paths">The file system paths of the books whose enrichment state is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetEnrichmentStateForChangedPathsAsync(IUnitOfWork unitOfWork, Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken)
    {
        return await unitOfWork.BookRepository.ResetEnrichmentStateForPathsAsync(libraryId, paths, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Materializes the books of the media library from the paths of the media library scan snapshot.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose books are materialized.</param>
    /// <param name="scanId">The Id of the media library scan whose results are materialized.</param>
    /// <param name="paths">The file system paths of the media library scan snapshot.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> MaterializeItemsAsync(IUnitOfWork unitOfWork, Guid libraryId, Guid scanId, IReadOnlyList<string> paths, CancellationToken cancellationToken)
    {
        // The library holds the path template that describes the structure of the library on disk, used to derive the metadata of each book from its path.
        Result<LibraryEntity?> getLibraryResult = await unitOfWork.LibraryRepository.GetByIdAsync(libraryId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;
        Result<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
        if (domainLibraryResult.IsFailure)
            return domainLibraryResult.Errors;
        Library library = domainLibraryResult.Value;
        Result<LibraryPathTemplate> pathTemplateResult = _pathTemplateService.ResolveTemplate(library.LibraryType, library.PathTemplate.Parts);
        if (pathTemplateResult.IsFailure)
            return pathTemplateResult.Errors;
        LibraryPathTemplate pathTemplate = pathTemplateResult.Value;

        // Books that are already stored are never inserted again, so a re-scan of an unchanged file does not duplicate its book. The existing paths are
        // loaded in a single query, instead of one existence check per path, so that the number of round trips does not scale with the size of the library.
        List<string> stagedPaths = [.. paths];
        Result<IReadOnlyCollection<string>> getExistingPathsResult = await unitOfWork.BookRepository.GetExistingPathsAsync(libraryId, stagedPaths, cancellationToken).ConfigureAwait(false);
        if (getExistingPathsResult.IsFailure)
            return getExistingPathsResult.Errors;
        HashSet<string> existingPaths = [.. getExistingPathsResult.Value];

        foreach (string path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (existingPaths.Contains(path))
                continue;

            Result<Created> insertBookResult = await unitOfWork.BookRepository.InsertAsync(CreateShellBookEntity(library, path, pathTemplate), cancellationToken).ConfigureAwait(false);
            if (insertBookResult.IsFailure)
                return insertBookResult.Errors;
        }
        return Result.Success;
    }

    /// <summary>
    /// Resets the metadata enrichment status of all the books of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose books are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(IUnitOfWork unitOfWork, Guid libraryId, CancellationToken cancellationToken)
    {
        return await unitOfWork.BookRepository.ResetMetadataStatusForLibraryAsync(libraryId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resets the artwork enrichment status of all the books of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose books are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetArtworkStatusForLibraryAsync(IUnitOfWork unitOfWork, Guid libraryId, CancellationToken cancellationToken)
    {
        return await unitOfWork.BookRepository.ResetArtworkStatusForLibraryAsync(libraryId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a shell book entity for the file stored at <paramref name="path"/> in the media library identified by <paramref name="library"/>.
    /// </summary>
    /// <param name="library">The media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="pathTemplate">The template describing the structure of the media library on disk.</param>
    /// <returns>The created shell book entity.</returns>
    private BookEntity CreateShellBookEntity(Library library, string path, LibraryPathTemplate pathTemplate)
    {
        string title = GetTitleFromPath(path);
        float? volumeNumber = null;

        string relativePath = GetRelativePath(path, library.ContentLocations, _pathService);
        Result<Optional<ParsedLibraryPath>> parsedPathResult = _pathTemplateService.Parse(library.LibraryType, pathTemplate, relativePath, _pathService.PathSeparator);
        if (parsedPathResult.IsSuccess && parsedPathResult.Value.HasValue)
        {
            ParsedLibraryPath parsedPath = parsedPathResult.Value.Value;
            Optional<string> parsedTitle = parsedPath.GetString(LibraryPathPartKind.Title);
            if (parsedTitle.HasValue)
                title = parsedTitle.Value;
            Optional<int> parsedSeriesNumber = parsedPath.GetInt(LibraryPathPartKind.SeriesNumber);
            if (parsedSeriesNumber.HasValue)
                volumeNumber = parsedSeriesNumber.Value;
        }

        return new BookEntity
        {
            Id = Guid.NewGuid(),
            LibraryId = library.Id.Value,
            Path = path,
            Title = title,
            VolumeNumber = volumeNumber,
            MetadataStatus = MetadataStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Derives a book title from the file name of the provided <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The file system path of the book.</param>
    /// <returns>The derived title.</returns>
    private string GetTitleFromPath(string path)
    {
        string fileName = _pathService.GetFileNameWithoutExtension(path);
        string title = Regex.Replace(fileName, @"[_\-\.]+", " ").Trim();
        return title.Length > 0 ? title : fileName;
    }

    /// <summary>
    /// Gets the path of the book stored at <paramref name="path"/>, relative to the content location of the media library it belongs to.
    /// </summary>
    /// <param name="path">The absolute file system path of the book.</param>
    /// <param name="contentLocations">The content locations of the media library the book belongs to.</param>
    /// <param name="pathService">The service used to determine whether the file is inside a content location of the media library.</param>
    /// <returns>The path of the book, relative to its content location, or the file name when no content location contains it.</returns>
    private static string GetRelativePath(string path, IReadOnlyCollection<FileSystemPathId> contentLocations, IPathService pathService)
    {
        foreach (FileSystemPathId contentLocation in contentLocations)
        {
            if (pathService.IsPathWithin(path, contentLocation.Path))
            {
                string relativePath = path[contentLocation.Path.Length..].TrimStart('\\', '/');
                if (relativePath.Length > 0)
                    return relativePath;
            }
        }
        return pathService.GetFileName(path);
    }
}
