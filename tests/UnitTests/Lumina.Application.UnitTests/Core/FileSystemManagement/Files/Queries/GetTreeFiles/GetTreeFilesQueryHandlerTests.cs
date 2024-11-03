#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Queries.GetTreeFiles.Fixtures;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeFilesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IFileService _mockFileService;
    private readonly GetTreeFilesQueryHandler _sut;
    private readonly FileFixture _fileFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryHandler"/> class.
    /// </summary>
    public GetTreeFilesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockFileService = Substitute.For<IFileService>();
        _sut = new GetTreeFilesQueryHandler(_mockFileService);
        _fileFixture = new FileFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithoutHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeFilesQuery getFilesQuery = GetTreeFilesQueryFixture.CreateGetFilesQuery(false);

        IEnumerable<File> files = _fileFixture.CreateMany();

        _mockFileService.GetFiles(getFilesQuery.Path!, getFilesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(files));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getFilesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().HaveCount(files.Count());

        List<FileSystemTreeNodeResponse> resultList = result.Value.ToList();
        List<File> filesList = files.ToList();

        for (int i = 0; i < resultList.Count; i++)
        {
            resultList[i].Path.Should().Be(filesList[i].Id.Path);
            resultList[i].Name.Should().Be(filesList[i].Name);
            resultList[i].ItemType.Should().Be(FileSystemItemType.File);
            resultList[i].IsExpanded.Should().BeFalse();
            resultList[i].ChildrenLoaded.Should().BeFalse();
            resultList[i].Children.Should().BeEmpty();
        }

        _mockFileService.Received(1).GetFiles(getFilesQuery.Path!, getFilesQuery.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeFilesQuery getFilesQuery = GetTreeFilesQueryFixture.CreateGetFilesQuery(true);

        IEnumerable<File> files = _fileFixture.CreateMany();

        _mockFileService.GetFiles(getFilesQuery.Path!, getFilesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(files));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getFilesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().HaveCount(files.Count());

        List<FileSystemTreeNodeResponse> resultList = result.Value.ToList();
        List<File> filesList = files.ToList();

        for (int i = 0; i < resultList.Count; i++)
        {
            resultList[i].Path.Should().Be(filesList[i].Id.Path);
            resultList[i].Name.Should().Be(filesList[i].Name);
            resultList[i].ItemType.Should().Be(FileSystemItemType.File);
            resultList[i].IsExpanded.Should().BeFalse();
            resultList[i].ChildrenLoaded.Should().BeFalse();
            resultList[i].Children.Should().BeEmpty();
        }

        _mockFileService.Received(1).GetFiles(getFilesQuery.Path!, getFilesQuery.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenFileServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetTreeFilesQuery query = _fixture.Create<GetTreeFilesQuery>();
        Error error = Error.Failure("FileService.Error", "An error occurred");
        _mockFileService.GetFiles(query.Path!, query.IncludeHiddenElements)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockFileService.Received(1).GetFiles(query.Path!, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenFileServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetTreeFilesQuery query = _fixture.Create<GetTreeFilesQuery>();
        ErrorOr<IEnumerable<File>> emptyList = ErrorOrFactory.From(Enumerable.Empty<File>());
        _mockFileService.GetFiles(query.Path!, query.IncludeHiddenElements)
            .Returns(emptyList);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockFileService.Received(1).GetFiles(query.Path!, query.IncludeHiddenElements);
    }
}
