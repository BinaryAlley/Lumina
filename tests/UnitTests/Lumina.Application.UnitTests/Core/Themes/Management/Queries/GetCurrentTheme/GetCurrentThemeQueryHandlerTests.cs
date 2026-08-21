#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.Themes.Management.Queries.GetCurrentTheme;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetCurrentTheme;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Queries.GetCurrentTheme;

/// <summary>
/// Contains unit tests for the <see cref="GetCurrentThemeQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetCurrentThemeQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly GetCurrentThemeQueryHandler _sut;
    private readonly GetCurrentThemeQueryFixture _getCurrentThemeQueryFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentThemeQueryHandlerTests"/> class.
    /// </summary>
    public GetCurrentThemeQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);
        _sut = new GetCurrentThemeQueryHandler(_mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentThemeExists_ShouldReturnItsResponse()
    {
        // Arrange
        GetCurrentThemeQuery query = _getCurrentThemeQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(isCurrent: true, includeIsCurrent: true);
        _mockThemeRepository.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(theme.Id, result.Value.Id);
        Assert.Equal(theme.ThemeId, result.Value.ThemeId);
        Assert.Equal(theme.Name, result.Value.Name);
        Assert.Equal(theme.Description, result.Value.Description);
        Assert.Equal(theme.Author, result.Value.Author);
        Assert.Equal(theme.Version, result.Value.Version);
        Assert.Equal(theme.PreviewPath, result.Value.PreviewPath);
        Assert.Equal(theme.InstallSource, result.Value.InstallSource);
        Assert.True(result.Value.IsCurrent);
        Assert.Equal(theme.InstalledAtUtc, result.Value.InstalledAtUtc);
    }

    [Fact]
    public async Task HandleAsync_WhenNoCurrentThemeExists_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        GetCurrentThemeQuery query = _getCurrentThemeQueryFixture.Create();
        _mockThemeRepository.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnError()
    {
        // Arrange
        GetCurrentThemeQuery query = _getCurrentThemeQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get current theme");
        _mockThemeRepository.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
