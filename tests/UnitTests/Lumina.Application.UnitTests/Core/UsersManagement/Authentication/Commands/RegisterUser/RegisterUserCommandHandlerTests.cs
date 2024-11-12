#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RegisterUser.Fixture;
using Lumina.Contracts.Responses.Authentication;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RegisterUser;

/// <summary>
/// Contains unit tests for the <see cref="RegisterUserCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterUserCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IHashService _mockHashService;
    private readonly ICryptographyService _mockCryptographyService;
    private readonly ITotpTokenGenerator _mockTotpTokenGenerator;
    private readonly IQRCodeGenerator _mockQRCodeGenerator;
    private readonly IUserRepository _mockUserRepository;
    private readonly RegisterUserCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandHandlerTests"/> class.
    /// </summary>
    public RegisterUserCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockHashService = Substitute.For<IHashService>();
        _mockCryptographyService = Substitute.For<ICryptographyService>();
        _mockTotpTokenGenerator = Substitute.For<ITotpTokenGenerator>();
        _mockQRCodeGenerator = Substitute.For<IQRCodeGenerator>();
        _mockUserRepository = Substitute.For<IUserRepository>();

        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);

        _sut = new RegisterUserCommandHandler(
            _mockUnitOfWork,
            _mockHashService,
            _mockCryptographyService,
            _mockTotpTokenGenerator,
            _mockQRCodeGenerator);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExistAndWith2FA_ShouldRegisterUser()
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        byte[] totpSecret = [1, 2, 3];
        string qrCodeUri = "data:image/png;base64,test";
        string encryptedSecret = "encryptedSecret";
        string hashedPassword = "hashedPassword";

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(null));
        _mockHashService.HashString(command.Password!)
            .Returns(hashedPassword);
        _mockTotpTokenGenerator.GenerateSecret()
            .Returns(totpSecret);
        _mockQRCodeGenerator.GenerateQrCodeDataUri(command.Username!, totpSecret)
            .Returns(qrCodeUri);
        _mockCryptographyService.Encrypt(Convert.ToBase64String(totpSecret))
            .Returns(encryptedSecret);
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Username.Should().Be(command.Username);
        result.Value.TotpSecret.Should().Be(qrCodeUri);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        _mockHashService.Received(1).HashString(command.Password!);
        _mockTotpTokenGenerator.Received(1).GenerateSecret();
        _mockQRCodeGenerator.Received(1).GenerateQrCodeDataUri(command.Username!, totpSecret);
        _mockCryptographyService.Received(1).Encrypt(Convert.ToBase64String(totpSecret));
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExistAndWithout2FA_ShouldRegisterUser()
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand() with { Use2fa = false };
        string hashedPassword = "hashedPassword";

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(null));
        _mockHashService.HashString(command.Password!)
            .Returns(hashedPassword);
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Username.Should().Be(command.Username);
        result.Value.TotpSecret.Should().BeNull();

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        _mockHashService.Received(1).HashString(command.Password!);
        _mockTotpTokenGenerator.DidNotReceive().GenerateSecret();
        _mockQRCodeGenerator.DidNotReceive().GenerateQrCodeDataUri(Arg.Any<string>(), Arg.Any<byte[]>());
        _mockCryptographyService.DidNotReceive().Encrypt(Arg.Any<string>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldReturnError()
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        UserEntity existingUser = UserEntityFixture.CreateUserEntity();

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(existingUser));

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authentication.UsernameAlreadyExists);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ShouldReturnError()
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        Error error = Error.Failure("Database.Error", "Failed to insert user");

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(null));
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetByUsernameReturnsError_ShouldReturnError()
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        Error error = Error.Failure("Database.Error", "Failed to check username");

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
