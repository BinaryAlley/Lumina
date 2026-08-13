#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryMetadataProviderEnabledCommandHandler"/> class.
/// </summary>
public class SetLibraryMetadataProviderEnabledCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockConfigurationRepository;
    private readonly SetLibraryMetadataProviderEnabledCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryMetadataProviderEnabledCommandHandlerTests"/> class.
    /// </summary>
    public SetLibraryMetadataProviderEnabledCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockUnitOfWork.GetRepository<ILibraryMetadataProviderConfigurationRepository>().Returns(_mockConfigurationRepository);
        IValidator<SetLibraryMetadataProviderEnabledCommand> mockValidator = Substitute.For<IValidator<SetLibraryMetadataProviderEnabledCommand>>();
        mockValidator.Validate(Arg.Any<SetLibraryMetadataProviderEnabledCommand>())
            .Returns([]);
        _sut = new SetLibraryMetadataProviderEnabledCommandHandler(_mockUnitOfWork, mockValidator);
    }

    [Fact]
    public async Task Handle_WhenConfigurationDoesNotExist_ShouldCreateEnabledConfigurationWithNextRank()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(libraryId, pluginId, Arg.Any<CancellationToken>())
            .Returns((LibraryMetadataProviderConfigurationEntity?)null);
        _mockConfigurationRepository.GetByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(new List<LibraryMetadataProviderConfigurationEntity>());
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        ErrorOr<Success> result = await _sut.HandleAsync(new SetLibraryMetadataProviderEnabledCommand(libraryId, pluginId, true), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.IsEnabled && configuration.Rank == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConfigurationExists_ShouldUpdateItsEnabledState()
    {
        // Arrange
        LibraryMetadataProviderConfigurationEntity existingConfiguration = new()
        {
            Id = Guid.NewGuid(),
            LibraryId = Guid.NewGuid(),
            PluginId = Guid.NewGuid(),
            IsEnabled = false,
            Rank = 2,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = default,
            UpdatedBy = default
        };
        _mockConfigurationRepository.GetByLibraryAndPluginIdAsync(existingConfiguration.LibraryId, existingConfiguration.PluginId, Arg.Any<CancellationToken>())
            .Returns(existingConfiguration);
        _mockConfigurationRepository.UpsertAsync(Arg.Any<LibraryMetadataProviderConfigurationEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        ErrorOr<Success> result = await _sut.HandleAsync(new SetLibraryMetadataProviderEnabledCommand(existingConfiguration.LibraryId, existingConfiguration.PluginId, true), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        await _mockConfigurationRepository.Received(1).UpsertAsync(
            Arg.Is<LibraryMetadataProviderConfigurationEntity>(configuration => configuration.IsEnabled && configuration.Rank == 2),
            Arg.Any<CancellationToken>());
    }
}
