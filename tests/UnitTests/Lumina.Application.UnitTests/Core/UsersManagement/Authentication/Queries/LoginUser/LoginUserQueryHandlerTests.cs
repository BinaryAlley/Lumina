#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Contracts.Responses.Authentication;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.LoginUser;

/// <summary>
/// Contains unit tests for the <see cref="LoginUserQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginUserQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IHashService _mockHashService;
    private readonly IJwtTokenGenerator _mockJwtTokenGenerator;
    private readonly ITotpTokenGenerator _mockTotpTokenGenerator;
    private readonly ICryptographyService _mockCryptographyService;
    private readonly IUserRepository _mockUserRepository;
    private readonly IDateTimeProvider _mockDateTimeProvider;
    private readonly LoginUserQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUserQueryHandlerTests"/> class.
    /// </summary>
    public LoginUserQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockHashService = Substitute.For<IHashService>();
        _mockJwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _mockTotpTokenGenerator = Substitute.For<ITotpTokenGenerator>();
        _mockCryptographyService = Substitute.For<ICryptographyService>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();

        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);
        _mockDateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        _sut = new LoginUserQueryHandler(
            _mockUnitOfWork,
            _mockHashService,
            _mockJwtTokenGenerator,
            _mockTotpTokenGenerator,
            _mockCryptographyService,
            _mockDateTimeProvider);
    }

    [Fact]
    public async Task Handle_WhenValidCredentialsWithoutTOTP_ShouldReturnLoginResponse()
    {
        // Arrange
        string password = "password123";
        string hashedPassword = "hashedPassword";
        string jwtToken = "jwtToken";

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = Uri.EscapeDataString(hashedPassword);
        user.TotpSecret = null;

        LoginUserQuery query = new(user.Username, password);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(password, hashedPassword)
            .Returns(true);
        _mockJwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Username)
            .Returns(jwtToken);

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(user.Id, result.Value.Id);
        Assert.Equal(user.Username, result.Value.Username);
        Assert.Equal(jwtToken, result.Value.Token);
        Assert.False(result.Value.UsesTotp);
    }

    [Fact]
    public async Task Handle_WhenValidCredentialsWithTOTP_ShouldReturnLoginResponse()
    {
        // Arrange
        string password = "password123";
        string hashedPassword = "hashedPassword";
        string jwtToken = "jwtToken";
        string totpCode = "123456";
        string encryptedTotpSecret = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        string decryptedTotpSecret = Convert.ToBase64String(new byte[] { 4, 5, 6 });

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = Uri.EscapeDataString(hashedPassword);
        user.TotpSecret = encryptedTotpSecret;

        LoginUserQuery query = new(user.Username, password, totpCode);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(password, hashedPassword)
            .Returns(true);
        _mockJwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Username)
            .Returns(jwtToken);
        _mockCryptographyService.Decrypt(encryptedTotpSecret)
            .Returns(decryptedTotpSecret);
        _mockTotpTokenGenerator.ValidateToken(Arg.Any<byte[]>(), totpCode)
            .Returns(true);

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(user.Id, result.Value.Id);
        Assert.Equal(user.Username, result.Value.Username);
        Assert.Equal(jwtToken, result.Value.Token);
        Assert.True(result.Value.UsesTotp);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnError()
    {
        // Arrange
        LoginUserQuery query = new("nonexistentUser", "password");

        _mockUserRepository.GetByUsernameAsync(query.Username!, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(null));

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.InvalidUsernameOrPassword, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ShouldReturnError()
    {
        // Arrange
        string password = "wrongPassword";
        string hashedPassword = "hashedPassword";

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = Uri.EscapeDataString(hashedPassword);
        user.TempPassword = null;

        LoginUserQuery query = new(user.Username, password);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(password, hashedPassword)
            .Returns(false);

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.InvalidUsernameOrPassword, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenTempPasswordIsExpired_ShouldReturnError()
    {
        // Arrange
        string password = "tempPassword";
        string hashedPassword = "hashedPassword";
        string hashedTempPassword = "hashedTempPassword";

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = Uri.EscapeDataString(hashedPassword);
        user.TempPassword = Uri.EscapeDataString(hashedTempPassword);
        user.TempPasswordCreated = DateTime.UtcNow.AddMinutes(-16); // Expired (more than 15 minutes old)

        LoginUserQuery query = new(user.Username, password);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(password, hashedPassword)
            .Returns(false);
        _mockHashService.CheckStringAgainstHash(password, hashedTempPassword)
            .Returns(true);

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.TempPasswordExpired, result.FirstError);

        await _mockUserRepository.Received(1).UpdateAsync(Arg.Is<UserEntity>(u =>
            u.TempPassword == null && u.TempPasswordCreated == null),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValidTempPassword_ShouldReturnLoginResponse()
    {
        // Arrange
        string password = "tempPassword";
        string hashedPassword = "hashedPassword";
        string hashedTempPassword = "hashedTempPassword";
        string jwtToken = "jwtToken";

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = Uri.EscapeDataString(hashedPassword);
        user.TempPassword = Uri.EscapeDataString(hashedTempPassword);
        // Set temp password created time to 10 minutes ago (still valid)
        user.TempPasswordCreated = DateTime.UtcNow.AddMinutes(-10);
        user.TotpSecret = null; // Ensure no TOTP validation is needed

        LoginUserQuery query = new(user.Username, password);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(password, hashedPassword)
            .Returns(false); // Regular password check fails
        _mockHashService.CheckStringAgainstHash(password, hashedTempPassword)
            .Returns(true); // Temp password check succeeds
        _mockJwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Username)
            .Returns(jwtToken);

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(user.Id, result.Value.Id);
        Assert.Equal(user.Username, result.Value.Username);
        Assert.Equal(jwtToken, result.Value.Token);
        Assert.False(result.Value.UsesTotp);
    }

    [Fact]
    public async Task Handle_WhenUserHasTOTPButNoCodeProvided_ShouldReturnError()
    {
        // Arrange
        string password = "password123";
        string hashedPassword = "hashedPassword";
        string encryptedTotpSecret = Convert.ToBase64String(new byte[] { 1, 2, 3 });

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = Uri.EscapeDataString(hashedPassword);
        user.TotpSecret = encryptedTotpSecret;

        LoginUserQuery query = new(user.Username, password);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(password, hashedPassword)
            .Returns(true);

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.InvalidTotpCode, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenTOTPCodeIsInvalid_ShouldReturnError()
    {
        // Arrange
        string password = "password123";
        string hashedPassword = "hashedPassword";
        string totpCode = "123456";
        string encryptedTotpSecret = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        string decryptedTotpSecret = Convert.ToBase64String(new byte[] { 4, 5, 6 });

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = Uri.EscapeDataString(hashedPassword);
        user.TotpSecret = encryptedTotpSecret;

        LoginUserQuery query = new(user.Username, password, totpCode);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(password, hashedPassword)
            .Returns(true);
        _mockCryptographyService.Decrypt(encryptedTotpSecret)
            .Returns(decryptedTotpSecret);
        _mockTotpTokenGenerator.ValidateToken(Arg.Any<byte[]>(), totpCode)
            .Returns(false);

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.InvalidTotpCode, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenGetByUsernameReturnsError_ShouldReturnError()
    {
        // Arrange
        LoginUserQuery query = new("username", "password");
        Error error = Error.Failure("Database.Error", "Failed to retrieve user");

        _mockUserRepository.GetByUsernameAsync(query.Username!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenBothRegularAndTempPasswordsAreIncorrect_ShouldReturnError()
    {
        // Arrange
        string password = "wrongPassword";
        string hashedPassword = "hashedPassword";
        string hashedTempPassword = "hashedTempPassword";

        UserEntity user = UserEntityFixture.CreateUserEntity();
        user.Password = Uri.EscapeDataString(hashedPassword);
        user.TempPassword = Uri.EscapeDataString(hashedTempPassword);
        user.TempPasswordCreated = DateTime.UtcNow.AddMinutes(-10); // Valid timeframe

        LoginUserQuery query = new(user.Username, password);

        _mockUserRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));
        _mockHashService.CheckStringAgainstHash(password, hashedPassword)
            .Returns(false); // Regular password check fails
        _mockHashService.CheckStringAgainstHash(password, hashedTempPassword)
            .Returns(false); // Temp password check also fails

        // Act
        ErrorOr<LoginResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.InvalidUsernameOrPassword, result.FirstError);

        _mockHashService.Received(1).CheckStringAgainstHash(password, hashedPassword);
        _mockHashService.Received(1).CheckStringAgainstHash(password, hashedTempPassword);
        await _mockUserRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
