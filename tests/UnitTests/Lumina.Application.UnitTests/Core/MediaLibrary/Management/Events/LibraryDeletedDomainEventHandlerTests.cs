#region ========================================================================= USING =====================================================================================
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
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryDeletedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryDeletedDomainEventHandlerTests
{
    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly IDirectoryProviderService _mockDirectoryProviderService;
    private readonly IPathService _mockPathService;
    private readonly LibraryDeletedDomainEventHandler _sut;
    private readonly LibraryFixture _libraryFixture = new();
    private readonly MediaSettingsDtoFixture _mediaSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryDeletedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryDeletedDomainEventHandlerTests()
    {
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockDirectoryProviderService = Substitute.For<IDirectoryProviderService>();
        _mockEnvironmentContext.DirectoryProviderService.Returns(_mockDirectoryProviderService);
        _mockPathService = Substitute.For<IPathService>();

        MediaSettingsDto mediaSettings = _mediaSettingsDtoFixture.Create(
            rootDirectory: "Media",
            librariesDirectory: "Libraries",
            booksDirectory: "Books");
        IOptions<MediaSettingsDto> mediaSettingsOptions = Substitute.For<IOptions<MediaSettingsDto>>();
        mediaSettingsOptions.Value.Returns(mediaSettings);

        // default stubs: all path combinations succeed and the directory is deleted successfully
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(string.Concat(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1))));
        _mockDirectoryProviderService.DeleteDirectory(Arg.Any<FileSystemPathId>()).Returns(Result.Deleted);

        _sut = new LibraryDeletedDomainEventHandler(_mockEnvironmentContext, _mockPathService, mediaSettingsOptions);
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsDeleted_ShouldDeleteLibraryDirectory()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = new(Guid.NewGuid(), library, DateTime.UtcNow);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockDirectoryProviderService.Received(1).DeleteDirectory(
            Arg.Is<FileSystemPathId>(path => path.Path.EndsWith(libraryId.ToString(), StringComparison.Ordinal)));
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteDirectoryFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Library library = _libraryFixture.Create(id: libraryId);
        LibraryDeletedDomainEvent domainEvent = new(Guid.NewGuid(), library, DateTime.UtcNow);
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
        LibraryDeletedDomainEvent domainEvent = new(Guid.NewGuid(), library, DateTime.UtcNow);
        Error error = Error.Failure(description: "Failed to combine path");
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        _mockDirectoryProviderService.DidNotReceive().DeleteDirectory(Arg.Any<FileSystemPathId>());
    }
}
