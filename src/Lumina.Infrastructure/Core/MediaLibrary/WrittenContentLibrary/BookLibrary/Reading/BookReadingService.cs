#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Reading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Service for reading books using the book reader plugins configured for their media library. The contents of a book are
/// extracted into a temporary directory on first access and served from there, and the extraction is wiped at startup.
/// </summary>
/// <remarks>
/// The reading flow, for every request: resolve the reader plugin supporting the format of the book, check that the reader
/// is enabled for the book's library, reuse the already extracted book when its file has not changed on disk, and only then
/// serve the requested part (manifest, section, or resource). The section content is always sanitized by the host, and the
/// section and resource file paths coming from the plugins are contained-checked before they are read. The text and the image
/// extraction of the same book use separate working directories, and each extraction is serialized per rendering preference,
/// so that users reading the same book with different preferences never overwrite each other's extracted files.
/// </remarks>
internal sealed class BookReadingService : IBookReadingService
{
    private readonly ConcurrentDictionary<ReadingCacheKey, BookExtractionEntry> _extractions = [];
    private readonly ConcurrentDictionary<ReadingCacheKey, SemaphoreSlim> _bookExtractionLocks = [];
    private readonly IServiceProvider _serviceProvider;
    private readonly IPluginManager _pluginManager;
    private readonly IBookReaderEnablementCache _enablementCache;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BookReadingService> _logger;

