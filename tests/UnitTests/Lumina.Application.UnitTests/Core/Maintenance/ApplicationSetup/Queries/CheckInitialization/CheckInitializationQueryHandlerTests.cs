#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;

/// <summary>
/// Contains unit tests for the <see cref="CheckInitializationQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckInitializationQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IUserRepository _mockUserRepository;
    private readonly CheckInitializationQueryHandler _sut;
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckInitializationQueryHandlerTests"/> class.
    /// </summary>
    public CheckInitializationQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUserRepository = Substitute.For<IUserRepository>();

        _mockUnitOfWork.UserRepository.Returns(_mockUserRepository);

        _sut = new CheckInitializationQueryHandler(_mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUsersExist_ShouldReturnInitialized()
    {
        // Arrange
        List<UserEntity> users = _userEntityFixture.CreateMany();
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(users.AsEnumerable()));

        // Act
        InitializationResponse result = await _sut.HandleAsync(new CheckInitializationQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsInitialized);
        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoUsersExist_ShouldReturnNotInitialized()
    {
        // Arrange
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<UserEntity>()));

        // Act
        InitializationResponse result = await _sut.HandleAsync(new CheckInitializationQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsInitialized);
        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnNotInitialized()
    {
        // Arrange
        Error error = Error.Failure("Database.Error", "Failed to retrieve users");
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        InitializationResponse result = await _sut.HandleAsync(new CheckInitializationQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsInitialized);
        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }
}
