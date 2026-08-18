#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanStartedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanStartedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly LibraryScanStartedDomainEventHandler _sut;
    private readonly LibraryScanFixture _libraryScanFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanStartedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryScanStartedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockLibraryScanRepository.UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _sut = new LibraryScanStartedDomainEventHandler(_mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScanStarts_ShouldUpdateScanStatusAndSaveChanges()
    {
        // Arrange
        LibraryScan libraryScan = _libraryScanFixture.Create();
        LibraryScanStartedDomainEvent domainEvent = new(Guid.NewGuid(), libraryScan, DateTime.UtcNow);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockLibraryScanRepository.Received(1).UpdateAsync(
            Arg.Is<LibraryScanEntity>(scan => scan.Id == libraryScan.Id.Value),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryScan libraryScan = _libraryScanFixture.Create();
        LibraryScanStartedDomainEvent domainEvent = new(Guid.NewGuid(), libraryScan, DateTime.UtcNow);
        Error error = Error.Failure(description: "Failed to update library scan");
        _mockLibraryScanRepository.UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