    // The plugin set is fixed for the lifetime of the host, so the mapping from a format to the plugin that reads it is built once on
    // its first use, instead of being recomputed - and each plugin's keyed services resolved - on every section or resource request.
    private IReadOnlyDictionary<ReaderKey, Guid>? _pluginIdsByReaderKey;
    private readonly object _readerMapSync = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookReadingService"/> class.
    /// </summary>
    /// <param name="serviceProvider">Injected provider used to resolve the book readers registered by the plugins.</param>
    /// <param name="pluginManager">Injected manager of the plugins loaded by the host application.</param>
    /// <param name="enablementCache">Injected cache of whether the book reader of a plugin is enabled for a media library.</param>
    /// <param name="serviceScopeFactory">Injected factory for creating scopes in which the book reader configurations are requested.</param>
    /// <param name="logger">Injected logger used to report the failures to open the books.</param>
    public BookReadingService(IServiceProvider serviceProvider, IPluginManager pluginManager, IBookReaderEnablementCache enablementCache, IServiceScopeFactory serviceScopeFactory, ILogger<BookReadingService> logger)
    {
        _serviceProvider = serviceProvider;
        _pluginManager = pluginManager;
        _enablementCache = enablementCache;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Gets the reading manifest of the book stored at <paramref name="path"/>.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the reading manifest of the book, or an error.</returns>
    public async Task<Result<ReadingManifestResponse>> GetManifestAsync(Guid bookId, Guid libraryId, string path, LibraryType libraryType, bool shouldRenderPdfAsImages, CancellationToken cancellationToken)
    {
        Result<BookExtractionEntry> extractionResult = await GetExtractionAsync(bookId, libraryId, path, libraryType, shouldRenderPdfAsImages, cancellationToken).ConfigureAwait(false);
        return extractionResult.IsSuccess ? extractionResult.Value.Document.ToResponse() : extractionResult.Errors;
    }

    /// <summary>
    /// Gets the content of the reading section identified by <paramref name="locationRef"/> of the book stored at <paramref name="path"/>.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <param name="locationRef">The opaque location reference of the reading section.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <param name="shouldPreserveStyles">Whether the styles of the section content are preserved when it is sanitized, or stripped.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the content of the reading section, or an error.</returns>
    public async Task<Result<ReadingSectionDto>> GetSectionAsync(Guid bookId, Guid libraryId, string path, LibraryType libraryType, string locationRef, bool shouldRenderPdfAsImages, bool shouldPreserveStyles, CancellationToken cancellationToken)
    {
        Result<BookExtractionEntry> extractionResult = await GetExtractionAsync(bookId, libraryId, path, libraryType, shouldRenderPdfAsImages, cancellationToken).ConfigureAwait(false);
        if (extractionResult.IsFailure)
            return extractionResult.Errors;
        BookExtractionEntry extraction = extractionResult.Value;

        if (!extraction.SectionsByLocationRef.TryGetValue(locationRef, out ReadingSpineItemDto? spineItem))
            return Errors.Reading.SectionNotFound;

        string workingDirectory = GetWorkingDirectory(bookId, shouldRenderPdfAsImages);
        // The section file path comes from the reader plugin, so it is resolved against the working directory and the resolved path is verified to stay inside it,
        // so that a malicious plugin cannot point the reader at arbitrary files.
        Result<string> sectionFilePathResult = ResolveContainedPath(workingDirectory, spineItem.RelativeSectionFilePath);
        if (sectionFilePathResult.IsFailure || !File.Exists(sectionFilePathResult.Value))
            return Errors.Reading.SectionNotFound;

        string content = await File.ReadAllTextAsync(sectionFilePathResult.Value, cancellationToken).ConfigureAwait(false);
        // The section content is always sanitized by the host, regardless of what the reader plugin produced, so that no plugin can ever serve active content to the client;
        // whether the inert style attributes survive is the per-user choice that decides if a book keeps its original look.
        return new ReadingSectionDto(spineItem.LocationRef, spineItem.Title, ReadingContentSanitizer.Sanitize(content, shouldPreserveStyles));
    }

    /// <summary>
    /// Gets the resource identified by <paramref name="resourceKey"/> of the book stored at <paramref name="path"/>.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <param name="resourceKey">The opaque resource key of the resource.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the resource, or an error.</returns>
    public async Task<Result<ReadingResourceDataDto>> GetResourceAsync(Guid bookId, Guid libraryId, string path, LibraryType libraryType, string resourceKey, bool shouldRenderPdfAsImages, CancellationToken cancellationToken)
    {
        // The reader is resolved and its enablement verified before anything is served, so that disabling a reader immediately cuts access
        // even to the resources that are already extracted and cached on disk, and to the pages a PDF rendered for an earlier request.
        Result<BookExtractionEntry> extractionResult = await GetExtractionAsync(bookId, libraryId, path, libraryType, shouldRenderPdfAsImages, cancellationToken).ConfigureAwait(false);
        if (extractionResult.IsFailure)
            return extractionResult.Errors;
        BookExtractionEntry extraction = extractionResult.Value;

        if (!extraction.Document.Resources.TryGetValue(resourceKey, out ReadingResourceInfoDto? resource))
            return Errors.Reading.ResourceNotFound;

        string workingDirectory = GetWorkingDirectory(bookId, shouldRenderPdfAsImages);
        // The resource file path comes from the reader plugin, so it is contained-checked the same way the section files are.
        Result<string> resourceFilePathResult = ResolveContainedPath(workingDirectory, resource.RelativeFilePath);
        if (resourceFilePathResult.IsFailure)
            return Errors.Reading.ResourceNotFound;
        string resourceFilePath = resourceFilePathResult.Value;

        // A resource that was already produced (for example an EPUB resource extracted when the book was opened, or a PDF page
        // rendered by an earlier request) is served from disk; otherwise the reader produces it on demand, and the produced file is
        // cached to disk, so that a resource is only ever produced once, and a large book is not rendered up front.
        if (!File.Exists(resourceFilePath))
        {
            // A resource is produced by at most one request at a time: two requests that miss the same resource (for example two
            // readers turning to the same not-yet-rendered page of a PDF) would otherwise both render it, and one of them could read
            // the resource file while the other was still writing it.
            SemaphoreSlim productionLock = extraction.ResourceProductionLocks.GetOrAdd(resourceFilePath, _ => new SemaphoreSlim(initialCount: 1, maxCount: 1));
            await productionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Another request may have produced the resource while this one was waiting for the lock.
                if (File.Exists(resourceFilePath))
                {
                    byte[] existingData = await File.ReadAllBytesAsync(resourceFilePath, cancellationToken).ConfigureAwait(false);
                    return new ReadingResourceDataDto(existingData, resource.MimeType);
                }

                Result<(Guid pluginId, IBookReader reader)> resolveResult = ResolveReader(path, libraryType);
                if (resolveResult.IsFailure)
                    return resolveResult.Errors;

                // A failing reader must not escape as an unhandled exception, so the production is wrapped the same way the extraction is;
                // a corrupt book or renderer failure becomes a reading error the client can show, instead of a raw server error.
                byte[] producedData;
                try
                {
                    producedData = await resolveResult.Value.reader.GetResourceAsync(path, workingDirectory, resourceKey, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to produce the resource '{ResourceKey}' of the book '{BookId}'.", resourceKey, bookId);
                    return Error.Failure(code: "Reading.ResourceProductionFailed", description: "The resource of the book could not be produced.");
                }

                // The produced bytes are written to a temporary file that is then moved over the resource file, so that a request that
                // observes the resource file on disk only ever reads a fully written resource, never one that is still being written.
                Directory.CreateDirectory(Path.GetDirectoryName(resourceFilePath)!);
                string temporaryFilePath = $"{resourceFilePath}.{Guid.NewGuid():N}.tmp";
                try
                {
                    await File.WriteAllBytesAsync(temporaryFilePath, producedData, cancellationToken).ConfigureAwait(false);
                    File.Move(temporaryFilePath, resourceFilePath, overwrite: true);
                }
                catch
                {
                    if (File.Exists(temporaryFilePath))
                        File.Delete(temporaryFilePath);
                    throw;
                }
                return new ReadingResourceDataDto(producedData, resource.MimeType);
            }
            finally
            {
                productionLock.Release();
            }
        }

        byte[] data = await File.ReadAllBytesAsync(resourceFilePath, cancellationToken).ConfigureAwait(false);
        return new ReadingResourceDataDto(data, resource.MimeType);
    }

    /// <summary>
    /// Checks whether the book stored at <paramref name="path"/> can be opened for reading, resolving the book reader configured for
    /// its media library and verifying that the reader is enabled, without extracting the book.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the reading availability of the book, or an error.</returns>
    public async Task<Result<ReadingAvailabilityResponse>> GetAvailabilityAsync(Guid bookId, Guid libraryId, string path, LibraryType libraryType, CancellationToken cancellationToken)
    {
        // The same two checks that gate the actual reading are performed here, so that the availability the user is told about is
        // exactly the availability the reader would experience, but the book is not extracted just to answer the question.
        Result<(Guid pluginId, IBookReader reader)> resolveResult = ResolveReader(path, libraryType);
        if (resolveResult.IsFailure)
            return new ReadingAvailabilityResponse(bookId, libraryId, IsAvailable: false, ErrorCode: resolveResult.FirstError.Description);

        Result<bool> isEnabledResult = await IsReaderEnabledAsync(libraryId, resolveResult.Value.pluginId, cancellationToken).ConfigureAwait(false);
        if (isEnabledResult.IsFailure)
            return new ReadingAvailabilityResponse(bookId, libraryId, IsAvailable: false, ErrorCode: isEnabledResult.FirstError.Description);

        return new ReadingAvailabilityResponse(bookId, libraryId, IsAvailable: true, ErrorCode: null);
    }

    /// <summary>
    /// Gets the extracted reading document of the book stored at <paramref name="path"/>, resolving the book reader configured
    /// for its media library, extracting the book into the temporary directory when it is not already extracted, and enforcing
    /// the enablement of the reader before anything is served.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the extracted document of the book with its section lookup, or an error.</returns>
    private async Task<Result<BookExtractionEntry>> GetExtractionAsync(Guid bookId, Guid libraryId, string path, LibraryType libraryType, bool shouldRenderPdfAsImages, CancellationToken cancellationToken)
    {
        // The plugin supporting the book format is resolved first, so that a book whose format has no plugin at all gets a distinct error from a book whose plugin
        // is simply disabled for the library.
        Result<(Guid pluginId, IBookReader reader)> resolveResult = ResolveReader(path, libraryType);
        if (resolveResult.IsFailure)
            return resolveResult.Errors;

        // The per-library enablement is enforced before anything is served, and never derived from the extraction cache, so disabling
        // a reader immediately cuts access for that library, even to already extracted content; the check itself is cached, since it is
        // read on every request and only changes through an explicit toggle.
        Result<bool> isEnabledResult = await IsReaderEnabledAsync(libraryId, resolveResult.Value.pluginId, cancellationToken).ConfigureAwait(false);
        if (isEnabledResult.IsFailure)
            return isEnabledResult.Errors;

        // The extraction is keyed by both the book and the rendering preference, so that the same book can be extracted both as
        // text and as page images for different users without them evicting each other.
        ReadingCacheKey cacheKey = new(bookId, shouldRenderPdfAsImages);

        // Reuse the already extracted document while the book file on disk still matches the extraction, so that reading a book does not re-parse it on every section
        // or resource request; a book modified in place is re-extracted.
        if (_extractions.TryGetValue(cacheKey, out BookExtractionEntry? extraction) && !HasFileChanged(extraction, path))
            return extraction;

        // A book is extracted by at most one request at a time, and each rendering preference of a book has its own working directory, so
        // concurrent extractions are serialized per book and rendering preference; a request that arrives while the book is being extracted
        // for its preference waits for that extraction to finish and reuses its result, instead of starting a second extraction over the
        // same directory. The extraction itself runs with a host lifetime token rather than the token of the first request, so that a single
        // disconnected client cannot abort the extraction of a book that other users are waiting for.
        SemaphoreSlim extractionLock = _bookExtractionLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(initialCount: 1, maxCount: 1));
        await extractionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Another request may have extracted the book while this one was waiting for the lock.
            if (_extractions.TryGetValue(cacheKey, out BookExtractionEntry? existingExtraction) && !HasFileChanged(existingExtraction, path))
                return existingExtraction;

            Result<ReadingDocumentDto> extractResult = await ExtractAsync(resolveResult.Value.reader, bookId, path, shouldRenderPdfAsImages, CancellationToken.None).ConfigureAwait(false);
            if (extractResult.IsFailure)
                return extractResult.Errors;
            ReadingDocumentDto document = extractResult.Value;
            BookExtractionEntry newExtraction = new(document, BuildSectionLookup(document), GetFileFingerprint(path));
            _extractions[cacheKey] = newExtraction;
            return newExtraction;
        }
        finally
        {
            extractionLock.Release();
        }
    }

    /// <summary>
    /// Extracts the book stored at <paramref name="path"/> into its temporary directory using the provided <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The book reader used to extract the book.</param>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the reading document of the book, or an error.</returns>
    private async Task<Result<ReadingDocumentDto>> ExtractAsync(IBookReader reader, Guid bookId, string path, bool shouldRenderPdfAsImages, CancellationToken cancellationToken)
    {
        string workingDirectory = GetWorkingDirectory(bookId, shouldRenderPdfAsImages);
        try
        {
            // A re-extraction replaces the previous contents of the directory, so that a book modified in place is re-extracted from scratch.
            if (Directory.Exists(workingDirectory))
                Directory.Delete(workingDirectory, recursive: true);
            Directory.CreateDirectory(workingDirectory);
            return await reader.OpenAsync(path, workingDirectory, shouldRenderPdfAsImages, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return Errors.Reading.BookFileNotFound;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to open the book '{BookId}' for reading.", bookId);
            return Error.Failure(code: "Reading.ExtractionFailed", description: "The book could not be opened for reading.");
        }
    }

    /// <summary>
    /// Resolves the book reader supporting the extension and the library type of the book stored at <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="libraryType">The type of the media library the book belongs to.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the Id of the plugin and the resolved book reader, or an error.</returns>
    private Result<(Guid pluginId, IBookReader reader)> ResolveReader(string path, LibraryType libraryType)
    {
        // The extension is matched case-insensitively against the extensions the readers declare, like the per-request scan it replaces.
        string extension = Path.GetExtension(path).ToLowerInvariant();
        ReaderKey readerKey = new(extension, libraryType);
        IReadOnlyDictionary<ReaderKey, Guid> pluginIdsByReaderKey = GetPluginIdsByReaderKey();
        if (!pluginIdsByReaderKey.TryGetValue(readerKey, out Guid pluginId))
            return Errors.Reading.NoReaderAvailable;
        IBookReader? reader = _serviceProvider.GetKeyedServices<IBookReader>(pluginId)
            .FirstOrDefault(candidate => candidate.SupportedLibraryTypes.Contains(libraryType)
                && candidate.SupportedExtensions.Any(supportedExtension => string.Equals(supportedExtension, extension, StringComparison.OrdinalIgnoreCase)));
        return reader is not null ? (pluginId, reader) : Errors.Reading.NoReaderAvailable;
    }

    /// <summary>
    /// Gets the mapping from a format to the Id of the plugin that reads it, built once from the plugins loaded by the host.
    /// </summary>
    /// <returns>The Id of the plugin reading each format, keyed by the format.</returns>
    private IReadOnlyDictionary<ReaderKey, Guid> GetPluginIdsByReaderKey()
    {
        IReadOnlyDictionary<ReaderKey, Guid>? pluginIdsByReaderKey = _pluginIdsByReaderKey;
        if (pluginIdsByReaderKey is not null)
            return pluginIdsByReaderKey;
        lock (_readerMapSync)
        {
            pluginIdsByReaderKey = _pluginIdsByReaderKey;
            if (pluginIdsByReaderKey is not null)
                return pluginIdsByReaderKey;
            // The readers are iterated in the same stable order the per-request scan used - plugins alphabetically by name, then their
            // readers - and the first reader that declares a format is the one that reads it, so a format is handled by exactly one reader.
            Dictionary<ReaderKey, Guid> discoveredPluginIdsByReaderKey = [];
            foreach (IPlugin plugin in _pluginManager.GetPlugins().OrderBy(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase))
            {
                foreach (IBookReader candidateReader in _serviceProvider.GetKeyedServices<IBookReader>(plugin.Id))
                {
                    foreach (LibraryType supportedLibraryType in candidateReader.SupportedLibraryTypes)
                    {
                        foreach (string supportedExtension in candidateReader.SupportedExtensions)
                        {
                            string normalizedExtension = supportedExtension.Trim().ToLowerInvariant();
                            if (normalizedExtension.Length == 0)
                                continue;
                            discoveredPluginIdsByReaderKey.TryAdd(new ReaderKey(normalizedExtension, supportedLibraryType), plugin.Id);
                        }
                    }
                }
            }
            _pluginIdsByReaderKey = discoveredPluginIdsByReaderKey;
            return discoveredPluginIdsByReaderKey;
        }
    }

    /// <summary>
    /// Determines whether the book reader of the plugin identified by <paramref name="pluginId"/> is enabled for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configuration is read.</param>
    /// <param name="pluginId">The Id of the plugin providing the book reader.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either whether the book reader is enabled, or an error.</returns>
    private async Task<Result<bool>> IsReaderEnabledAsync(Guid libraryId, Guid pluginId, CancellationToken cancellationToken)
    {
        // The enablement is cached, because it is consulted on every request and only changes when the user toggles a reader; the toggle
        // invalidates the cache, so a disabled reader is never served content for longer than the toggle operation takes.
        bool? cachedIsEnabled = _enablementCache.Get(libraryId, pluginId);
        if (cachedIsEnabled is not null)
            return cachedIsEnabled.Value ? true : Errors.Reading.ReaderDisabled;

        // The service is a singleton so that the extraction cache is shared across requests, and the unit of work is scoped, so a fresh scope is created for this single
        // configuration read instead of capturing a scoped dependency into a singleton.
        await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
        IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        Result<LibraryBookReaderConfigurationEntity?> getConfigurationResult = await unitOfWork.LibraryBookReaderConfigurationRepository.GetByLibraryAndPluginIdAsync(libraryId, pluginId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationResult.IsFailure)
            return getConfigurationResult.Errors;
        // A missing configuration row (for example when the library was created before the reader was installed) is treated as disabled, so that no reader is ever used without an explicit enablement.
        bool isEnabled = getConfigurationResult.Value is not null && getConfigurationResult.Value.IsEnabled;
        _enablementCache.Set(libraryId, pluginId, isEnabled);
        return isEnabled ? true : Errors.Reading.ReaderDisabled;
    }

    /// <summary>
    /// Determines whether the file stored at <paramref name="path"/> changed since it was extracted.
    /// </summary>
    /// <param name="extraction">The stored extraction of the book.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <returns><see langword="true"/> when the file changed since it was extracted, <see langword="false"/> otherwise.</returns>
    private static bool HasFileChanged(BookExtractionEntry extraction, string path)
    {
        // Comparing the size and the last write time against the values captured at extraction time is a cheap staleness check that does not re-read the book file,
        // so that an in-place modification of the book is detected without the cost of hashing the whole file on every request.
        BookFileFingerprint currentFingerprint = GetFileFingerprint(path);
        return currentFingerprint.FileSize != extraction.Fingerprint.FileSize || currentFingerprint.LastWriteTimeUtc != extraction.Fingerprint.LastWriteTimeUtc;
    }

    /// <summary>
    /// Gets the size and the last write time of the file stored at <paramref name="path"/>, in a single file system probe.
    /// </summary>
    /// <param name="path">The file system path of the file.</param>
    /// <returns>The fingerprint of the file, or the fingerprint of a missing file when it cannot be probed.</returns>
    private static BookFileFingerprint GetFileFingerprint(string path)
    {
        try
        {
            // A missing file is reported with a size of -1 and a minimum last write time, which never match a stored extraction
            // fingerprint, so that a deleted book is re-extracted (and fails) instead of being served from the cache.
            FileInfo fileInfo = new(path);
            return fileInfo.Exists ? new BookFileFingerprint(fileInfo.Length, fileInfo.LastWriteTimeUtc) : BookFileFingerprint.Missing;
        }
        catch (IOException)
        {
            return BookFileFingerprint.Missing;
        }
        catch (UnauthorizedAccessException)
        {
            return BookFileFingerprint.Missing;
        }
    }

    /// <summary>
    /// Builds the lookup of the reading sections of a book, keyed by their location reference, so that a section is found without
    /// scanning the whole spine on every request.
    /// </summary>
    /// <param name="document">The reading document of the book.</param>
    /// <returns>The reading sections of the book, keyed by their location reference.</returns>
    private static IReadOnlyDictionary<string, ReadingSpineItemDto> BuildSectionLookup(ReadingDocumentDto document)
    {
        Dictionary<string, ReadingSpineItemDto> sectionsByLocationRef = [];
        foreach (ReadingSpineItemDto spineItem in document.Spine)
            sectionsByLocationRef.TryAdd(spineItem.LocationRef, spineItem);
        return sectionsByLocationRef;
    }

    /// <summary>
    /// Gets the temporary directory into which the book identified by <paramref name="bookId"/> is extracted for the rendering
    /// preference identified by <paramref name="shouldRenderPdfAsImages"/>.
    /// </summary>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <returns>The absolute path of the temporary directory of the book for the rendering preference.</returns>
    private static string GetWorkingDirectory(Guid bookId, bool shouldRenderPdfAsImages)
    {
        // The directory of a book combines its Id with the rendering preference used to extract it, so that the text and the image
        // extraction of the same book live in separate directories and a re-extraction of one preference never deletes the files of
        // the other; the whole cache is wiped at startup, so no further bookkeeping is needed to keep it from growing across restarts.
        return Path.Combine(ReadingCachePaths.GetRootDirectory(), bookId.ToString("N"), shouldRenderPdfAsImages ? "images" : "text");
    }

    /// <summary>
    /// Resolves a package-relative path against a package root, rejecting paths that escape the root.
    /// </summary>
    /// <param name="root">The absolute path of the package root.</param>
    /// <param name="relativePath">The package-relative path to resolve.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the absolute path, or an error.</returns>
    private static Result<string> ResolveContainedPath(string root, string relativePath)
    {
        string rootPath = Path.GetFullPath(root);
        string candidate = Path.GetFullPath(Path.Combine(rootPath, relativePath));
        string rootWithSeparator = rootPath.EndsWith(Path.DirectorySeparatorChar) ? rootPath : rootPath + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        // A relative path that walks up past the root (for example with "..") is rejected, so that the reader can never serve a file outside the temporary extraction directory of the book.
        if (!candidate.StartsWith(rootWithSeparator, comparison))
            return Errors.Reading.ResourceNotFound;
        return candidate;
    }

    /// <summary>
    /// The extracted document of a book, along with the lookup of its sections and the file fingerprint captured at extraction time.
    /// </summary>
    /// <param name="Document">The reading document of the book.</param>
    /// <param name="SectionsByLocationRef">The reading sections of the book, keyed by their location reference.</param>
    /// <param name="Fingerprint">The size and last write time of the book file at extraction time.</param>
    private sealed record BookExtractionEntry(ReadingDocumentDto Document, IReadOnlyDictionary<string, ReadingSpineItemDto> SectionsByLocationRef, BookFileFingerprint Fingerprint)
    {
        /// <summary>
        /// The on-demand productions of the resources of the book, serialized per resource file, so that two requests that miss the
        /// same resource (for example two readers turning to the same not-yet-rendered page of a PDF) do not both produce it, and a
        /// request never reads a resource while another request is writing it.
        /// </summary>
        public ConcurrentDictionary<string, SemaphoreSlim> ResourceProductionLocks { get; } = [];
    }

    /// <summary>
    /// The fingerprint of a book file, used to detect whether the file changed since it was extracted.
    /// </summary>
    /// <param name="FileSize">The size of the file, or -1 when the file does not exist.</param>
    /// <param name="LastWriteTimeUtc">The last write time of the file, or <see cref="DateTime.MinValue"/> when the file does not exist.</param>
    private readonly record struct BookFileFingerprint(long FileSize, DateTime LastWriteTimeUtc)
    {
        /// <summary>
        /// The fingerprint of a file that does not exist or cannot be probed.
        /// </summary>
        public static BookFileFingerprint Missing { get; } = new(-1, DateTime.MinValue);
    }

    /// <summary>
    /// The key of the extraction cache of a book, combining the Id of the book with the rendering preference used to extract it,
    /// so that the same book can be extracted both as text and as page images for different users without them evicting each other.
    /// </summary>
    /// <param name="BookId">The Id of the book.</param>
    /// <param name="ShouldRenderPdfAsImages">Whether the book, when it is a PDF, was rendered as page images instead of extracting its text layer.</param>
    private sealed record ReadingCacheKey(Guid BookId, bool ShouldRenderPdfAsImages);

    /// <summary>
    /// The format of a book, combining its file extension with the type of its media library.
    /// </summary>
    /// <param name="Extension">The lower-case file extension of the format, with its leading dot.</param>
    /// <param name="LibraryType">The type of the media library the format is read for.</param>
    private sealed record ReaderKey(string Extension, LibraryType LibraryType);
}
