#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Queries.GetPluginSettings;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetPluginSettings;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Fixtures.Common.Models.DTO.Settings;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetPluginSettings;

/// <summary>
/// Contains unit tests for the <see cref="GetPluginSettingsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly IPluginManager _mockPluginManager;
    private readonly IValidator<GetPluginSettingsQuery> _mockValidator;
    private readonly GetPluginSettingsQueryHandler _sut;
    private readonly GetPluginSettingsQueryFixture _getPluginSettingsQueryFixture = new();
    private readonly PluginEntityFixture _pluginEntityFixture = new();
    private readonly PluginSettingDescriptorDtoFixture _pluginSettingDescriptorDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsQueryHandlerTests"/> class.
    /// </summary>
    public GetPluginSettingsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);
        _mockPluginManager = Substitute.For<IPluginManager>();
        _mockValidator = Substitute.For<IValidator<GetPluginSettingsQuery>>();
        _mockValidator.Validate(Arg.Any<GetPluginSettingsQuery>()).Returns([]);

        _sut = new GetPluginSettingsQueryHandler(_mockUnitOfWork, _mockPluginManager, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenPluginIdIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        GetPluginSettingsQuery query = _getPluginSettingsQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetPluginSettingsQuery>()).Returns([DomainErrors.Plugins.PluginIdCannotBeEmpty]);

        // Act
        Result<PluginSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Plugins.PluginIdCannotBeEmpty, result.FirstError);
        await _mockPluginRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPluginIsLoaded_ShouldReturnSchemaAndSettings()
    {
        // Arrange
        GetPluginSettingsQuery query = _getPluginSettingsQueryFixture.Create();
        PluginEntity plugin = _pluginEntityFixture.Create(query.PluginId);
        _mockPluginRepository.GetByIdAsync(query.PluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<PluginEntity?>(plugin));

        IPlugin loadedPlugin = Substitute.For<IPlugin>();
        loadedPlugin.GetSettingsSchema().Returns(
        [
            _pluginSettingDescriptorDtoFixture.Create(key: "key1", label: "Label 1", type: PluginSettingType.Text, defaultValue: "default"),
            _pluginSettingDescriptorDtoFixture.Create(key: "key2", label: "Label 2", type: PluginSettingType.Select, allowedValues: ["a", "b"])
        ]);
        _mockPluginManager.GetPlugin(query.PluginId).Returns(loadedPlugin);
        IReadOnlyDictionary<string, string>? expectedSettings = JsonSerializer.Deserialize<Dictionary<string, string>>(plugin.SettingsJson!);

        // Act
        Result<PluginSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(query.PluginId, result.Value.PluginId);
        Assert.Equal(2, result.Value.Schema.Count);
        Assert.Equal("key1", result.Value.Schema[0].Key);
        Assert.Equal(PluginSettingType.Text, result.Value.Schema[0].Type);
        Assert.NotNull(result.Value.Settings);
        Assert.Equal(expectedSettings, result.Value.Settings);
    }

    [Fact]
    public async Task HandleAsync_WhenPluginIsNotLoaded_ShouldReturnEmptySchema()
    {
        // Arrange
        GetPluginSettingsQuery query = _getPluginSettingsQueryFixture.Create();
        PluginEntity plugin = _pluginEntityFixture.Create(query.PluginId);
        _mockPluginRepository.GetByIdAsync(query.PluginId, Arg.Any<CancellationToken>())
            .Returns(Result.From<PluginEntity?>(plugin));
        _mockPluginManager.GetPlugin(query.PluginId).Returns((IPlugin?)null);

        // Act
        Result<PluginSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(query.PluginId, result.Value.PluginId);
        Assert.Empty(result.Value.Schema);
    }

    [Fact]
    public async Task HandleAsync_WhenPluginDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        GetPluginSettingsQuery query = _getPluginSettingsQueryFixture.Create();
        _mockPluginRepository.GetByIdAsync(query.PluginId, Arg.Any<CancellationToken>())
            .Returns(Result<PluginEntity?>.Success(null));

        // Act
        Result<PluginSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Plugins.PluginNotFound, result.FirstError);
        _mockPluginManager.DidNotReceive().GetPlugin(Arg.Any<Guid>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdFails_ShouldReturnError()
    {
        // Arrange
        GetPluginSettingsQuery query = _getPluginSettingsQueryFixture.Create();
        Error error = Error.Failure(description: "Failed to get plugin");
        _mockPluginRepository.GetByIdAsync(query.PluginId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PluginSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockPluginManager.DidNotReceive().GetPlugin(Arg.Any<Guid>());
    }
}
