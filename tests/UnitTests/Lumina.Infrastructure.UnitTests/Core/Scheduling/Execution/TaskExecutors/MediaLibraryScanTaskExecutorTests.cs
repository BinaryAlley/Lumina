#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanTaskExecutor"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanTaskExecutorTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly MediaLibraryScanTaskExecutor _sut;
    private readonly ScheduledJobFixture _scheduledJobFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanTaskExecutorTests"/> class.
    /// </summary>
    public MediaLibraryScanTaskExecutorTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<LibraryEntity>>([]));
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _sut = new MediaLibraryScanTaskExecutor(_mockDomainEventPublisher, Substitute.For<ILogger<MediaLibraryScanTaskExecutor>>(), _mockUnitOfWork);
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenNoLibrariesAreEnabled_ShouldReturnSuccessWithoutPublishingEvents()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenGetAllEnabledAndUnlockedLibrariesFails_ShouldReturnError()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get the enabled libraries");
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenGetPastMonthScansFails_ShouldReturnError()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create();
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<LibraryEntity>>([library]));
        Error error = Error.Failure("Database.Error", "Failed to get the past month scans");
        _mockUnitOfWork.LibraryScanRepository.GetPastMonthScansByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
