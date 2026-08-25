#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.Plugins.Queries.GetPlugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetPlugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetPlugins;

/// <summary>
/// Contains unit tests for the <see cref="GetPluginsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly GetPluginsQueryHandler _sut;
    private readonly GetPluginsQueryFixture _getPluginsQueryFixture = new();
    private readonly PluginEntityFixture _pluginEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsQueryHandlerTests"/> class.
    /// </summary>
    public GetPluginsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);
        _sut = new GetPluginsQueryHandler(_mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldReturnAllDetectedPlugins()
    {
        // Arrange
        List<PluginEntity> plugins = [_pluginEntityFixture.Create(), _pluginEntityFixture.Create()];
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(plugins);

        // Act
        Result<IReadOnlyList<PluginResponse>> result = await _sut.HandleAsync(_getPluginsQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(plugins[0].Name, result.Value[0].Name);
        Assert.Equal(plugins[0].Id, result.Value[0].Id);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnError()
    {
        // Arrange
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get plugins"));

        // Act
        Result<IReadOnlyList<PluginResponse>> result = await _sut.HandleAsync(_getPluginsQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }
}
