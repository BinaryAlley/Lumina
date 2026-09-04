#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibrarySavedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibrarySavedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly IFileProviderService _mockFileProviderService;
    private readonly IDirectoryProviderService _mockDirectoryProviderService;
    private readonly IFileTypeService _mockFileTypeService;
    private readonly IPathService _mockPathService;
    private readonly LibrarySavedDomainEventHandler _sut;
    private readonly LibraryFixture _libraryFixture = new();
    private readonly LibrarySavedDomainEventFixture _librarySavedDomainEventFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly MediaSettingsDtoFixture _mediaSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrarySavedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibrarySavedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockFileProviderService = Substitute.For<IFileProviderService>();
        _mockDirectoryProviderService = Substitute.For<IDirectoryProviderService>();
        _mockFileTypeService = Substitute.For<IFileTypeService>();
        _mockEnvironmentContext.FileProviderService.Returns(_mockFileProviderService);
        _mockEnvironmentContext.DirectoryProviderService.Returns(_mockDirectoryProviderService);
        _mockEnvironmentContext.FileTypeService.Returns(_mockFileTypeService);
        _mockPathService = Substitute.For<IPathService>();

        MediaSettingsDto mediaSettings = _mediaSettingsDtoFixture.Create(
            rootDirectory: "Media",
            librariesDirectory: "Libraries",
            booksDirectory: "Books");
        IOptions<MediaSettingsDto> mediaSettingsOptions = Substitute.For<IOptions<MediaSettingsDto>>();
        mediaSettingsOptions.Value.Returns(mediaSettings);

        // default stubs: the cover source file exists and is a valid image, the library directory does not exist yet, and all path combinations succeed
        _mockPathService.PathSeparator.Returns('/');
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(string.Concat(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1))));
        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
        _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.PNG));
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(false));
        _mockDirectoryProviderService.CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(Result.From(_fileSystemPathIdFixture.Create(path: "C:/Media/Libraries/guid")));
        _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
            .Returns(Result.From(_fileSystemPathIdFixture.Create(path: "C:/Media/Libraries/guid/cover.png")));
        _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(_fileSystemPathIdFixture.Create(path: string.Concat(AppContext.BaseDirectory, "libraries/guid/", callInfo.ArgAt<string>(1)))));
        _mockLibraryRepository.UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _sut = new LibrarySavedDomainEventHandler(_mockUnitOfWork, _mockEnvironmentContext, _mockPathService, mediaSettingsOptions);
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryHasCoverImage_ShouldCopyCoverToMediaDirectoryAndUpdateLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, coverImage: "C:/Users/user/cover.jpg");
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(library: library);
        LibraryEntity updatedLibrary = _libraryEntityFixture.Create(id: libraryId);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        Assert.NotNull(domainEvent.Library.CoverImage);
        await _mockLibraryRepository.Received(1).UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryCoverDirectoryExists_ShouldDeletePreviousCoversAndCopyNewCover()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, coverImage: "C:/Users/user/cover.jpg");
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(library: library);
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
        _mockFileProviderService.GetFilePaths(Arg.Any<FileSystemPathId>(), true)
            .Returns(Result.From<IEnumerable<FileSystemPathId>>(
            [
                _fileSystemPathIdFixture.Create(path: "C:/Media/Libraries/guid/cover.jpg"),
                _fileSystemPathIdFixture.Create(path: "C:/Media/Libraries/guid/notes.txt")
            ]));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        FileSystemPathId coverFile = _fileSystemPathIdFixture.Create(path: "C:/Media/Libraries/guid/cover.jpg");
        _mockFileProviderService.Received(2).DeleteFile(coverFile);
        _mockFileProviderService.DidNotReceive().DeleteFile(_fileSystemPathIdFixture.Create(path: "C:/Media/Libraries/guid/notes.txt"));
        await _mockLibraryRepository.Received(1).UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryHasNoCoverImage_ShouldDeleteExistingCoverFromMediaDirectory()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, includeCoverImage: false);
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(library: library);
        _mockFileProviderService.GetFilePaths(Arg.Any<FileSystemPathId>(), true)
            .Returns(Result.From<IEnumerable<FileSystemPathId>>(
            [
                _fileSystemPathIdFixture.Create(path: "C:/Media/Libraries/guid/cover.png")
            ]));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockFileProviderService.Received(1).DeleteFile(Arg.Any<FileSystemPathId>());
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCoverImageIsAlreadyInMediaDirectory_ShouldKeepItWithoutRecopying()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, coverImage: "/Media/Libraries/guid/cover.png");
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(library: library);
        // mirror the real CombinePath behavior, which treats the combined name as a directory segment and appends a trailing
        // separator; the internal cover path resolution must still yield the file path without that trailing separator
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(string.Concat(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)) + '/'));
        string internalImagePath = $"{AppContext.BaseDirectory.TrimEnd('/')}/Media/Libraries/guid/cover.png";
        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>())
            .Returns(callInfo => Result.From(callInfo.Arg<FileSystemPathId>().Path == internalImagePath));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        Assert.Equal("/Media/Libraries/guid/cover.png", domainEvent.Library.CoverImage);
        _mockFileProviderService.DidNotReceive().CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), Arg.Any<bool>());
        await _mockLibraryRepository.Received(1).UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCoverSourceFileDoesNotExist_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, coverImage: "C:/Users/user/cover.jpg");
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(library: library);
        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(false));

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(DomainErrors.FileSystemManagement.FileNotFound, exception.EventualConsistencyError);
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCoverFileIsNotAnImage_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, coverImage: "C:/Users/user/cover.jpg");
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(library: library);
        _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.None));

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(DomainErrors.Library.CoverFileMustBeAnImage, exception.EventualConsistencyError);
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCopyFileFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, coverImage: "C:/Users/user/cover.jpg");
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to copy cover file");
        _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetLibraryPathFailsForLibraryWithoutCover_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, includeCoverImage: false);
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to combine path");
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        _mockFileProviderService.DidNotReceive().DeleteFile(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId, coverImage: "C:/Users/user/cover.jpg");
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create(library: library);
        Error error = Error.Failure(description: "Failed to update library");
        _mockLibraryRepository.UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
