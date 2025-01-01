#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.DeleteRole.Fixtures;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Contains unit tests for the <see cref="DeleteRoleCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IRoleRepository _mockRoleRepository;
    private readonly DeleteRoleCommandHandler _sut;
    private readonly DeleteRoleCommandFixture _deleteRoleCommandFixture;
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleCommandHandlerTests"/> class.
    /// </summary>
    public DeleteRoleCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockRoleRepository = Substitute.For<IRoleRepository>();
        _deleteRoleCommandFixture = new DeleteRoleCommandFixture();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(_mockRoleRepository);

        _sut = new DeleteRoleCommandHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        DeleteRoleCommand command = _deleteRoleCommandFixture.CreateCommand();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        ErrorOr<Deleted> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.NotAuthorized);
        await _mockRoleRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ShouldReturnRoleNotFoundError()
    {
        // Arrange
        DeleteRoleCommand command = _deleteRoleCommandFixture.CreateCommand();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        // Act
        ErrorOr<Deleted> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.RoleNotFound);
        await _mockRoleRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDeletingAdminRole_ShouldReturnAdminRoleCannotBeDeletedError()
    {
        // Arrange
        DeleteRoleCommand command = _deleteRoleCommandFixture.CreateCommand();
        RoleEntity adminRole = new() { Id = command.RoleId, RoleName = "Admin" };

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns(adminRole);

        // Act
        ErrorOr<Deleted> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.AdminRoleCannotBeDeleted);
        await _mockRoleRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetRoleByIdFails_ShouldReturnError()
    {
        // Arrange
        DeleteRoleCommand command = _deleteRoleCommandFixture.CreateCommand();
        Error error = Error.Failure("Database.Error", "Failed to get role");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<Deleted> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        await _mockRoleRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDeleteRoleFails_ShouldReturnError()
    {
        // Arrange
        DeleteRoleCommand command = _deleteRoleCommandFixture.CreateCommand();
        RoleEntity role = new() { Id = command.RoleId, RoleName = "TestRole" };
        Error error = Error.Failure("Database.Error", "Failed to delete role");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _mockRoleRepository.DeleteByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<Deleted> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAllOperationsSucceed_ShouldReturnSuccess()
    {
        // Arrange
        DeleteRoleCommand command = _deleteRoleCommandFixture.CreateCommand();
        RoleEntity role = new() { Id = command.RoleId, RoleName = "TestRole" };

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _mockRoleRepository.DeleteByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);

        // Act
        ErrorOr<Deleted> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
