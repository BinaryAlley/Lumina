#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Contracts.Responses.Authentication;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RecoverPassword;

/// <summary>
/// Contains unit tests for the <see cref="RecoverPasswordCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPasswordHashService _mockHashService;
    private readonly ITotpTokenGenerator _mockTotpTokenGenerator;
    private readonly ICryptographyService _mockCryptographyService;
    private readonly IUserRepository _mockUserRepository;
    private readonly RecoverPasswordCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckInitializationQueryHandlerTests"/> class.
    /// </summary>
    public RecoverPasswordCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockHashService = Substitute.For<IPasswordHashService>();
        _mockTotpTokenGenerator = Substitute.For<ITotpTokenGenerator>();
        _mockCryptographyService = Substitute.For<ICryptographyService>();
        _mockUserRepository = Substitute.For<IUserRepository>();

        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);

        _sut = new RecoverPasswordCommandHandler(
            _mockUnitOfWork,
            _mockHashService,
            _mockTotpTokenGenerator,
            _mockCryptographyService);
    }

    [Fact]
    public async Task Handle_WhenUserExistsAndTotpIsValid_ShouldResetPassword()
    {
        // Arrange
        string totpCode = "123456";
        string encryptedTotpSecret = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        string decryptedTotpSecret = Convert.ToBase64String(new byte[] { 4, 5, 6 });
        string hashedTempPassword = "hashedTempPassword";

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.TotpSecret = encryptedTotpSecret;
        user.TempPassword = null;

        RecoverPasswordCommand command = new(user.Username, totpCode);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockCryptographyService.Decrypt(encryptedTotpSecret)
            .Returns(decryptedTotpSecret);
        _mockTotpTokenGenerator.ValidateToken(Arg.Any<byte[]>(), totpCode)
            .Returns(true);
        _mockHashService.HashString("Abcd123$")
            .Returns(hashedTempPassword);
        _mockUserRepository.UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        ErrorOr<RecoverPasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value.IsPasswordReset);

        await _mockUserRepository.Received(1).GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>());
        _mockTotpTokenGenerator.Received(1).ValidateToken(Arg.Any<byte[]>(), totpCode);
        _mockHashService.Received(1).HashString("Abcd123$");
        await _mockUserRepository.Received(1).UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnError()
    {
        // Arrange
        RecoverPasswordCommand command = new("nonexistentUser", "123456");

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(null));

        // Act
        ErrorOr<RecoverPasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.UsernameDoesNotExist, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTempPasswordExists_ShouldReturnError()
    {
        // Arrange
        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.TempPassword = "existingTempPassword";
        user.TempPasswordCreated = DateTime.UtcNow;

        RecoverPasswordCommand command = new(user.Username, "123456");

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        ErrorOr<RecoverPasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.PasswordResetAlreadyRequested, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotUseTOTP_ShouldReturnError()
    {
        // Arrange
        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.TotpSecret = null;

        RecoverPasswordCommand command = new(user.Username, "123456");

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        ErrorOr<RecoverPasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.InvalidTotpCode, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTotpCodeIsInvalid_ShouldReturnError()
    {
        // Arrange
        string totpCode = "123456";
        string encryptedTotpSecret = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        string decryptedTotpSecret = Convert.ToBase64String(new byte[] { 4, 5, 6 });

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.TotpSecret = encryptedTotpSecret;

        RecoverPasswordCommand command = new(user.Username, totpCode);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockCryptographyService.Decrypt(encryptedTotpSecret)
            .Returns(decryptedTotpSecret);
        _mockTotpTokenGenerator.ValidateToken(Arg.Any<byte[]>(), totpCode)
            .Returns(false);

        // Act
        ErrorOr<RecoverPasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.InvalidTotpCode, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>());
        _mockTotpTokenGenerator.Received(1).ValidateToken(Arg.Any<byte[]>(), totpCode);
        await _mockUserRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ShouldReturnError()
    {
        // Arrange
        string totpCode = "123456";
        string encryptedTotpSecret = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        string decryptedTotpSecret = Convert.ToBase64String(new byte[] { 4, 5, 6 });
        Error error = Error.Failure("Database.Error", "Failed to update user");

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.TotpSecret = encryptedTotpSecret;

        RecoverPasswordCommand command = new(user.Username, totpCode);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockCryptographyService.Decrypt(encryptedTotpSecret)
            .Returns(decryptedTotpSecret);
        _mockTotpTokenGenerator.ValidateToken(Arg.Any<byte[]>(), totpCode)
            .Returns(true);
        _mockUserRepository.UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RecoverPasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>());
        _mockTotpTokenGenerator.Received(1).ValidateToken(Arg.Any<byte[]>(), totpCode);
        await _mockUserRepository.Received(1).UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetByUsernameReturnsError_ShouldReturnError()
    {
        // Arrange
        string totpCode = "123456";
        string encryptedTotpSecret = Convert.ToBase64String(new byte[] { 1, 2, 3 });

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.TotpSecret = encryptedTotpSecret;

        RecoverPasswordCommand command = new(user.Username, totpCode);
        Error error = Error.Failure("Database.Error", "Failed to check username");

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RecoverPasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserUsesTOTPButNoCodeProvided_ShouldReturnError()
    {
        // Arrange
        string encryptedTotpSecret = Convert.ToBase64String(new byte[] { 1, 2, 3 });

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.TotpSecret = encryptedTotpSecret;

        RecoverPasswordCommand command = new(user.Username, null);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        ErrorOr<RecoverPasswordResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.InvalidTotpCode, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>());
        _mockTotpTokenGenerator.DidNotReceive().ValidateToken(Arg.Any<byte[]>(), Arg.Any<string>());
        await _mockUserRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
