#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Common.DataAccess.Repositories.Users;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Fixtures.Common.DTO.Pagination;
using Nemera.Application.Fixtures.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;
using Nemera.Contracts.Responses.UsersManagement;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;

/// <summary>
/// Contains unit tests for the <see cref="CheckInitializationQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckInitializationQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IUserRepository _mockUserRepository;
    private readonly CheckInitializationQueryHandler _sut;
    private readonly CheckInitializationQueryFixture _checkInitializationQueryFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly PaginatedResultDtoFixture<UserEntity> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckInitializationQueryHandlerTests"/> class.
    /// </summary>
    public CheckInitializationQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockUserRepository = Substitute.For<IUserRepository>();

        _mockUnitOfWork.UserRepository.Returns(_mockUserRepository);

        _sut = new CheckInitializationQueryHandler(_mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUsersExist_ShouldReturnInitialized()
    {
        // Arrange
        List<UserEntity> users = _userEntityFixture.CreateMany();
        _mockUserRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: users, currentPage: 1, perPage: users.Count, count: users.Count, numberOfPages: 1)));

        // Act
        InitializationResponse result = await _sut.HandleAsync(_checkInitializationQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsInitialized);
        await _mockUserRepository.Received(1).GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoUsersExist_ShouldReturnNotInitialized()
    {
        // Arrange
        _mockUserRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 0, count: 0, numberOfPages: 1)));

        // Act
        InitializationResponse result = await _sut.HandleAsync(_checkInitializationQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsInitialized);
        await _mockUserRepository.Received(1).GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnNotInitialized()
    {
        // Arrange
        Error error = Error.Failure("Database.Error", "Failed to retrieve users");
        _mockUserRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        InitializationResponse result = await _sut.HandleAsync(_checkInitializationQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsInitialized);
        await _mockUserRepository.Received(1).GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }
}
