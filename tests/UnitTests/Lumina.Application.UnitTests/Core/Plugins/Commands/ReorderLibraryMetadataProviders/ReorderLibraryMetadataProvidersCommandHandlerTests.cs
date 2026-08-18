#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.ReorderLibraryMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryMetadataProvidersCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockConfigurationRepository;
    private readonly IValidator<ReorderLibraryMetadataProvidersCommand> _mockValidator;
    private readonly ReorderLibraryMetadataProvidersCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryMetadataProvidersCommandHandlerTests"/> class.
    /// </summary>
    public ReorderLibraryMetadataProvidersCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockConfigurationRepository);
        _mockValidator = Substitute.For<IValidator<ReorderLibraryMetadataProvidersCommand>>();
        _mockValidator.Validate(Arg.Any<ReorderLibraryMetadataProvidersCommand>())
            .Returns([]);
        _sut = new ReorderLibraryMetadataProvidersCommandHandler(_mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldAssignRanksInTheProvidedOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntityFixture configurationFixture = new();
        LibraryMetadataProviderConfigurationEntity firstProvider = configurationFixture.Create(libraryId, Guid.NewGuid(), 2);
        LibraryMetadataProviderConfigurationEntity secondProvider = configurationFixture.Create(libraryId, Guid.NewGuid(), 1);
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryMetadataProviderConfigurationEntity> { firstProvider, secondProvider });
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.HandleAsync(new ReorderLibraryMetadataProvidersCommand(libraryId, [secondProvider.PluginId, firstProvider.PluginId]), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == secondProvider.PluginId && configuration.Rank == 1),
            Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == firstProvider.PluginId && configuration.Rank == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutPersisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockValidator.Validate(Arg.Any<ReorderLibraryMetadataProvidersCommand>()).Returns([Errors.Plugins.PluginIdsListCannotBeEmpty]);

        // Act
        Result<Success> result = await _sut.HandleAsync(new ReorderLibraryMetadataProvidersCommand(libraryId, [Guid.NewGuid()]), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginIdsListCannotBeEmpty, result.FirstError);
        await _mockConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.DidNotReceive().UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
