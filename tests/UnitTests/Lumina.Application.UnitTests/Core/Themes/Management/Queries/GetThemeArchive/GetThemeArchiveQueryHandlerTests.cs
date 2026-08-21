#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeArchive;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeArchive;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Queries.GetThemeArchive;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeArchiveQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeArchiveQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IThemeService _mockThemeService;
    private readonly IValidator<GetThemeArchiveQuery> _mockValidator;
    private readonly IThemeRepository _mockThemeRepository;
    private readonly GetThemeArchiveQueryHandler _sut;
    private readonly GetThemeArchiveQueryFixture _getThemeArchiveQueryFixture = new();
    private readonly ThemeEntityFixture _themeEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeArchiveQueryHandlerTests"/> class.
    /// </summary>
    public GetThemeArchiveQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockThemeService = Substitute.For<IThemeService>();
        _mockValidator = Substitute.For<IValidator<GetThemeArchiveQuery>>();
        _mockThemeRepository = Substitute.For<IThemeRepository>();

        _mockValidator.Validate(Arg.Any<GetThemeArchiveQuery>())
            .Returns([]);
        _mockUnitOfWork.ThemeRepository.Returns(_mockThemeRepository);

        _sut = new GetThemeArchiveQueryHandler(_mockUnitOfWork, _mockThemeService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutAnyDownstreamCalls()
    {
        // Arrange
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetThemeArchiveQuery>())
            .Returns([Errors.Themes.ThemeIdCannotBeEmpty]);

        // Act
        Result<ThemeArchiveResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeIdCannotBeEmpty, result.FirstError);
        await _mockThemeRepository.DidNotReceive().GetByThemeIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockThemeService.DidNotReceive().BuildArchiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetThemeFails_ShouldReturnError()
    {
        // Arrange
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get theme");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeArchiveResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockThemeService.DidNotReceive().BuildArchiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeDoesNotExist_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create();
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(null));

        // Act
        Result<ThemeArchiveResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        await _mockThemeService.DidNotReceive().BuildArchiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThemeIsDeleted_ShouldReturnThemeNotFoundError()
    {
        // Arrange
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, isDeleted: true);
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));

        // Act
        Result<ThemeArchiveResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        await _mockThemeService.DidNotReceive().BuildArchiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArchiveBuildsSuccessfully_ShouldReturnArchiveResponse()
    {
        // Arrange
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, isDeleted: false);
        byte[] bytes = Encoding.UTF8.GetBytes("archive-content");
        string fileName = $"{theme.ThemeId}.zip";
        string contentType = "application/zip";
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.BuildArchiveAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(Result.From(new ThemeArchiveDto(bytes, fileName, contentType)));

        // Act
        Result<ThemeArchiveResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bytes, result.Value.Bytes);
        Assert.Equal(fileName, result.Value.FileName);
        Assert.Equal(contentType, result.Value.ContentType);
    }

    [Fact]
    public async Task HandleAsync_WhenArchiveBuildFails_ShouldReturnError()
    {
        // Arrange
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create();
        ThemeEntity theme = _themeEntityFixture.Create(themeId: query.ThemeId, isDeleted: false);
        Error error = Error.Failure("Theme.ArchiveBuildFailed", "Failed to build archive");
        _mockThemeRepository.GetByThemeIdAsync(query.ThemeId!, Arg.Any<CancellationToken>())
            .Returns(Result.From<ThemeEntity?>(theme));
        _mockThemeService.BuildArchiveAsync(theme.ThemeId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThemeArchiveResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
