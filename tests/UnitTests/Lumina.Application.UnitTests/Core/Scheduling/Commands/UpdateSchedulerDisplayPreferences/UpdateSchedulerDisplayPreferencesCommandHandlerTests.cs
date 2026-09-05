#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSchedulerDisplayPreferencesCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ISchedulerDisplayPreferencesRepository _mockSchedulerDisplayPreferencesRepository;
    private readonly IValidator<UpdateSchedulerDisplayPreferencesCommand> _mockValidator;
    private readonly UpdateSchedulerDisplayPreferencesCommandHandler _sut;
    private readonly UpdateSchedulerDisplayPreferencesCommandFixture _updateSchedulerDisplayPreferencesCommandFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesCommandHandlerTests"/> class.
    /// </summary>
    public UpdateSchedulerDisplayPreferencesCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockSchedulerDisplayPreferencesRepository = Substitute.For<ISchedulerDisplayPreferencesRepository>();

        _mockUnitOfWork.SchedulerDisplayPreferencesRepository.Returns(_mockSchedulerDisplayPreferencesRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _mockValidator = Substitute.For<IValidator<UpdateSchedulerDisplayPreferencesCommand>>();
        _mockValidator.Validate(Arg.Any<UpdateSchedulerDisplayPreferencesCommand>()).Returns([]);

        _sut = new UpdateSchedulerDisplayPreferencesCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValid_ShouldUpsertDisplayPreferencesAndReturnUpdated()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesCommand command = _updateSchedulerDisplayPreferencesCommandFixture.Create(
            jobTypeFilter: ScheduledTaskType.ScanMediaLibraries,
            displayTimeSpan: 30,
            displayTimeUnit: SchedulerDisplayTimeUnit.Minutes);
        _mockSchedulerDisplayPreferencesRepository.UpsertAsync(Arg.Any<SchedulerDisplayPreferencesEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        await _mockSchedulerDisplayPreferencesRepository.Received(1).UpsertAsync(
            Arg.Is<SchedulerDisplayPreferencesEntity>(preferences =>
                preferences.UserId == _userId &&
                preferences.JobTypeFilter == ScheduledTaskType.ScanMediaLibraries &&
                preferences.DisplayTimeSpan == 30 &&
                preferences.DisplayTimeUnit == SchedulerDisplayTimeUnit.Minutes),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotPersist()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesCommand command = _updateSchedulerDisplayPreferencesCommandFixture.Create(displayTimeSpan: 0);
        _mockValidator.Validate(Arg.Any<UpdateSchedulerDisplayPreferencesCommand>())
            .Returns([DomainErrors.Scheduling.SchedulerDisplayTimeSpanMustBePositive]);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.SchedulerDisplayTimeSpanMustBePositive, result.FirstError);
        await _mockSchedulerDisplayPreferencesRepository.DidNotReceive().UpsertAsync(Arg.Any<SchedulerDisplayPreferencesEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorized()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesCommand command = _updateSchedulerDisplayPreferencesCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockSchedulerDisplayPreferencesRepository.DidNotReceive().UpsertAsync(Arg.Any<SchedulerDisplayPreferencesEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdministrator_ShouldReturnNotAuthorized()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesCommand command = _updateSchedulerDisplayPreferencesCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockSchedulerDisplayPreferencesRepository.DidNotReceive().UpsertAsync(Arg.Any<SchedulerDisplayPreferencesEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpsertFails_ShouldReturnErrorAndNotSaveChanges()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesCommand command = _updateSchedulerDisplayPreferencesCommandFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to upsert the display preferences");
        _mockSchedulerDisplayPreferencesRepository.UpsertAsync(Arg.Any<SchedulerDisplayPreferencesEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Updated> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
