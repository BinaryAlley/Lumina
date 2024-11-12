#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Application.UnitTests.Core.Maintenance.ApplicationSetup.Commands.SetupApplication.Fixtures;
using Lumina.Contracts.Responses.Authentication;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;

/// <summary>
/// Contains unit tests for the <see cref="SetupApplicationCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetupApplicationCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IHashService _mockHashService;
    private readonly ICryptographyService _mockCryptographyService;
    private readonly ITotpTokenGenerator _mockTotpTokenGenerator;
    private readonly IQRCodeGenerator _mockQRCodeGenerator;
    private readonly IUserRepository _mockUserRepository;
    private readonly SetupApplicationCommandHandler _sut;
    private readonly SetupApplicationCommandFixture _setupApplicationCommandFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationCommandHandlerTests"/> class.
    /// </summary>
    public SetupApplicationCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockHashService = Substitute.For<IHashService>();
        _mockCryptographyService = Substitute.For<ICryptographyService>();
        _mockTotpTokenGenerator = Substitute.For<ITotpTokenGenerator>();
        _mockQRCodeGenerator = Substitute.For<IQRCodeGenerator>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _setupApplicationCommandFixture = new SetupApplicationCommandFixture();

        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);

        _sut = new SetupApplicationCommandHandler(
            _mockUnitOfWork,
            _mockHashService,
            _mockCryptographyService,
            _mockTotpTokenGenerator,
            _mockQRCodeGenerator);
    }

    [Fact]
    public async Task Handle_WhenNoExistingUsers_ShouldCreateAdminUserWithout2FA()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand() with { Use2fa = false };
        string hashedPassword = Uri.EscapeDataString("hashedPassword");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Enumerable.Empty<UserEntity>()));
        _mockHashService.HashString(command.Password!)
            .Returns(hashedPassword);
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be(command.Username);
        result.Value.TotpSecret.Should().BeNull();

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoExistingUsersAndWith2FA_ShouldCreateAdminUserWithTOTP()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand() with { Use2fa = true };
        string hashedPassword = Uri.EscapeDataString("hashedPassword");
        byte[] totpSecret = [1, 2, 3];
        string qrCodeUri = "data:image/png;base64,test";
        string encryptedSecret = "encryptedSecret";

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Enumerable.Empty<UserEntity>()));
        _mockHashService.HashString(command.Password!)
            .Returns(hashedPassword);
        _mockTotpTokenGenerator.GenerateSecret()
            .Returns(totpSecret);
        _mockQRCodeGenerator.GenerateQrCodeDataUri(command.Username!, totpSecret)
            .Returns(qrCodeUri);
        _mockCryptographyService.Encrypt(Arg.Any<string>())
            .Returns(encryptedSecret);
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be(command.Username);
        result.Value.TotpSecret.Should().Be(qrCodeUri);

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        UserEntity existingUser = UserEntityFixture.CreateUserEntity();

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { existingUser });

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.AdminAccountAlreadyCreated);

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetAllUsersReturnsError_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        Error error = Error.Failure("Database.Error", "Failed to retrieve users");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInsertUserReturnsError_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        Error error = Error.Failure("Database.Error", "Failed to insert user");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Enumerable.Empty<UserEntity>()));
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RegistrationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
