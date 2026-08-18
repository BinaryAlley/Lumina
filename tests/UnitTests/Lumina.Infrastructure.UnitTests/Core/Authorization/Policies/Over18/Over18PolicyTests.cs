#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Core.Authorization.Policies.Over18;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authorization.Policies.Over18;

/// <summary>
/// Contains unit tests for the <see cref="Over18Policy"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class Over18PolicyTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IUserRepository _mockUserRepository;
    private readonly Over18Policy _sut;
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Over18PolicyTests"/> class.
    /// </summary>
    public Over18PolicyTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _mockUnitOfWork.UserRepository.Returns(_mockUserRepository);

        _sut = new Over18Policy(_mockUnitOfWork, Substitute.For<IDateTimeProvider>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        UserEntity user = _userEntityFixture.Create();
        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(user));

        // Act
        bool result = await _sut.EvaluateAsync(user.Id, null, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(null));

        // Act
        bool result = await _sut.EvaluateAsync(userId, null, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenGetUserReturnsFailure_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to retrieve user"));

        // Act
        bool result = await _sut.EvaluateAsync(userId, null, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenUserEntityIsNull_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result<UserEntity?>.Success(null));

        // Act
        bool result = await _sut.EvaluateAsync(userId, null, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
