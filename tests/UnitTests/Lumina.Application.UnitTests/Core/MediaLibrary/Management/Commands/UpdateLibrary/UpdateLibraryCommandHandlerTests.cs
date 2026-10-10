#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    private readonly ILibraryPathTemplateService _mockLibraryPathTemplateService;
    private readonly IValidator<UpdateLibraryCommand> _mockValidator;
    private readonly UpdateLibraryCommandHandler _sut;
    private readonly UpdateLibraryCommandFixture _updateLibraryCommandFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryPathPartFixture _libraryPathPartFixture = new();
    private readonly LibraryPathTemplateFixture _libraryPathTemplateFixture = new();
    private readonly LibraryPathTemplatePartDtoFixture _libraryPathTemplatePartDtoFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryCommandHandlerTests"/> class.
    /// </summary>
    public UpdateLibraryCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockProviderConfigurationStore = Substitute.For<IMediaLibraryProviderConfigurationStore>();
        _mockLibraryPathTemplateService = Substitute.For<ILibraryPathTemplateService>();
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
        _mockLibraryPathTemplateService.ResolveTemplate(Arg.Any<LibraryType>(), Arg.Any<IReadOnlyList<LibraryPathPart>>()).Returns(Result.From(LibraryPathTemplate.Empty()));

        _sut = new UpdateLibraryCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockEnvironmentContext, _mockProviderConfigurationStore, _mockLibraryPathTemplateService, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValid_ShouldUpdateLibraryAndReturnResponse()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryType commandLibraryType = Enum.Parse<LibraryType>(command.LibraryType);
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        existingLibrary.LibraryType = commandLibraryType == LibraryType.Book ? LibraryType.EBook : LibraryType.Book;
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.None));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.CoverFileMustBeAnImage, result.FirstError);
        await _mockLibraryRepository.Received(1).GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>());
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
        _mockLibraryRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to determine image type"));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockLibraryRepository.Received(1).GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>());
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
        _mockLibraryRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.FileSystemManagement.InvalidPath, result.FirstError);
        await _mockLibraryRepository.Received(1).GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCoverImageIsUnchanged_ShouldSkipImageTypeCheck()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        existingLibrary.CoverImage = command.CoverImage;
        _mockLibraryRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockEnvironmentContext.FileTypeService.DidNotReceive()
            .GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.Received(2).GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        _mockLibraryRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>())
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
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryCreationFails_ShouldReturnError()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create();
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(command.Id, cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
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
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPathTemplatePartsIsNull_ShouldResolveTemplateWithoutParts()
    {
        // Arrange
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create(pathTemplateParts: null);
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        LibraryPathPart defaultPart = _libraryPathPartFixture.Create(
            kind: LibraryPathPartKind.Artist,
            representation: "{0}",
            isOptional: true);
        LibraryPathTemplate defaultTemplate = _libraryPathTemplateFixture.Create(parts: [defaultPart]);
        _mockLibraryPathTemplateService.ResolveTemplate(Arg.Any<LibraryType>(), Arg.Any<IReadOnlyList<LibraryPathPart>>()).Returns(Result.From(defaultTemplate));
        LibraryType expectedLibraryType = Enum.Parse<LibraryType>(command.LibraryType!);

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockLibraryPathTemplateService.Received(1).ResolveTemplate(
            expectedLibraryType,
            Arg.Is<IReadOnlyList<LibraryPathPart>>(parts => parts.Count == 0));
        await _mockLibraryRepository.Received(1).UpdateAsync(
            Arg.Is<LibraryEntity>(library =>
                library.PathTemplateParts.Count == 1 &&
                library.PathTemplateParts.First().Kind == LibraryPathPartKind.Artist &&
                library.PathTemplateParts.First().Representation == "{0}" &&
                library.PathTemplateParts.First().IsOptional),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPathTemplatePartsAreProvided_ShouldBuildTemplateFromProvidedParts()
    {
        // Arrange
        LibraryPathTemplatePartDto providedDto = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Artist),
            representation: "{0}",
            isOptional: false);
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create(pathTemplateParts: [providedDto]);
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        LibraryPathPart providedPart = _libraryPathPartFixture.Create(
            kind: LibraryPathPartKind.Artist,
            representation: "{0}",
            isOptional: false);
        LibraryPathTemplate resolvedTemplate = _libraryPathTemplateFixture.Create(parts: [providedPart]);
        _mockLibraryPathTemplateService.ResolveTemplate(Arg.Any<LibraryType>(), Arg.Any<IReadOnlyList<LibraryPathPart>>()).Returns(Result.From(resolvedTemplate));
        LibraryType expectedLibraryType = Enum.Parse<LibraryType>(command.LibraryType!);

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockLibraryPathTemplateService.Received(1).ResolveTemplate(
            expectedLibraryType,
            Arg.Is<IReadOnlyList<LibraryPathPart>>(parts =>
                parts.Count == 1 &&
                parts[0].Kind == LibraryPathPartKind.Artist &&
                parts[0].Representation == "{0}" &&
                !parts[0].IsOptional));
        await _mockLibraryRepository.Received(1).UpdateAsync(
            Arg.Is<LibraryEntity>(library =>
                library.PathTemplateParts.Count == 1 &&
                library.PathTemplateParts.First().Kind == LibraryPathPartKind.Artist &&
                library.PathTemplateParts.First().Representation == "{0}" &&
                !library.PathTemplateParts.First().IsOptional),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPathTemplateMappingFails_ShouldReturnErrorWithoutPersisting()
    {
        // Arrange
        LibraryPathTemplatePartDto invalidPart = _libraryPathTemplatePartDtoFixture.Create() with { Kind = "NotARealLibraryPathPartKind" };
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create(pathTemplateParts: [invalidPart]);
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.PathTemplatePartKindNotSupportedForLibraryType, result.FirstError);
        _mockLibraryPathTemplateService.DidNotReceive().ResolveTemplate(Arg.Any<LibraryType>(), Arg.Any<IReadOnlyList<LibraryPathPart>>());
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPathTemplateValidationFails_ShouldReturnErrorWithoutPersisting()
    {
        // Arrange
        LibraryPathTemplatePartDto validPart = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Artist),
            representation: "{0}");
        UpdateLibraryCommand command = _updateLibraryCommandFixture.Create(pathTemplateParts: [validPart]);
        LibraryEntity existingLibrary = _libraryEntityFixture.Create(id: command.Id, userId: command.OwnerId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(existingLibrary));
        Error validationError = DomainErrors.Library.PathTemplateIsNotSupportedForLibraryType;
        _mockLibraryPathTemplateService.ResolveTemplate(Arg.Any<LibraryType>(), Arg.Any<IReadOnlyList<LibraryPathPart>>()).Returns(validationError);

        // Act
        Result<LibraryResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(validationError, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
