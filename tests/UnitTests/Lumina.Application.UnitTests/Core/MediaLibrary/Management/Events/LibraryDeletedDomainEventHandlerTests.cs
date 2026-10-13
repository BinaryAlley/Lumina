#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryDeletedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryDeletedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IMusicLibraryScanItemMetadataRepository _mockMusicLibraryScanItemMetadataRepository;
    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly IDirectoryProviderService _mockDirectoryProviderService;
    private readonly IPathService _mockPathService;
    private readonly LibraryDeletedDomainEventHandler _sut;
    private readonly LibraryFixture _libraryFixture = new();
    private readonly LibraryDeletedDomainEventFixture _libraryDeletedDomainEventFixture = new();
    private readonly MediaSettingsDtoFixture _mediaSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryDeletedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryDeletedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockMusicLibraryScanItemMetadataRepository = Substitute.For<IMusicLibraryScanItemMetadataRepository>();
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.MusicLibraryScanItemMetadataRepository.Returns(_mockMusicLibraryScanItemMetadataRepository);
        _mockMusicArtworkRepository.DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.Deleted);
        _mockArtistRepository.DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.Deleted);
        _mockBookRepository.DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.Deleted);
        _mockMusicLibraryScanItemMetadataRepository.DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.Deleted);

        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockDirectoryProviderService = Substitute.For<IDirectoryProviderService>();
        _mockEnvironmentContext.DirectoryProviderService.Returns(_mockDirectoryProviderService);
        _mockPathService = Substitute.For<IPathService>();

        MediaSettingsDto mediaSettings = _mediaSettingsDtoFixture.Create(
            rootDirectory: "Media",
            librariesDirectory: "Libraries",
            booksDirectory: "Books",
            musicDirectory: "Music");
        IOptions<MediaSettingsDto> mediaSettingsOptions = Substitute.For<IOptions<MediaSettingsDto>>();
        mediaSettingsOptions.Value.Returns(mediaSettings);

        // default stubs: all path combinations succeed, segments are sanitized to their input, the library directories exist and they are deleted successfully
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(string.Concat(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1))));
        _mockPathService.SanitizeSegment(Arg.Any<string>())
            .Returns(callInfo => PathSegment.Create(callInfo.ArgAt<string>(0), isDirectory: false, isDrive: false));
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
        _mockDirectoryProviderService.DeleteDirectory(Arg.Any<FileSystemPathId>()).Returns(Result.Deleted);

        _sut = new LibraryDeletedDomainEventHandler(_mockUnitOfWork, _mockEnvironmentContext, _mockPathService, mediaSettingsOptions);
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsDeleted_ShouldDeleteMediaItemsAndLibraryDirectory()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockMusicArtworkRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockMusicLibraryScanItemMetadataRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        _mockDirectoryProviderService.Received(3).DeleteDirectory(
            Arg.Is<FileSystemPathId>(path => path.Path.EndsWith(libraryId.ToString(), StringComparison.Ordinal)));
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteDirectoryFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to delete library directory");
        _mockDirectoryProviderService.DeleteDirectory(Arg.Any<FileSystemPathId>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
    }

    [Fact]
    public async Task HandleAsync_WhenGetLibraryPathFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to combine path");
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        _mockDirectoryProviderService.DidNotReceive().DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteMusicArtworkFails_ShouldThrowEventualConsistencyExceptionWithoutDeletingAnythingElse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to delete music artwork");
        _mockMusicArtworkRepository.DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockArtistRepository.DidNotReceive().DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicLibraryScanItemMetadataRepository.DidNotReceive().DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockDirectoryProviderService.DidNotReceive().DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteArtistsFails_ShouldThrowEventualConsistencyExceptionWithoutDeletingAnythingElse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to delete artists");
        _mockArtistRepository.DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        // The artwork is deleted before its owners, so that its owners are still present when the artwork is looked up.
        await _mockMusicArtworkRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicLibraryScanItemMetadataRepository.DidNotReceive().DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockDirectoryProviderService.DidNotReceive().DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteBooksFails_ShouldThrowEventualConsistencyExceptionWithoutDeletingAnythingElse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to delete books");
        _mockBookRepository.DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockMusicArtworkRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockMusicLibraryScanItemMetadataRepository.DidNotReceive().DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockDirectoryProviderService.DidNotReceive().DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteStagedMusicMetadataFails_ShouldThrowEventualConsistencyExceptionWithoutDeletingAnythingElse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to delete staged music metadata");
        _mockMusicLibraryScanItemMetadataRepository.DeleteByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockMusicArtworkRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        _mockDirectoryProviderService.DidNotReceive().DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenSanitizeSegmentFails_ShouldThrowEventualConsistencyExceptionWithoutDeletingItemDirectories()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to sanitize segment");
        _mockPathService.SanitizeSegment(Arg.Any<string>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockMusicArtworkRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockMusicLibraryScanItemMetadataRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        // Only the libraries directory, deleted before the segment is sanitized, was removed.
        _mockDirectoryProviderService.Received(1).DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsDeleted_ShouldDeleteEachMediaTypeDirectoryAtItsExactPath()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        string libraryTitle = "My Library";
        Library library = _libraryFixture.Create(id: libraryId, title: libraryTitle);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        string librarySegment = $"{libraryTitle}-{libraryId}";
        string expectedLibrariesPath = string.Concat(AppContext.BaseDirectory, "Media", "Libraries", libraryId.ToString());
        string expectedBooksPath = string.Concat(AppContext.BaseDirectory, "Media", "Books", librarySegment);
        string expectedMusicPath = string.Concat(AppContext.BaseDirectory, "Media", "Music", librarySegment);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockPathService.Received(1).SanitizeSegment(librarySegment);
        _mockDirectoryProviderService.Received(1).DeleteDirectory(Arg.Is<FileSystemPathId>(path => path.Path == expectedLibrariesPath));
        _mockDirectoryProviderService.Received(1).DeleteDirectory(Arg.Is<FileSystemPathId>(path => path.Path == expectedBooksPath));
        _mockDirectoryProviderService.Received(1).DeleteDirectory(Arg.Is<FileSystemPathId>(path => path.Path == expectedMusicPath));
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDirectoryDoesNotExist_ShouldSkipDeletingIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(false));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        // The media items are still removed, but a directory that does not exist is not an error.
        await _mockMusicArtworkRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        await _mockMusicLibraryScanItemMetadataRepository.Received(1).DeleteByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>());
        _mockDirectoryProviderService.DidNotReceive().DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenDirectoryExistsFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to check directory existence");
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        _mockDirectoryProviderService.DidNotReceive().DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenCombiningTheMediaTypeDirectoryPathFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, title: "My Library");
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to combine the media type directory path");
        // Only the combination of the media type segment of the books directory fails, after the libraries directory was already removed.
        _mockPathService.CombinePath(Arg.Any<string>(), "Books").Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        // Only the libraries directory, whose path was combined before the failure, was removed.
        _mockDirectoryProviderService.Received(1).DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenCombiningTheLibraryDirectoryPathFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        string libraryTitle = "My Library";
        Library library = _libraryFixture.Create(id: libraryId, title: libraryTitle);
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        string librarySegment = $"{libraryTitle}-{libraryId}";
        Error error = Error.Failure(description: "Failed to combine the library directory path");
        // Only the combination of the leaf library segment of the books directory fails.
        _mockPathService.CombinePath(Arg.Is<string>(path => path.EndsWith("Books", StringComparison.Ordinal)), librarySegment).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        // Only the libraries directory, whose path was combined before the failure, was removed.
        _mockDirectoryProviderService.Received(1).DeleteDirectory(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenCreatingTheLibraryPathIdFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, title: "My Library");
        LibraryDeletedDomainEvent domainEvent = _libraryDeletedDomainEventFixture.Create(library: library);
        // The combined leaf path of the libraries directory is blank, so the path value object cannot be created from it.
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Is(libraryId.ToString())).Returns(Result.From("   "));

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(DomainErrors.FileSystemManagement.InvalidPath, exception.EventualConsistencyError);
        _mockDirectoryProviderService.DidNotReceive().DeleteDirectory(Arg.Any<FileSystemPathId>());
    }
}
