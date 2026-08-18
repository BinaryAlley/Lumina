#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryMetadataProviders;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetLibraryMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryMetadataProvidersQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryMetadataProvidersQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockLibraryMetadataProviderConfigurationRepository;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly GetLibraryMetadataProvidersQueryHandler _sut;
    private readonly GetLibraryMetadataProvidersQueryFixture _getLibraryMetadataProvidersQueryFixture = new();
    private readonly LibraryMetadataProviderConfigurationEntityFixture _configurationEntityFixture = new();
    private readonly PluginEntityFixture _pluginEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryMetadataProvidersQueryHandlerTests"/> class.
    /// </summary>
    public GetLibraryMetadataProvidersQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryMetadataProviderConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockLibraryMetadataProviderConfigurationRepository);
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);

        _sut = new GetLibraryMetadataProvidersQueryHandler(_mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationsExist_ShouldReturnProvidersOrderedByRankWithPluginNames()
    {
        // Arrange
        GetLibraryMetadataProvidersQuery query = _getLibraryMetadataProvidersQueryFixture.Create();
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        List<LibraryMetadataProviderConfigurationEntity> configurations =
        [
            _configurationEntityFixture.Create(query.LibraryId, secondPluginId, 2),
            _configurationEntityFixture.Create(query.LibraryId, firstPluginId, 1)
        ];
        _mockLibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>(configurations));
        List<PluginEntity> plugins =
        [
            _pluginEntityFixture.Create(firstPluginId),
            _pluginEntityFixture.Create(secondPluginId)
        ];
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(plugins);

        // Act
        Result<IReadOnlyList<LibraryMetadataProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(firstPluginId, result.Value[0].PluginId);
        Assert.Equal(plugins[0].Name, result.Value[0].Name);
        Assert.Equal(configurations[1].Rank, result.Value[0].Rank);
        Assert.Equal(secondPluginId, result.Value[1].PluginId);
        Assert.Equal(plugins[1].Name, result.Value[1].Name);
    }

    [Fact]
    public async Task HandleAsync_WhenPluginIsMissingFromDetectedPlugins_ShouldUseEmptyName()
    {
        // Arrange
        GetLibraryMetadataProvidersQuery query = _getLibraryMetadataProvidersQueryFixture.Create();
        Guid unknownPluginId = Guid.NewGuid();
        List<LibraryMetadataProviderConfigurationEntity> configurations =
        [
            _configurationEntityFixture.Create(query.LibraryId, unknownPluginId, 1)
        ];
        _mockLibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>(configurations));
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<PluginEntity>>([_pluginEntityFixture.Create()]));

        // Act
        Result<IReadOnlyList<LibraryMetadataProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryMetadataProviderResponse response = Assert.Single(result.Value);
        Assert.Equal(unknownPluginId, response.PluginId);
        Assert.Equal(string.Empty, response.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenNoConfigurationsExist_ShouldReturnEmptyList()
    {
        // Arrange
        GetLibraryMetadataProvidersQuery query = _getLibraryMetadataProvidersQueryFixture.Create();
        _mockLibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<PluginEntity>>([]));

        // Act
        Result<IReadOnlyList<LibraryMetadataProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenGetConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryMetadataProvidersQuery query = _getLibraryMetadataProvidersQueryFixture.Create();
        Error error = Error.Failure(description: "Failed to get configurations");
        _mockLibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IReadOnlyList<LibraryMetadataProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockPluginRepository.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetPluginsFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryMetadataProvidersQuery query = _getLibraryMetadataProvidersQueryFixture.Create();
        _mockLibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get plugins"));

        // Act
        Result<IReadOnlyList<LibraryMetadataProviderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }
}
