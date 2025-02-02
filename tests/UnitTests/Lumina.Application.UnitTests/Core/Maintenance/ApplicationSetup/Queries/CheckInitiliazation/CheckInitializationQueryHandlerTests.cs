#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Contracts.Responses.UsersManagement;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Maintenance.ApplicationSetup.Queries.CheckInitiliazation;

/// <summary>
/// Contains unit tests for the <see cref="CheckInitializationQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckInitializationQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IUserRepository _mockUserRepository;
    private readonly CheckInitializationQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckInitializationQueryHandlerTests"/> class.
    /// </summary>
    public CheckInitializationQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUserRepository = Substitute.For<IUserRepository>();

        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);

        _sut = new CheckInitializationQueryHandler(_mockUnitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUsersExist_ShouldReturnInitialized()
    {
        // Arrange
        List<UserEntity> users = UserEntityFixture.CreateMany();
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(users.AsEnumerable()));

        // Act
        InitializationResponse result = await _sut.Handle(new CheckInitializationQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsInitialized);
        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoUsersExist_ShouldReturnNotInitialized()
    {
        // Arrange
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Enumerable.Empty<UserEntity>()));

        // Act
        InitializationResponse result = await _sut.Handle(new CheckInitializationQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsInitialized);
        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsError_ShouldReturnNotInitialized()
    {
        // Arrange
        Error error = Error.Failure("Database.Error", "Failed to retrieve users");
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        InitializationResponse result = await _sut.Handle(new CheckInitializationQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsInitialized);
        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }
}
