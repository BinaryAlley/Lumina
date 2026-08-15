#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Application.Fixtures.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IDirectoryService _mockDirectoryService;
    private readonly GetDirectoriesQueryHandler _sut;
    private readonly DirectoryFixture _directoryFixture;
    private readonly GetDirectoriesQueryFixture _getDirectoriesQueryFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesQueryHandlerTests"/> class.
    /// </summary>
    public GetDirectoriesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockDirectoryService = Substitute.For<IDirectoryService>();
        IValidator<GetDirectoriesQuery> mockValidator = Substitute.For<IValidator<GetDirectoriesQuery>>();
        mockValidator.Validate(Arg.Any<GetDirectoriesQuery>())
            .Returns([]);
        _sut = new GetDirectoriesQueryHandler(_mockDirectoryService, mockValidator);
        _directoryFixture = new DirectoryFixture();
        _getDirectoriesQueryFixture = new GetDirectoriesQueryFixture();
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidQueryWithoutFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetDirectoriesQuery getDirectoriesQuery = _getDirectoriesQueryFixture.Create(includeHiddenElements: false);

        IEnumerable<Directory> directories = _directoryFixture.CreateMany();

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(Result.From(directories));

        // Act
        Result<IEnumerable<DirectoryResponse>> result = await _sut.HandleAsync(getDirectoriesQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.IsAssignableFrom<IEnumerable<DirectoryResponse>>(result.Value);
        Assert.Equal(directories.Count(), result.Value.Count());

        List<DirectoryResponse> resultList = [.. result.Value];
        List<Directory> directoriesList = [.. directories];

        for (int i = 0; i < resultList.Count; i++)
        {
            Assert.Equal(directoriesList[i].Id.Path, resultList[i].Path);
            Assert.Equal(directoriesList[i].Name, resultList[i].Name);
            Assert.Equal(directoriesList[i].DateCreated.Value, resultList[i].DateCreated);
            Assert.Equal(directoriesList[i].DateModified.Value, resultList[i].DateModified);
            Assert.Empty(resultList[i].Items); // since files are not included
        }

        _mockDirectoryService.Received(1).GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidQueryWithFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetDirectoriesQuery getDirectoriesQuery = _getDirectoriesQueryFixture.Create(includeHiddenElements: true);

        IEnumerable<Directory> directories = _directoryFixture.CreateMany();

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(Result.From(directories));

        // Act
        Result<IEnumerable<DirectoryResponse>> result = await _sut.HandleAsync(getDirectoriesQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.IsAssignableFrom<IEnumerable<DirectoryResponse>>(result.Value);
        Assert.Equal(directories.Count(), result.Value.Count());

        List<DirectoryResponse> resultList = [.. result.Value];
        List<Directory> directoriesList = [.. directories];

        for (int i = 0; i < resultList.Count; i++)
        {
            Assert.Equal(directoriesList[i].Id.Path, resultList[i].Path);
            Assert.Equal(directoriesList[i].Name, resultList[i].Name);
            Assert.Equal(directoriesList[i].DateCreated.Value, resultList[i].DateCreated);
            Assert.Equal(directoriesList[i].DateModified.Value, resultList[i].DateModified);
            Assert.Equal(directoriesList[i].Items.Count, resultList[i].Items.Count);
        }

        _mockDirectoryService.Received(1).GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements);
    }

    [Fact]
    public async Task HandleAsync_WhenDirectoryServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetDirectoriesQuery query = _getDirectoriesQueryFixture.Create();
        Error error = Error.Failure("DirectoryService.Error", "An error occurred");
        _mockDirectoryService.GetSubdirectories(query.Path!, query.IncludeHiddenElements)
            .Returns(error);

        // Act
        Result<IEnumerable<DirectoryResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path!, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task HandleAsync_WhenDirectoryServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetDirectoriesQuery query = _getDirectoriesQueryFixture.Create();
        Result<IEnumerable<Directory>> emptyList = Result.From(Enumerable.Empty<Directory>());
        _mockDirectoryService.GetSubdirectories(query.Path!, query.IncludeHiddenElements)
            .Returns(emptyList);

        // Act
        Result<IEnumerable<DirectoryResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path!, query.IncludeHiddenElements);
    }
}
