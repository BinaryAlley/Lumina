#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;

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
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockMediaLibrariesScanProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated, is an admin, and every scan has progress
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
    public async Task HandleAsync_WhenUserIsAdmin_ShouldReturnProgressForAllRunningScans()
    {
        // Arrange
        List<LibraryScanEntity> runningScans =
        [
            _libraryScanEntityFixture.Create(userId: _userId),
            _libraryScanEntityFixture.Create(userId: Guid.NewGuid())
        ];
        _mockLibraryScanRepository.GetRunningScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(runningScans));

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
        List<LibraryScanEntity> runningScans =
        [
            _libraryScanEntityFixture.Create(userId: _userId),
            _libraryScanEntityFixture.Create(userId: Guid.NewGuid())
        ];
        _mockLibraryScanRepository.GetRunningScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(runningScans));
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sut.HandleAsync(_getRunningLibraryScansQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
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
        await _mockLibraryScanRepository.DidNotReceive().GetRunningScansAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetRunningScansFails_ShouldReturnError()
    {
        // Arrange
        _mockLibraryScanRepository.GetRunningScansAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get running scans"));

        // Act
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sut.HandleAsync(_getRunningLibraryScansQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task HandleAsync_WhenGettingScanProgressFails_ShouldReturnError()
    {
        // Arrange
        List<LibraryScanEntity> runningScans = [_libraryScanEntityFixture.Create(userId: _userId)];
        _mockLibraryScanRepository.GetRunningScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(runningScans));
        _mockMediaLibrariesScanProgressTracker.GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>())
            .Returns(Error.Failure(description: "Failed to get scan progress"));

        // Act
        Result<IEnumerable<MediaLibraryScanProgressResponse>> result = await _sut.HandleAsync(_getRunningLibraryScansQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }
}
