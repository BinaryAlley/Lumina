#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanJobTests
{
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();

    [Fact]
    public void Status_WhenCreated_ShouldBePending()
    {
        // Act
        TestMediaLibraryScanJob job = CreateJob();

        // Assert
        Assert.Equal(LibraryScanJobStatus.Pending, job.Status);
        Assert.Empty(job.Children);
        Assert.Empty(job.Parents);
    }

    [Fact]
    public void AddChild_WhenCalledWithNewJob_ShouldAddChild()
    {
        // Arrange
        TestMediaLibraryScanJob job = CreateJob();
        TestMediaLibraryScanJob child = CreateJob();

        // Act
        job.AddChild(child);

        // Assert
        Assert.Single(job.Children);
        Assert.Contains(child, job.Children);
    }

    [Fact]
    public void AddChild_WhenJobIsAlreadyChild_ShouldNotAddDuplicate()
    {
        // Arrange
        TestMediaLibraryScanJob job = CreateJob();
        TestMediaLibraryScanJob child = CreateJob();

        // Act
        job.AddChild(child);
        job.AddChild(child);

        // Assert
        Assert.Single(job.Children);
    }

    [Fact]
    public void AddChild_WhenJobIsParent_ShouldNotAddChild()
    {
        // Arrange
        TestMediaLibraryScanJob job = CreateJob();
        TestMediaLibraryScanJob parent = CreateJob();
        job.AddParent(parent);

        // Act
        job.AddChild(parent);

        // Assert
        Assert.Empty(job.Children);
        Assert.Single(job.Parents);
    }

    [Fact]
    public void AddParent_WhenCalledWithNewJob_ShouldAddParent()
    {
        // Arrange
        TestMediaLibraryScanJob job = CreateJob();
        TestMediaLibraryScanJob parent = CreateJob();

        // Act
        job.AddParent(parent);

        // Assert
        Assert.Single(job.Parents);
        Assert.Contains(parent, job.Parents);
    }

    [Fact]
    public void AddParent_WhenJobIsAlreadyParent_ShouldNotAddDuplicate()
    {
        // Arrange
        TestMediaLibraryScanJob job = CreateJob();
        TestMediaLibraryScanJob parent = CreateJob();

        // Act
        job.AddParent(parent);
        job.AddParent(parent);

        // Assert
        Assert.Single(job.Parents);
    }

    [Fact]
    public void AddParent_WhenJobIsChild_ShouldNotAddParent()
    {
        // Arrange
        TestMediaLibraryScanJob job = CreateJob();
        TestMediaLibraryScanJob child = CreateJob();
        job.AddChild(child);

        // Act
        job.AddParent(child);

        // Assert
        Assert.Single(job.Children);
        Assert.Empty(job.Parents);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldCompleteWithoutChangingJobState()
    {
        // Arrange
        TestMediaLibraryScanJob job = CreateJob();

        // Act
        await job.ExecuteAsync(Guid.NewGuid(), "input", CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Pending, job.Status);
        Assert.Empty(job.Children);
        Assert.Empty(job.Parents);
    }

    private TestMediaLibraryScanJob CreateJob()
    {
        return new TestMediaLibraryScanJob
        {
            ScanId = _scanIdFixture.Create(),
            UserId = _userIdFixture.Create(),
            LibraryId = _libraryIdFixture.Create()
        };
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="MediaLibraryScanJob"/> class.
    /// </summary>
    private sealed class TestMediaLibraryScanJob : MediaLibraryScanJob
    {
        /// <summary>
        /// Executes the payload of the media library scan job.
        /// </summary>
        /// <typeparam name="TInput">The type of the input parameter.</typeparam>
        /// <param name="id">The unique identifier of the media library scan job.</param>
        /// <param name="input">The input data to be processed.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
