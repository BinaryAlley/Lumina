#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Contains unit tests for the <see cref="BooksFileSystemDiscoveryJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksFileSystemDiscoveryJobTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ILibraryScanStagingResultsRepository _mockStagingResultsRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly BooksFileSystemDiscoveryJob _sut;
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanId _scanId;
    private readonly UserId _userId;
    private readonly LibraryId _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksFileSystemDiscoveryJobTests"/> class.
    /// </summary>
    public BooksFileSystemDiscoveryJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockStagingResultsRepository = Substitute.For<ILibraryScanStagingResultsRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.LibraryScanStagingResultsRepository.Returns(_mockStagingResultsRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();
        _sut = new BooksFileSystemDiscoveryJob(_mockServiceScopeFactory)
        {
            ScanId = _scanId,
            UserId = _userId,
            LibraryId = _libraryId
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingTheLibraryFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to retrieve the library"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockStagingResultsRepository.DidNotReceive().InsertRangeAsync(Arg.Any<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId
            && domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.MediaLibraryScanCompositeId.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryDoesNotExist_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldMarkJobAsCanceledAndThrow()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        Task operation = _sut.ExecuteAsync(Guid.NewGuid(), new { }, cancellationTokenSource.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
        Assert.Equal(LibraryScanJobStatus.Canceled, _sut.Status);
    }
}
