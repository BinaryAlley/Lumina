#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Nemera.Application.Fixtures.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Nemera.Contracts.Responses.FileSystemManagement.Common;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Nemera.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Nemera.Domain.SharedKernel.Common.Enums.FileSystem;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeFilesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesQueryHandlerTests
{
    private readonly IFileService _mockFileService;
    private readonly GetTreeFilesQueryHandler _sut;
    private readonly FileFixture _fileFixture = new();
    private readonly GetTreeFilesQueryFixture _getTreeFilesQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryHandler"/> class.
    /// </summary>
    public GetTreeFilesQueryHandlerTests()
    {
        _mockFileService = Substitute.For<IFileService>();
        IValidator<GetTreeFilesQuery> mockValidator = Substitute.For<IValidator<GetTreeFilesQuery>>();
        mockValidator.Validate(Arg.Any<GetTreeFilesQuery>())
            .Returns([]);
        _sut = new GetTreeFilesQueryHandler(_mockFileService, mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidQueryWithoutHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeFilesQuery getFilesQuery = _getTreeFilesQueryFixture.Create(shouldIncludeHiddenElements: false);

        IEnumerable<File> files =
        [
            _fileFixture.Create(isHidden: true),
            _fileFixture.Create(isHidden: false)
        ];

        _mockFileService.GetFiles(getFilesQuery.Path!, getFilesQuery.ShouldIncludeHiddenElements)
            .Returns(Result.From(files));

        // Act
        Result<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.HandleAsync(getFilesQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.IsAssignableFrom<IEnumerable<FileSystemTreeNodeResponse>>(result.Value);
        Assert.Equal(files.Count(), result.Value.Count());

        List<FileSystemTreeNodeResponse> resultList = [.. result.Value];
        List<File> filesList = [.. files];

        for (int i = 0; i < resultList.Count; i++)
        {
            Assert.Equal(filesList[i].Id.Path, resultList[i].Path);
            Assert.Equal(filesList[i].Name, resultList[i].Name);
            Assert.Equal(FileSystemItemType.File, resultList[i].ItemType);
            Assert.Equal(filesList[i].IsHidden, resultList[i].IsHidden);
            Assert.False(resultList[i].IsExpanded);
            Assert.False(resultList[i].ChildrenLoaded);
            Assert.Empty(resultList[i].Children);
        }

        _mockFileService.Received(1).GetFiles(getFilesQuery.Path!, getFilesQuery.ShouldIncludeHiddenElements);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidQueryWithHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeFilesQuery getFilesQuery = _getTreeFilesQueryFixture.Create(shouldIncludeHiddenElements: true);

        IEnumerable<File> files =
        [
            _fileFixture.Create(isHidden: true),
            _fileFixture.Create(isHidden: false)
        ];

        _mockFileService.GetFiles(getFilesQuery.Path!, getFilesQuery.ShouldIncludeHiddenElements)
            .Returns(Result.From(files));

        // Act
        Result<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.HandleAsync(getFilesQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.IsAssignableFrom<IEnumerable<FileSystemTreeNodeResponse>>(result.Value);
        Assert.Equal(files.Count(), result.Value.Count());

        List<FileSystemTreeNodeResponse> resultList = [.. result.Value];
        List<File> filesList = [.. files];

        for (int i = 0; i < resultList.Count; i++)
        {
            Assert.Equal(filesList[i].Id.Path, resultList[i].Path);
            Assert.Equal(filesList[i].Name, resultList[i].Name);
            Assert.Equal(FileSystemItemType.File, resultList[i].ItemType);
            Assert.Equal(filesList[i].IsHidden, resultList[i].IsHidden);
            Assert.False(resultList[i].IsExpanded);
            Assert.False(resultList[i].ChildrenLoaded);
            Assert.Empty(resultList[i].Children);
        }

        _mockFileService.Received(1).GetFiles(getFilesQuery.Path!, getFilesQuery.ShouldIncludeHiddenElements);
    }

    [Fact]
    public async Task HandleAsync_WhenFileServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
        Error error = Error.Failure("FileService.Error", "An error occurred");
        _mockFileService.GetFiles(query.Path!, query.ShouldIncludeHiddenElements)
            .Returns(error);

        // Act
        Result<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockFileService.Received(1).GetFiles(query.Path!, query.ShouldIncludeHiddenElements);
    }

    [Fact]
    public async Task HandleAsync_WhenFileServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
        Result<IEnumerable<File>> emptyList = Result.From(Enumerable.Empty<File>());
        _mockFileService.GetFiles(query.Path!, query.ShouldIncludeHiddenElements)
            .Returns(emptyList);

        // Act
        Result<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
        _mockFileService.Received(1).GetFiles(query.Path!, query.ShouldIncludeHiddenElements);
    }
}
