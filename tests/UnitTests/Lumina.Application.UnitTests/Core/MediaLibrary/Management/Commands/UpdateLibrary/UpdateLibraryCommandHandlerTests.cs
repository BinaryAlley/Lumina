#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Commands.UpdateLibrary;

/// <summary>
/// Contains unit tests for the <see cref="UpdateLibraryCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateLibraryCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly IMediaLibraryProviderConfigurationStore _mockProviderConfigurationStore;
    private readonly IValidator<UpdateLibraryCommand> _mockValidator;
    private readonly UpdateLibraryCommandHandler _sut;
    private readonly UpdateLibraryCommandFixture _updateLibraryCommandFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryCommandHandlerTests"/> class.
    /// </summary>
    public UpdateLibraryCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockProviderConfigurationStore = Substitute.For<IMediaLibraryProviderConfigurationStore>();
        _mockValidator = Substitute.For<IValidator<UpdateLibraryCommand>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated, has the manage permission, the cover image is a valid image, and the provider configurations are reconciled successfully
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.HasPermissionAsync(_userId, Arg.Any<AuthorizationPermission>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.PNG));
        _mockLibraryRepository.UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockProviderConfigurationStore.ReconcileProviderConfigurationsAsync(Arg.Any<Guid>(), Arg.Any<LibraryType>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);
        _mockValidator.Validate(Arg.Any<UpdateLibraryCommand>()).Returns([]);

        _sut = new UpdateLibraryCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockEnvironmentContext, _mockProviderConfigurationStore, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValid_ShouldUpdateLibraryAndReturnResponse()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryType commandLibraryType = Enum.Parse<LibraryType>(command.LibraryType);
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        existingLibrary.LibraryType = commandLibraryType == LibraryType.Book ? LibraryType.EBook : LibraryType.Book;
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(existingLibrary.Id, result.Value.Id);
        await _mockLibraryRepository.Received(1).UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockProviderConfigurationStore.Received(1).ReconcileProviderConfigurationsAsync(
            command.Id, Arg.Is<LibraryType>(libraryType => libraryType == commandLibraryType), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Any<LibrarySavedDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryTypeIsUnchanged_ShouldNotReconcileProviderConfigurations()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        existingLibrary.LibraryType = Enum.Parse<LibraryType>(command.LibraryType);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockProviderConfigurationStore.DidNotReceive().ReconcileProviderConfigurationsAsync(Arg.Any<Guid>(), Arg.Any<LibraryType>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenProviderConfigurationReconciliationFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryType commandLibraryType = Enum.Parse<LibraryType>(command.LibraryType);
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        existingLibrary.LibraryType = commandLibraryType == LibraryType.Book ? LibraryType.EBook : LibraryType.Book;
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        _mockProviderConfigurationStore.ReconcileProviderConfigurationsAsync(Arg.Any<Guid>(), Arg.Any<LibraryType>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to reconcile provider configurations"));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCoverImageIsNull_ShouldSkipImageTypeCheck()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        command = command with { CoverImage = null };
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockEnvironmentContext.FileTypeService.DidNotReceive()
            .GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCoverImageIsNotAnImage_ShouldReturnCoverFileMustBeAnImageError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        command = command with { CoverImage = "C:/Users/user/cover.jpg" };
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        existingLibrary.CoverImage = null;
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.None));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.CoverFileMustBeAnImage, result.FirstError);
        await _mockLibraryRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenImageTypeCheckFails_ShouldReturnError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        command = command with { CoverImage = "C:/Users/user/cover.jpg" };
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        existingLibrary.CoverImage = null;
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to determine image type"));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockLibraryRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCoverImagePathIsInvalid_ShouldReturnInvalidPathError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        command = command with { CoverImage = " " };
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        existingLibrary.CoverImage = null;
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.FileSystemManagement.InvalidPath, result.FirstError);
        await _mockLibraryRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCoverImageIsUnchanged_ShouldSkipImageTypeCheck()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        existingLibrary.CoverImage = command.CoverImage;
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockEnvironmentContext.FileTypeService.DidNotReceive()
            .GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.Received(2).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result<LibraryEntity?>.Success(null));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdFails_ShouldReturnError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get library"));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserLacksPermissionAndPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        _mockAuthorizationService.HasPermissionAsync(_userId, Arg.Any<AuthorizationPermission>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryCreationFails_ShouldReturnError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        command = command with { ContentLocations = [""] };

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.FileSystemManagement.InvalidPath, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        _mockLibraryRepository.UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to update library"));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetCreatedLibraryFails_ShouldReturnError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary), Error.Failure(description: "Failed to retrieve updated library"));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task HandleAsync_WhenCreatedLibraryCannotBeRetrieved_ShouldReturnPersistenceError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary), Result<LibraryEntity?>.Success(null));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.ErrorPersistingMediaLibrary, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdateLibraryCommand>()).Returns([DomainErrors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
