#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Queries.GetDirectories.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesQueryHandlerTests"/> class.
    /// </summary>
    public GetDirectoriesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockDirectoryService = Substitute.For<IDirectoryService>();
        _sut = new GetDirectoriesQueryHandler(_mockDirectoryService);
        _directoryFixture = new DirectoryFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithoutFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetDirectoriesQuery getDirectoriesQuery = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery(false);

        IEnumerable<Directory> directories = _directoryFixture.CreateMany();

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(directories));

        // Act
        ErrorOr<IEnumerable<DirectoryResponse>> result = await _sut.Handle(getDirectoriesQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.IsAssignableFrom<IEnumerable<DirectoryResponse>>(result.Value);
        Assert.Equal(directories.Count(), result.Value.Count());

        List<DirectoryResponse> resultList = result.Value.ToList();
        List<Directory> directoriesList = directories.ToList();

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
    public async Task Handle_WhenCalledWithValidQueryWithFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetDirectoriesQuery getDirectoriesQuery = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery(true);

        IEnumerable<Directory> directories = _directoryFixture.CreateMany();

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(directories));

        // Act
        ErrorOr<IEnumerable<DirectoryResponse>> result = await _sut.Handle(getDirectoriesQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.IsAssignableFrom<IEnumerable<DirectoryResponse>>(result.Value);
        Assert.Equal(directories.Count(), result.Value.Count());

        List<DirectoryResponse> resultList = result.Value.ToList();
        List<Directory> directoriesList = directories.ToList();

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
    public async Task Handle_WhenDirectoryServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetDirectoriesQuery query = _fixture.Create<GetDirectoriesQuery>();
        Error error = Error.Failure("DirectoryService.Error", "An error occurred");
        _mockDirectoryService.GetSubdirectories(query.Path!, query.IncludeHiddenElements)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<DirectoryResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path!, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenDirectoryServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetDirectoriesQuery query = _fixture.Create<GetDirectoriesQuery>();
        ErrorOr<IEnumerable<Directory>> emptyList = ErrorOrFactory.From(Enumerable.Empty<Directory>());
        _mockDirectoryService.GetSubdirectories(query.Path!, query.IncludeHiddenElements)
            .Returns(emptyList);

        // Act
        ErrorOr<IEnumerable<DirectoryResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value);
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path!, query.IncludeHiddenElements);
    }
}
