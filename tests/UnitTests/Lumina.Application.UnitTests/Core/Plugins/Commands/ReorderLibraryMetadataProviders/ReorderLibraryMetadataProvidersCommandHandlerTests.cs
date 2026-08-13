#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
using Lumina.DataAccess.UnitTests.Core.Repositories.Plugins.Fixtures;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.ReorderLibraryMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryMetadataProvidersCommandHandler"/> class.
/// </summary>
public class ReorderLibraryMetadataProvidersCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockConfigurationRepository;
    private readonly ReorderLibraryMetadataProvidersCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryMetadataProvidersCommandHandlerTests"/> class.
    /// </summary>
    public ReorderLibraryMetadataProvidersCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockUnitOfWork.GetRepository<ILibraryMetadataProviderConfigurationRepository>().Returns(_mockConfigurationRepository);
        IValidator<ReorderLibraryMetadataProvidersCommand> mockValidator = Substitute.For<IValidator<ReorderLibraryMetadataProvidersCommand>>();
        mockValidator.Validate(Arg.Any<ReorderLibraryMetadataProvidersCommand>())
            .Returns([]);
        _sut = new ReorderLibraryMetadataProvidersCommandHandler(_mockUnitOfWork, mockValidator);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldAssignRanksInTheProvidedOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntityFixture configurationFixture = new();
        LibraryMetadataProviderConfigurationEntity firstProvider = configurationFixture.CreateConfiguration(libraryId, Guid.NewGuid(), 2);
        LibraryMetadataProviderConfigurationEntity secondProvider = configurationFixture.CreateConfiguration(libraryId, Guid.NewGuid(), 1);
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryMetadataProviderConfigurationEntity> { firstProvider, secondProvider });
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        ErrorOr<Success> result = await _sut.HandleAsync(new ReorderLibraryMetadataProvidersCommand(libraryId, [secondProvider.PluginId, firstProvider.PluginId]), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == secondProvider.PluginId && configuration.Rank == 1),
            Arg.Any<CancellationToken>());
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.PluginId == firstProvider.PluginId && configuration.Rank == 2),
            Arg.Any<CancellationToken>());
    }
}
