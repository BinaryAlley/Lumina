#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.Plugins.Queries.GetPlugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetPlugins;

/// <summary>
/// Contains unit tests for the <see cref="GetPluginsQueryHandler"/> class.
/// </summary>
public class GetPluginsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly GetPluginsQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsQueryHandlerTests"/> class.
    /// </summary>
    public GetPluginsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.GetRepository<IPluginRepository>().Returns(_mockPluginRepository);
        _sut = new GetPluginsQueryHandler(_mockUnitOfWork);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnAllDetectedPlugins()
    {
        // Arrange
        List<PluginEntity> plugins = [CreatePluginEntity(), CreatePluginEntity()];
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(plugins);

        // Act
        ErrorOr<IReadOnlyList<PluginResponse>> result = await _sut.Handle(new GetPluginsQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(plugins[0].Name, result.Value[0].Name);
        Assert.Equal(plugins[0].Id, result.Value[0].Id);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsError_ShouldReturnError()
    {
        // Arrange
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get plugins"));

        // Act
        ErrorOr<IReadOnlyList<PluginResponse>> result = await _sut.Handle(new GetPluginsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
    }

    private static PluginEntity CreatePluginEntity()
    {
        return new PluginEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Plugin",
            Author = "Lumina",
            Version = "1.0.0",
            Description = "A test plugin.",
            LoadStatus = PluginLoadStatus.Loaded,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = default,
            UpdatedOnUtc = DateTime.UtcNow,
            UpdatedBy = default
        };
    }
}
