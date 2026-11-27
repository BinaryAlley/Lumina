#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;
using Nemera.Contracts.Responses.MediaLibrary.Management;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;

/// <summary>
/// Contains unit tests for the <see cref="GetRunningLibraryScansQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRunningLibraryScansQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IMediaLibrariesScanProgressTracker _mockMediaLibrariesScanProgressTracker;
    private readonly GetRunningLibraryScansQueryHandler _sut;
    private readonly GetRunningLibraryScansQueryFixture _getRunningLibraryScansQueryFixture = new();
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly MediaLibraryScanProgressFixture _mediaLibraryScanProgressFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRunningLibraryScansQueryHandlerTests"/> class.
    /// </summary>
    public GetRunningLibraryScansQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockMediaLibrariesScanProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, is an admin, and every scan has progress.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);
        _mockMediaLibrariesScanProgressTracker.GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>())
            .Returns(Result.From(_mediaLibraryScanProgressFixture.Create(
                userId: _userIdFixture.Create(value: _userId),
                completedJobs: 1,
                totalJobs: 2,
                status: LibraryScanJobStatus.Running)));

        _sut = new GetRunningLibraryScansQueryHandler(_mockMediaLibrariesScanProgressTracker, _mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAdmin_ShouldReturnProgressForAllActiveScans()
    {
        // Arrange
        List<LibraryScanEntity> activeScans =
        [
            _libraryScanEntityFixture.Create(userId: _userId),
            _libraryScanEntityFixture.Create(userId: Guid.NewGuid())
        ];
        _mockLibraryScanRepository.GetActiveScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(activeScans));

        // Act
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sut.HandleAsync(_getRunningLibraryScansQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnProgressOnlyForScansOwnedByTheUser()
    {
        // Arrange
        List<LibraryScanEntity> activeScans =
        [
            _libraryScanEntityFixture.Create(userId: _userId),
            _libraryScanEntityFixture.Create(userId: Guid.NewGuid())
        ];
        _mockLibraryScanRepository.GetActiveScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(activeScans));
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sut.HandleAsync(_getRunningLibraryScansQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenActiveScanIsPending_ShouldReturnItsQueuedProgress()
    {
        // Arrange
        LibraryScanEntity pendingScan = _libraryScanEntityFixture.Create(userId: _userId, status: LibraryScanJobStatus.Pending);
        _mockLibraryScanRepository.GetActiveScansAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<LibraryScanEntity>>([pendingScan]));
        _mockMediaLibrariesScanProgressTracker.GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>())
            .Returns(Result.From(_mediaLibraryScanProgressFixture.Create(
                scanId: ScanId.Create(pendingScan.Id),
                userId: _userIdFixture.Create(value: _userId),
                libraryId: LibraryId.Create(pendingScan.LibraryId),
                completedJobs: 0,
                totalJobs: 0,
                status: LibraryScanJobStatus.Pending)));

        // Act
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sut.HandleAsync(_getRunningLibraryScansQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        MediaLibraryScanProgressResponse response = Assert.Single(result.Value);
        Assert.Equal(pendingScan.Id, response.ScanId);
        Assert.Equal(LibraryScanJobStatus.Pending.ToString(), response.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sut.HandleAsync(_getRunningLibraryScansQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().GetActiveScansAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetActiveScansFails_ShouldReturnError()
    {
        // Arrange
        _mockLibraryScanRepository.GetActiveScansAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get active scans"));

        // Act
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sut.HandleAsync(_getRunningLibraryScansQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task HandleAsync_WhenAnActiveScanHasNoProgress_ShouldSkipThatScan()
    {
        // Arrange
        List<LibraryScanEntity> activeScans =
        [
            _libraryScanEntityFixture.Create(userId: _userId),
            _libraryScanEntityFixture.Create(userId: _userId)
        ];
        _mockLibraryScanRepository.GetActiveScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(activeScans));
        // A scan whose progress was already released has ended, so it is not reported as active any more, without failing the whole query.
        MediaLibraryScanProgress releasedProgress = _mediaLibraryScanProgressFixture.Create(
            userId: _userIdFixture.Create(value: _userId),
            completedJobs: 1,
            totalJobs: 2,
            status: LibraryScanJobStatus.Running);
        _mockMediaLibrariesScanProgressTracker.GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>())
            .Returns(Result.From(releasedProgress), Error.Failure(description: "The scan progress was already released"));

        // Act
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sut.HandleAsync(_getRunningLibraryScansQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
    }
}
