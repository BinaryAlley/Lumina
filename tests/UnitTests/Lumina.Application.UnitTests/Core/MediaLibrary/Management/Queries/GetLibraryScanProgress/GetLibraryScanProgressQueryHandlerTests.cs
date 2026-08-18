#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
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
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryScanProgressQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryScanProgressQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IMediaLibrariesScanProgressTracker _mockMediaLibrariesScanProgressTracker;
    private readonly IValidator<GetLibraryScanProgressQuery> _mockValidator;
    private readonly GetLibraryScanProgressQueryHandler _sut;
    private readonly GetLibraryScanProgressQueryFixture _getLibraryScanProgressQueryFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaLibraryScanProgressFixture _mediaLibraryScanProgressFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryScanProgressQueryHandlerTests"/> class.
    /// </summary>
    public GetLibraryScanProgressQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockMediaLibrariesScanProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _mockValidator = Substitute.For<IValidator<GetLibraryScanProgressQuery>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated, the ownership policy allows access, and the scan has progress
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockMediaLibrariesScanProgressTracker.GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>())
            .Returns(Result.From(_mediaLibraryScanProgressFixture.Create(
                userId: _userIdFixture.Create(value: _userId),
                completedJobs: 1,
                totalJobs: 2,
                status: LibraryScanJobStatus.Running)));
        _mockValidator.Validate(Arg.Any<GetLibraryScanProgressQuery>()).Returns([]);

        _sut = new GetLibraryScanProgressQueryHandler(_mockMediaLibrariesScanProgressTracker, _mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyAllowsAccess_ShouldReturnScanProgressResponse()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: query.LibraryId, userId: _userId);
        _mockLibraryRepository.GetByIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<MediaLibraryScanProgressResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.CompletedJobs);
        Assert.Equal(2, result.Value.TotalJobs);
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();
        _mockLibraryRepository.GetByIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result<LibraryEntity?>.Success(null));

        // Act
        Result<MediaLibraryScanProgressResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        _mockMediaLibrariesScanProgressTracker.DidNotReceive().GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();
        _mockLibraryRepository.GetByIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get library"));

        // Act
        Result<MediaLibraryScanProgressResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _mockMediaLibrariesScanProgressTracker.DidNotReceive().GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: query.LibraryId, userId: _userId);
        _mockLibraryRepository.GetByIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<MediaLibraryScanProgressResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        _mockMediaLibrariesScanProgressTracker.DidNotReceive().GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<MediaLibraryScanProgressResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGettingScanProgressFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: query.LibraryId, userId: _userId);
        _mockLibraryRepository.GetByIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockMediaLibrariesScanProgressTracker.GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>())
            .Returns(Error.Failure(description: "Failed to get scan progress"));

        // Act
        Result<MediaLibraryScanProgressResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetLibraryScanProgressQuery>()).Returns([DomainErrors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<MediaLibraryScanProgressResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockMediaLibrariesScanProgressTracker.DidNotReceive().GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>());
    }
}
