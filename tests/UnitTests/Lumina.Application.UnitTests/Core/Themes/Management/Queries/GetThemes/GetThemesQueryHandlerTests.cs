#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.Themes.Management.Queries.GetThemes;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Queries.GetThemes;

/// <summary>
/// Contains unit tests for the <see cref="GetThemesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemesQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly GetThemesQueryHandler _sut;
    private readonly GetThemesQueryFixture _getThemesQueryFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesQueryHandlerTests"/> class.
    /// </summary>
    public GetThemesQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);
        _sut = new GetThemesQueryHandler(_mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldReturnAllThemesOrderedAvailableBundledFirstThenByNameWithDeletedLast()
    {
        // Arrange
        GetThemesQuery query = _getThemesQueryFixture.Create();
        ThemeEntity uploadedTheme = _themeEntityFixture.Create(themeId: "user-theme", name: "Zeta", installSource: ThemeInstallSource.Uploaded, isDeleted: false);
        ThemeEntity bundledLowerTheme = _themeEntityFixture.Create(themeId: "bundled-lower", name: "apple", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        ThemeEntity bundledUpperTheme = _themeEntityFixture.Create(themeId: "bundled-upper", name: "Banana", installSource: ThemeInstallSource.Bundled, isDeleted: false);
        ThemeEntity deletedTheme = _themeEntityFixture.Create(themeId: "deleted-theme", name: "Deleted", installSource: ThemeInstallSource.Uploaded, isDeleted: true);
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([uploadedTheme, bundledLowerTheme, bundledUpperTheme, deletedTheme]));

        // Act
        Result<IReadOnlyList<ThemeResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4, result.Value.Count);
        Assert.Equal(bundledLowerTheme.Id, result.Value[0].Id);
        Assert.Equal(bundledUpperTheme.Id, result.Value[1].Id);
        Assert.Equal(uploadedTheme.Id, result.Value[2].Id);
        Assert.Equal(deletedTheme.Id, result.Value[3].Id);
        Assert.Equal(ThemeInstallSource.Bundled, result.Value[0].InstallSource);
        Assert.Equal(ThemeInstallSource.Bundled, result.Value[1].InstallSource);
        Assert.Equal(ThemeInstallSource.Uploaded, result.Value[2].InstallSource);
        Assert.True(result.Value[3].IsDeleted);
    }

    [Fact]
    public async Task HandleAsync_WhenNoThemesExist_ShouldReturnEmptyList()
    {
        // Arrange
        GetThemesQuery query = _getThemesQueryFixture.Create();
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ThemeEntity>>([]));

        // Act
        Result<IReadOnlyList<ThemeResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnError()
    {
        // Arrange
        GetThemesQuery query = _getThemesQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get all themes");
        _mockThemeRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IReadOnlyList<ThemeResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
