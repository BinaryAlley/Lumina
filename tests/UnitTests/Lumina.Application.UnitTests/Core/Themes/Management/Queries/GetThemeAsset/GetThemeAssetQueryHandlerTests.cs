#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeAsset;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeAsset;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Queries.GetThemeAsset;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeAssetQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeAssetQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeService _mockThemeService;
    private readonly IValidator<GetThemeAssetQuery> _mockValidator;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly GetThemeAssetQueryHandler _sut;
    private readonly GetThemeAssetQueryFixture _getThemeAssetQueryFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();
    private readonly ThemeAssetDtoFixture _themeAssetDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeAssetQueryHandlerTests"/> class.
    /// </summary>
    public GetThemeAssetQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeService = Substitute.For<IThemeService>();
        _mockValidator = Substitute.For<IValidator<GetThemeAssetQuery>>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();

        _mockValidator.Validate(Arg.Any<GetThemeAssetQuery>())
            .Returns([]);
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);

        _sut = new GetThemeAssetQueryHandler(_mockUnitOfWork, _mockThemeService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutAnyDownstreamCalls()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetThemeAssetQuery>())
            .Returns([Errors.Themes.ThemeIdCannotBeEmpty]);

        // Act
        Result<ThemeAssetResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeIdCannotBeEmpty, result.FirstError);
        await _mockThemeRepository.DidNotReceive().GetByThemeIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockThemeService.DidNotReceive().GetAssetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetThemeFails_ShouldReturnError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get theme");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeAssetResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockThemeService.DidNotReceive().GetAssetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeDoesNotExist_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));

        // Act
        Result<ThemeAssetResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenThemeIsDeleted_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, isDeleted: true);
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));

        // Act
        Result<ThemeAssetResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        await _mockThemeService.DidNotReceive().GetAssetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAssetLoadsSuccessfully_ShouldReturnAssetResponse()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, isDeleted: false);
        byte[] bytes = Encoding.UTF8.GetBytes("asset-content");
        string contentType = "image/png";
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetAssetAsync(theme.ThemeId, query.AssetPath!, Arg.Any<CancellationToken>())
            .Returns(Result.From(_themeAssetDtoFixture.Create(bytes: bytes, contentType: contentType)));

        // Act
        Result<ThemeAssetResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bytes, result.Value.Bytes);
        Assert.Equal(contentType, result.Value.ContentType);
        await _mockThemeService.DidNotReceive().RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAssetLoadFailsForUserTheme_ShouldReturnAssetErrorWithoutRestoring()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Uploaded, isDeleted: false);
        Error error = Error.Failure("Theme.AssetMissing", "Asset missing");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetAssetAsync(theme.ThemeId, query.AssetPath!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeAssetResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockThemeService.DidNotReceive().RestoreBundledThemeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAssetLoadFailsAndRestoreFails_ShouldReturnRestoreError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        Error assetError = Error.Failure("Theme.AssetMissing", "Asset missing");
        Error restoreError = Error.Failure("Theme.RestoreFailed", "Restore failed");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetAssetAsync(theme.ThemeId, query.AssetPath!, Arg.Any<CancellationToken>())
            .Returns(assetError);
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(restoreError);

        // Act
        Result<ThemeAssetResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(restoreError, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenAssetLoadFailsAndRestoreSucceeds_ShouldRetryAndReturnAssetResponse()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        Error assetError = Error.Failure("Theme.AssetMissing", "Asset missing");
        byte[] restoredBytes = Encoding.UTF8.GetBytes("restored-asset");
        string contentType = "text/css";
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetAssetAsync(theme.ThemeId, query.AssetPath!, Arg.Any<CancellationToken>())
            .Returns(assetError, Result.From(_themeAssetDtoFixture.Create(bytes: restoredBytes, contentType: contentType)));
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        Result<ThemeAssetResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(restoredBytes, result.Value.Bytes);
        Assert.Equal(contentType, result.Value.ContentType);
        await _mockThemeService.Received(2).GetAssetAsync(theme.ThemeId, query.AssetPath!, Arg.Any<CancellationToken>());
        await _mockThemeService.Received(1).RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRestoreSucceedsButRetryFails_ShouldReturnAssetError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, installSource: ThemeInstallSource.Bundled, isDeleted: false);
        Error assetError = Error.Failure("Theme.AssetMissing", "Asset missing");
        Error retryError = Error.Failure("Theme.AssetMissingAgain", "Asset still missing");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.GetAssetAsync(theme.ThemeId, query.AssetPath!, Arg.Any<CancellationToken>())
            .Returns(assetError, retryError);
        _mockThemeService.RestoreBundledThemeAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        Result<ThemeAssetResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(retryError, result.FirstError);
    }
}
