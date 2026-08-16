#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Core.UsersManagement.Settings.Queries.GetUserSettings;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Settings.Queries.GetUserSettings;

/// <summary>
/// Contains unit tests for the <see cref="GetUserSettingsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly GetUserSettingsQueryHandler _sut;
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsQueryHandlerTests"/> class.
    /// </summary>
    public GetUserSettingsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();

        _mockUnitOfWork.UserSettingsRepository.Returns(_mockUserSettingsRepository);
        _mockCurrentUserService.UserId.Returns(_userId);

        _sut = new GetUserSettingsQueryHandler(_mockCurrentUserService, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenSettingsExist_ShouldReturnStoredSettings()
    {
        // Arrange
        GetUserSettingsQuery query = new();
        UserSettingsEntity storedSettings = _userSettingsEntityFixture.Create(userId: _userId);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(storedSettings));

        // Act
        Result<UserSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(storedSettings.UserId, result.Value.UserId);
        Assert.Equal(storedSettings.IsPaginationEnabled, result.Value.IsPaginationEnabled);
        Assert.Equal(storedSettings.ItemsPerPage, result.Value.ItemsPerPage);
        Assert.Equal(storedSettings.IgnoreThePrefixForAlphaPicker, result.Value.IgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public async Task HandleAsync_WhenSettingsDoNotExist_ShouldReturnDefaultSettings()
    {
        // Arrange
        GetUserSettingsQuery query = new();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(null));

        // Act
        Result<UserSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value.UserId);
        Assert.True(result.Value.IsPaginationEnabled);
        Assert.Equal(48, result.Value.ItemsPerPage);
        Assert.False(result.Value.IgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorized()
    {
        // Arrange
        GetUserSettingsQuery query = new();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<UserSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockUserSettingsRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdReturnsError_ShouldReturnError()
    {
        // Arrange
        GetUserSettingsQuery query = new();
        Error error = Error.Failure("Database.Error", "Failed to retrieve user settings");
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<UserSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
