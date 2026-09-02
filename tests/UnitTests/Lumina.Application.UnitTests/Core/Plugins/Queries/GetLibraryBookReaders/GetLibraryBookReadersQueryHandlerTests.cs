#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryBookReaders;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetLibraryBookReaders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryBookReadersQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryBookReadersQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryBookReaderConfigurationRepository _mockLibraryBookReaderConfigurationRepository;
    private readonly IPluginRepository _mockPluginRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IBookReaderRegistry _mockBookReaderRegistry;
    private readonly IValidator<GetLibraryBookReadersQuery> _mockValidator;
    private readonly GetLibraryBookReadersQueryHandler _sut;
    private readonly GetLibraryBookReadersQueryFixture _getLibraryBookReadersQueryFixture = new();
    private readonly LibraryBookReaderConfigurationEntityFixture _configurationEntityFixture = new();
    private readonly PluginEntityFixture _pluginEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryBookReadersQueryHandlerTests"/> class.
    /// </summary>
    public GetLibraryBookReadersQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryBookReaderConfigurationRepository = Substitute.For<ILibraryBookReaderConfigurationRepository>();
        _mockPluginRepository = Substitute.For<IPluginRepository>();
        _mockUnitOfWork.LibraryBookReaderConfigurationRepository.Returns(_mockLibraryBookReaderConfigurationRepository);
        _mockUnitOfWork.PluginRepository.Returns(_mockPluginRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockBookReaderRegistry = Substitute.For<IBookReaderRegistry>();
        _mockValidator = Substitute.For<IValidator<GetLibraryBookReadersQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated and the library ownership policy allows access.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetLibraryBookReadersQuery>()).Returns([]);
        _mockBookReaderRegistry.GetSupportedExtensionsByPluginId().Returns(new Dictionary<Guid, IReadOnlyList<string>>());

        _sut = new GetLibraryBookReadersQueryHandler(_mockAuthorizationService, _mockCurrentUserService, _mockBookReaderRegistry, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIdIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetLibraryBookReadersQuery>()).Returns([Errors.Plugins.LibraryIdCannotBeEmpty]);

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockLibraryBookReaderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenConfigurationsExist_ShouldReturnEnabledReadersFirstOrderedByNameWithPluginNames()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create();
        Guid enabledPluginId = Guid.NewGuid();
        Guid disabledPluginId = Guid.NewGuid();
        List<LibraryBookReaderConfigurationEntity> configurations =
        [
            _configurationEntityFixture.Create(libraryId: query.LibraryId, pluginId: enabledPluginId, isEnabled: true),
            _configurationEntityFixture.Create(libraryId: query.LibraryId, pluginId: disabledPluginId, isEnabled: false)
        ];
        _mockLibraryBookReaderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>(configurations));
        List<PluginEntity> plugins =
        [
            _pluginEntityFixture.Create(enabledPluginId, "A Reader"),
            _pluginEntityFixture.Create(disabledPluginId, "B Reader")
        ];
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(plugins);
        Dictionary<Guid, IReadOnlyList<string>> supportedExtensions = new()
        {
            [enabledPluginId] = [".epub"],
            [disabledPluginId] = [".pdf"]
        };
        _mockBookReaderRegistry.GetSupportedExtensionsByPluginId().Returns(supportedExtensions);

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(enabledPluginId, result.Value[0].PluginId);
        Assert.True(result.Value[0].IsEnabled);
        Assert.Equal(".epub", Assert.Single(result.Value[0].SupportedExtensions));
        Assert.Equal(disabledPluginId, result.Value[1].PluginId);
        Assert.False(result.Value[1].IsEnabled);
        Assert.Equal(".pdf", Assert.Single(result.Value[1].SupportedExtensions));
    }

    [Fact]
    public async Task HandleAsync_WhenPluginIsMissingFromDetectedPlugins_ShouldUseEmptyNameAndNoExtensions()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create();
        Guid unknownPluginId = Guid.NewGuid();
        List<LibraryBookReaderConfigurationEntity> configurations =
        [
            _configurationEntityFixture.Create(libraryId: query.LibraryId, pluginId: unknownPluginId, isEnabled: true)
        ];
        _mockLibraryBookReaderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>(configurations));
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<PluginEntity>>([_pluginEntityFixture.Create()]));

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryBookReaderResponse response = Assert.Single(result.Value);
        Assert.Equal(unknownPluginId, response.PluginId);
        Assert.Equal(string.Empty, response.Name);
        Assert.Empty(response.SupportedExtensions);
    }

    [Fact]
    public async Task HandleAsync_WhenNoConfigurationsExist_ShouldReturnEmptyList()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create();
        _mockLibraryBookReaderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<PluginEntity>>([]));

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenGetConfigurationsFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create();
        Error error = Error.Failure(description: "Failed to get configurations");
        _mockLibraryBookReaderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockPluginRepository.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetPluginsFails_ShouldReturnError()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create();
        _mockLibraryBookReaderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryBookReaderConfigurationEntity>>([]));
        _mockPluginRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get plugins"));

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task HandleAsync_WhenOwnershipPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryBookReaderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockPluginRepository.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<LibraryBookReaderResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockLibraryBookReaderConfigurationRepository.DidNotReceive().GetByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
