#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Contracts.Responses.Authentication;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.ChangePassword;

/// <summary>
/// Contains unit tests for the <see cref="ChangePasswordCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IHashService _mockHashService;
    private readonly IUserRepository _mockUserRepository;
    private readonly ChangePasswordCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCommandHandlerTests"/> class.
    /// </summary>
    public ChangePasswordCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockHashService = Substitute.For<IHashService>();
        _mockUserRepository = Substitute.For<IUserRepository>();

        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);

        _sut = new ChangePasswordCommandHandler(
            _mockUnitOfWork,
            _mockHashService);
    }

    [Fact]
    public async Task Handle_WhenPasswordChangeIsSuccessful_ShouldUpdateUserAndReturnSuccess()
    {
        // Arrange
        string currentPassword = "currentPassword";
        string newPassword = "newPassword";
        string hashedNewPassword = "hashedNewPassword";
        string escapedHashedCurrentPassword = Uri.EscapeDataString("hashedCurrentPassword");

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = escapedHashedCurrentPassword;

        ChangePasswordCommand command = new(
            user.Username,
            currentPassword,
            newPassword,
            newPassword);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(currentPassword, "hashedCurrentPassword")
            .Returns(true);
        _mockHashService.HashString(newPassword)
            .Returns(hashedNewPassword);
        _mockUserRepository.UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        ErrorOr<ChangePasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value.IsPasswordChanged);

        await _mockUserRepository.Received(1).GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>());
        _mockHashService.Received(1).CheckStringAgainstHash(currentPassword, "hashedCurrentPassword");
        _mockHashService.Received(1).HashString(newPassword);
        await _mockUserRepository.Received(1).UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnError()
    {
        // Arrange
        ChangePasswordCommand command = new("nonexistentUser", "oldPass", "newPass", "newPass");

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(null));

        // Act
        ErrorOr<ChangePasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.UsernameDoesNotExist, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsIncorrect_ShouldReturnError()
    {
        // Arrange
        UserEntity user = UserEntityFixture.CreateUserEntity();
        string incorrectPassword = "wrongPassword";

        ChangePasswordCommand command = new(
            user.Username,
            incorrectPassword,
            "newPass",
            "newPass");

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(incorrectPassword, Uri.UnescapeDataString(user.Password))
            .Returns(false);

        // Act
        ErrorOr<ChangePasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.InvalidCurrentPassword, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>());
        _mockHashService.Received(1).CheckStringAgainstHash(incorrectPassword, Uri.UnescapeDataString(user.Password));
        await _mockUserRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ShouldReturnError()
    {
        // Arrange
        string currentPassword = "currentPassword";
        string newPassword = "newPassword";
        string escapedHashedCurrentPassword = Uri.EscapeDataString("hashedCurrentPassword");
        Error error = Error.Failure("Database.Error", "Failed to update user");

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = escapedHashedCurrentPassword;

        ChangePasswordCommand command = new(
            user.Username,
            currentPassword,
            newPassword,
            newPassword);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(currentPassword, "hashedCurrentPassword")
            .Returns(true);
        _mockUserRepository.UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<ChangePasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>());
        _mockHashService.Received(1).CheckStringAgainstHash(currentPassword, "hashedCurrentPassword");
        await _mockUserRepository.Received(1).UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetByUsernameReturnsError_ShouldReturnError()
    {
        // Arrange
        string currentPassword = "currentPassword";
        string newPassword = "newPassword";
        UserEntity user = UserEntityFixture.CreateUserEntity();
        ChangePasswordCommand command = new(
            user.Username,
            currentPassword,
            newPassword,
            newPassword);
        Error error = Error.Failure("Database.Error", "Failed to check username");

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<ChangePasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
