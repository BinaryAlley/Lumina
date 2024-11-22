#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.Repositories.Users;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Users.Fixtures;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly UserRepository _sut;
    private readonly UserEntityFixture _userEntityFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepositoryTests"/> class.
    /// </summary>
    public UserRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new UserRepository(_mockContext);
        _userEntityFixture = new UserEntityFixture();
    }

    [Fact]
    public async Task InsertAsync_WhenUserDoesNotExist_ShouldAddUserToContextAndReturnCreated()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(userModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        EntityEntry<UserEntity>? addedUser = _mockContext.ChangeTracker.Entries<UserEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == userModel.Id);
        addedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertAsync_WhenUserAlreadyExists_ShouldReturnError()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();

        _mockContext.Users.Add(userModel);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(userModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Users.UserAlreadyExists);
        _mockContext.ChangeTracker.Entries<UserEntity>().Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllUsers()
    {
        // Arrange
        List<UserEntity> users =
        [
            _userEntityFixture.CreateUserModel(),
            _userEntityFixture.CreateUserModel(),
            _userEntityFixture.CreateUserModel()
        ];
        _mockContext.Users.AddRange(users);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<IEnumerable<UserEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoUsersExist_ShouldReturnEmptyList()
    {
        // Act
        ErrorOr<IEnumerable<UserEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();
        _mockContext.Users.Add(userModel);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<UserEntity?> result = await _sut.GetByUsernameAsync(userModel.Username, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(userModel);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        ErrorOr<UserEntity?> result = await _sut.GetByUsernameAsync("nonexistent", CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_ShouldUpdateUserAndReturnUpdated()
    {
        // Arrange
        UserEntity existingUser = _userEntityFixture.CreateUserModel();
        _mockContext.Users.Add(existingUser);
        await _mockContext.SaveChangesAsync();

        // Create updated user with same Id but different properties
        UserEntity updatedUser = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username, // Keep username as it's used for lookup
            Password = "NewPassword123",
            TempPassword = "TempPass456",
            TotpSecret = "NewSecret",
            Libraries = [],
            UserRoles = [],
            UserPermissions = [],
            CreatedBy = existingUser.Id,
            CreatedOnUtc = existingUser.CreatedOnUtc,
            UpdatedOnUtc = DateTime.UtcNow
        };

        // Act
        ErrorOr<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);

        UserEntity? modifiedUser = await _mockContext.Users.FirstOrDefaultAsync(u => u.Username == existingUser.Username);
        modifiedUser.Should().NotBeNull();
        modifiedUser!.Password.Should().Be(updatedUser.Password);
        modifiedUser.TempPassword.Should().Be(updatedUser.TempPassword);
        modifiedUser.TotpSecret.Should().Be(updatedUser.TotpSecret);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_ShouldReturnError()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();

        // Act
        ErrorOr<Updated> result = await _sut.UpdateAsync(userModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Users.UserDoesNotExist);
    }
}
