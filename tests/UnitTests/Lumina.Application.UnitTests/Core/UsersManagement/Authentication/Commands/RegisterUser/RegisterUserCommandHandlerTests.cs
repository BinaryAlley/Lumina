#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Domain.Common.Primitives;
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
    private readonly IPasswordHashService _mockHashService;
    private readonly ICryptographyService _mockCryptographyService;
    private readonly ITotpTokenGenerator _mockTotpTokenGenerator;
    private readonly IQRCodeGenerator _mockQRCodeGenerator;
    private readonly IUserRepository _mockUserRepository;
    private readonly IDateTimeProvider _mockDateTimeProvider;
    private readonly RegisterUserCommandHandler _sut;
    private readonly RegisterUserCommandFixture _registerUserCommandFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandHandlerTests"/> class.
    /// </summary>
    public RegisterUserCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockHashService = Substitute.For<IPasswordHashService>();
        _mockCryptographyService = Substitute.For<ICryptographyService>();
        _mockTotpTokenGenerator = Substitute.For<ITotpTokenGenerator>();
        _mockQRCodeGenerator = Substitute.For<IQRCodeGenerator>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();

        _mockUnitOfWork.UserRepository.Returns(_mockUserRepository);
        _mockDateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        IValidator<RegisterUserCommand> mockValidator = Substitute.For<IValidator<RegisterUserCommand>>();
        mockValidator.Validate(Arg.Any<RegisterUserCommand>())
            .Returns([]);
        _sut = new RegisterUserCommandHandler(
            _mockUnitOfWork,
            _mockHashService,
            _mockCryptographyService,
            _mockTotpTokenGenerator,
            _mockQRCodeGenerator,
            _mockDateTimeProvider,
            mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExistAndWith2FA_ShouldRegisterUser()
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        byte[] totpSecret = [1, 2, 3];
        string qrCodeUri = "data:image/png;base64,test";
        string encryptedSecret = "encryptedSecret";
        string hashedPassword = "hashedPassword";

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(null));
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
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(command.Username, result.Value.Username);
        Assert.Equal(qrCodeUri, result.Value.TotpSecret);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        _mockHashService.Received(1).HashString(command.Password!);
        _mockTotpTokenGenerator.Received(1).GenerateSecret();
        _mockQRCodeGenerator.Received(1).GenerateQrCodeDataUri(command.Username!, totpSecret);
        _mockCryptographyService.Received(1).Encrypt(Convert.ToBase64String(totpSecret));
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExistAndWithout2FA_ShouldRegisterUser()
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create() with { Use2fa = false };
        string hashedPassword = "hashedPassword";

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(null));
        _mockHashService.HashString(command.Password!)
            .Returns(hashedPassword);
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(command.Username, result.Value.Username);
        Assert.Null(result.Value.TotpSecret);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        _mockHashService.Received(1).HashString(command.Password!);
        _mockTotpTokenGenerator.DidNotReceive().GenerateSecret();
        _mockQRCodeGenerator.DidNotReceive().GenerateQrCodeDataUri(Arg.Any<string>(), Arg.Any<byte[]>());
        _mockCryptographyService.DidNotReceive().Encrypt(Arg.Any<string>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserAlreadyExists_ShouldReturnError()
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        UserEntity existingUser = _userEntityFixture.Create();

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(existingUser));

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authentication.UsernameAlreadyExists, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertFails_ShouldReturnError()
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to insert user");

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(null));
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByUsernameReturnsError_ShouldReturnError()
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to check username");

        _mockUserRepository.GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);

        await _mockUserRepository.Received(1).GetByUsernameAsync(command.Username!, Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
