#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.Seed;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Application.UnitTests.Core.Maintenance.ApplicationSetup.Commands.SetupApplication.Fixtures;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Domain.Common.Primitives;
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
    private readonly IPasswordHashService _mockHashService;
    private readonly ICryptographyService _mockCryptographyService;
    private readonly ITotpTokenGenerator _mockTotpTokenGenerator;
    private readonly IQRCodeGenerator _mockQRCodeGenerator;
    private readonly IUserRepository _mockUserRepository;
    private readonly IDateTimeProvider _mockDateTimeProvider;
    private readonly IDataSeedService _mockDataSeedService;
    private readonly SetupApplicationCommandHandler _sut;
    private readonly SetupApplicationCommandFixture _setupApplicationCommandFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationCommandHandlerTests"/> class.
    /// </summary>
    public SetupApplicationCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockHashService = Substitute.For<IPasswordHashService>();
        _mockCryptographyService = Substitute.For<ICryptographyService>();
        _mockTotpTokenGenerator = Substitute.For<ITotpTokenGenerator>();
        _mockQRCodeGenerator = Substitute.For<IQRCodeGenerator>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();
        _mockDataSeedService = Substitute.For<IDataSeedService>();
        _setupApplicationCommandFixture = new SetupApplicationCommandFixture();
        IValidator<SetupApplicationCommand> mockValidator = Substitute.For<IValidator<SetupApplicationCommand>>();
        mockValidator.Validate(Arg.Any<SetupApplicationCommand>())
            .Returns([]);

        _mockUnitOfWork.UserRepository.Returns(_mockUserRepository);
        _mockDateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        _sut = new SetupApplicationCommandHandler(
            _mockUnitOfWork,
            _mockHashService,
            _mockCryptographyService,
            _mockTotpTokenGenerator,
            _mockQRCodeGenerator,
            _mockDateTimeProvider,
            _mockDataSeedService,
            mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenNoExistingUsers_ShouldCreateAdminUserWithout2FA()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand() with { Use2fa = false };
        string hashedPassword = Uri.EscapeDataString("hashedPassword");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<UserEntity>()));
        _mockHashService.HashString(command.Password!)
            .Returns(hashedPassword);
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetAdminRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetAdminRoleToAdministratorAccount(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(command.Username, result.Value.Username);
        Assert.Null(result.Value.TotpSecret);

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoExistingUsersAndWith2FA_ShouldCreateAdminUserWithTOTP()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand() with { Use2fa = true };
        string hashedPassword = Uri.EscapeDataString("hashedPassword");
        byte[] totpSecret = [1, 2, 3];
        string qrCodeUri = "data:image/png;base64,test";
        string encryptedSecret = "encryptedSecret";

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<UserEntity>()));
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
        _mockDataSeedService.SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetAdminRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetAdminRoleToAdministratorAccount(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(command.Username, result.Value.Username);
        Assert.Equal(qrCodeUri, result.Value.TotpSecret);

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserAlreadyExists_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        UserEntity existingUser = UserEntityFixture.CreateUserEntity();

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { existingUser });

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.AdminAccountAlreadyCreated, result.FirstError);

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllUsersReturnsError_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        Error error = Error.Failure("Database.Error", "Failed to retrieve users");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.DidNotReceive().InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertUserReturnsError_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        Error error = Error.Failure("Database.Error", "Failed to insert user");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<UserEntity>()));
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);

        await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await _mockUserRepository.Received(1).InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSetDefaultAuthorizationPermissionsFails_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        Error error = Error.Failure("Database.Error", "Failed to set default permissions");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<UserEntity>()));
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);

        await _mockDataSeedService.Received(1).SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.DidNotReceive().SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSetDefaultAuthorizationRolesFails_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        Error error = Error.Failure("Database.Error", "Failed to set default roles");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<UserEntity>()));
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);

        await _mockDataSeedService.Received(1).SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.Received(1).SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.DidNotReceive().SetAdminRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSetAdminRolePermissionsFails_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        Error error = Error.Failure("Database.Error", "Failed to set admin role permissions");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<UserEntity>()));
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetAdminRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);

        await _mockDataSeedService.Received(1).SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.Received(1).SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.Received(1).SetAdminRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.DidNotReceive().SetAdminRoleToAdministratorAccount(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSetAdminRoleToAdministratorAccountFails_ShouldReturnError()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();
        Error error = Error.Failure("Database.Error", "Failed to set admin role to administrator account");

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<UserEntity>()));
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetAdminRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetAdminRoleToAdministratorAccount(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);

        await _mockDataSeedService.Received(1).SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.Received(1).SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.Received(1).SetAdminRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.Received(1).SetAdminRoleToAdministratorAccount(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAllOperationsSucceed_ShouldReturnSuccessResponse()
    {
        // Arrange
        SetupApplicationCommand command = _setupApplicationCommandFixture.CreateSetupApplicationCommand();

        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<UserEntity>()));
        _mockUserRepository.InsertAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetAdminRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockDataSeedService.SetAdminRoleToAdministratorAccount(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        Result<RegistrationResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(command.Username, result.Value.Username);

        await _mockDataSeedService.Received(1).SetDefaultAuthorizationPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.Received(1).SetDefaultAuthorizationRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.Received(1).SetAdminRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockDataSeedService.Received(1).SetAdminRoleToAdministratorAccount(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
