#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Contains unit tests for the <see cref="RepairThemesTaskExecutor"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RepairThemesTaskExecutorTests
{
    private readonly IThemeService _mockThemeService;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly RepairThemesTaskExecutor _sut;
    private readonly ScheduledJobFixture _scheduledJobFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RepairThemesTaskExecutorTests"/> class.
    /// </summary>
    public RepairThemesTaskExecutorTests()
    {
        _mockThemeService = Substitute.For<IThemeService>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        ILogger<RepairThemesTaskExecutor> logger = Substitute.For<ILogger<RepairThemesTaskExecutor>>();
        _sut = new RepairThemesTaskExecutor(_mockThemeService, logger, _mockUnitOfWork);
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenCalled_ShouldSynchronizeTheThemes()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(taskType: ScheduledTaskType.RepairThemes);
        _mockThemeService.GetBundledThemeArchivePaths().Returns([]);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From(Enumerable.Empty<ThemeEntity>()));

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockThemeRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenABundledThemeArchiveCannotBeRead_ShouldStillSucceed()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(taskType: ScheduledTaskType.RepairThemes);
        _mockThemeService.GetBundledThemeArchivePaths().Returns(["missing.zip"]);
        _mockThemeService.ReadManifestFromArchiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Theme.Manifest", "Failed to read the theme manifest"));
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From(Enumerable.Empty<ThemeEntity>()));

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockThemeRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenTheSynchronizationThrows_ShouldReturnFailure()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(taskType: ScheduledTaskType.RepairThemes);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<IEnumerable<ThemeEntity>>>(new InvalidOperationException("The database is unavailable.")));

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(nameof(RepairThemesTaskExecutor), result.FirstError.Description);
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenTheSynchronizationIsCancelled_ShouldRethrowTheCancellation()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(taskType: ScheduledTaskType.RepairThemes);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<IEnumerable<ThemeEntity>>>(new OperationCanceledException()));

        // Act
        async Task Act()
        {
            await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(Act);
    }
}
