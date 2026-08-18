#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Contains unit tests for the <see cref="ScanFailurePublisher"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanFailurePublisherTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanFailurePublisherTests"/> class.
    /// </summary>
    public ScanFailurePublisherTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);
    }

    [Fact]
    public async Task PublishAsync_WhenCalled_ShouldPublishLibraryScanFailedDomainEvent()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        Exception exception = new("The scan job failed");

        // Act
        await ScanFailurePublisher.PublishAsync(_mockServiceScopeFactory, libraryId, compositeId, exception, CancellationToken.None);

        // Assert
        await _mockDomainEventPublisher.Received(1).PublishAsync(
            Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
                domainEvent.LibraryId == libraryId
                && domainEvent.MediaLibraryScanCompositeId == compositeId
                && domainEvent.ErrorMessage == "The scan job failed"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WhenPublisherThrows_ShouldNotThrow()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask(Task.FromException(new InvalidOperationException("Publishing failed"))));

        // Act
        Exception? caughtException = null;
        try
        {
            await ScanFailurePublisher.PublishAsync(_mockServiceScopeFactory, libraryId, compositeId, new Exception("boom"), CancellationToken.None);
        }
        catch (Exception exception)
        {
            caughtException = exception;
        }

        // Assert
        Assert.Null(caughtException);
    }
}
