#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.Authorization.Policies.LibraryOwnership;
using Lumina.Infrastructure.Fixtures.Core.Authorization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authorization.Policies.LibraryOwnership;

/// <summary>
/// Contains unit tests for the <see cref="LibraryOwnershipPolicy"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryOwnershipPolicyTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IUserRepository _mockUserRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly LibraryOwnershipPolicy _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryOwnershipPolicyTests"/> class.
    /// </summary>
    public LibraryOwnershipPolicyTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.UserRepository.Returns(_mockUserRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);

        _sut = new LibraryOwnershipPolicy(_mockUnitOfWork);
    }

    [Fact]
    public async Task EvaluateAsync_WhenContextIsNotLibraryOwnershipContext_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        bool result = await _sut.EvaluateAsync(userId, null, CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockUserRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        LibraryOwnershipPolicyContext context = new(Guid.NewGuid());
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(null));

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenGetUserFails_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        LibraryOwnershipPolicyContext context = new(Guid.NewGuid());
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to retrieve user"));

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenUserIsAdmin_ShouldReturnTrue()
    {
        // Arrange
        UserEntity adminUser = AuthorizationServiceFixture.CreateUserWithPermissions(
            rolePermissions: new Dictionary<string, IEnumerable<AuthorizationPermission>>
            {
                { "Admin", [] }
            });
        LibraryOwnershipPolicyContext context = new(Guid.NewGuid());
        _mockUserRepository.GetByIdAsync(adminUser.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(adminUser));

        // Act
        bool result = await _sut.EvaluateAsync(adminUser.Id, context, CancellationToken.None);

        // Assert
        Assert.True(result);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenLibraryDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserEntity regularUser = AuthorizationServiceFixture.CreateUserWithPermissions();
        LibraryOwnershipPolicyContext context = new(Guid.NewGuid());
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(regularUser));
        _mockLibraryRepository.GetByIdAsync(context.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result<LibraryEntity?>.Success(null));

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenGetLibraryFails_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserEntity regularUser = AuthorizationServiceFixture.CreateUserWithPermissions();
        LibraryOwnershipPolicyContext context = new(Guid.NewGuid());
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(regularUser));
        _mockLibraryRepository.GetByIdAsync(context.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to retrieve library"));

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenUserOwnsLibrary_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserEntity regularUser = AuthorizationServiceFixture.CreateUserWithPermissions();
        LibraryEntity library = CreateLibrary(userId);
        LibraryOwnershipPolicyContext context = new(library.Id);
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(regularUser));
        _mockLibraryRepository.GetByIdAsync(context.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenUserDoesNotOwnLibrary_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserEntity regularUser = AuthorizationServiceFixture.CreateUserWithPermissions();
        LibraryEntity library = CreateLibrary(Guid.NewGuid());
        LibraryOwnershipPolicyContext context = new(library.Id);
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<UserEntity?>(regularUser));
        _mockLibraryRepository.GetByIdAsync(context.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Creates a library entity owned by <paramref name="ownerId"/>.
    /// </summary>
    /// <param name="ownerId">The Id of the user owning the library.</param>
    /// <returns>The created library entity.</returns>
    private static LibraryEntity CreateLibrary(Guid ownerId)
    {
        return new LibraryEntity
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Test Library",
            LibraryType = LibraryType.Book,
            ContentLocations = [],
            CreatedBy = ownerId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedBy = null
        };
    }
}
