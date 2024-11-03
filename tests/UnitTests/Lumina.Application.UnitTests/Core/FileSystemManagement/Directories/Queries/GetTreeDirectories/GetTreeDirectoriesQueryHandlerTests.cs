#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetTreeDirectories;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Queries.GetTreeDirectories.Fixtures;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Queries.GetTreeDirectories;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeDirectoriesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeDirectoriesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IDirectoryService _mockDirectoryService;
    private readonly GetTreeDirectoriesQueryHandler _sut;
    private readonly DirectoryFixture _directoryFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeDirectoriesQueryHandlerTests"/> class.
    /// </summary>
    public GetTreeDirectoriesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockDirectoryService = Substitute.For<IDirectoryService>();
        _sut = new GetTreeDirectoriesQueryHandler(_mockDirectoryService);
        _directoryFixture = new DirectoryFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithoutFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeDirectoriesQuery getDirectoriesQuery = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery(false);

        IEnumerable<Directory> directories = _directoryFixture.CreateMany();

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(directories));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getDirectoriesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().HaveCount(directories.Count());

        List<FileSystemTreeNodeResponse> resultList = result.Value.ToList();
        List<Directory> directoriesList = directories.ToList();

        for (int i = 0; i < resultList.Count; i++)
        {
            resultList[i].Path.Should().Be(directoriesList[i].Id.Path);
            resultList[i].Name.Should().Be(directoriesList[i].Name);
            resultList[i].ItemType.Should().Be(FileSystemItemType.Directory);
            resultList[i].IsExpanded.Should().BeFalse();
            resultList[i].ChildrenLoaded.Should().BeFalse();
            resultList[i].Children.Should().BeEmpty();
        }

        _mockDirectoryService.Received(1).GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeDirectoriesQuery getDirectoriesQuery = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery(true);

        IEnumerable<Directory> directories = _directoryFixture.CreateManyNested();

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(directories));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getDirectoriesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().HaveCount(directories.Count());

        List<FileSystemTreeNodeResponse> resultList = result.Value.ToList();
        List<Directory> directoriesList = directories.ToList();

        for (int i = 0; i < resultList.Count; i++)
        {
            FileSystemTreeNodeResponse response = resultList[i];
            Directory directory = directoriesList[i];

            response.Path.Should().Be(directory.Id.Path);
            response.Name.Should().Be(directory.Name);
            response.ItemType.Should().Be(FileSystemItemType.Directory);
            response.IsExpanded.Should().BeFalse();
            response.ChildrenLoaded.Should().BeFalse();
            response.Children.Should().HaveCount(directory.Items.Count);

            List<FileSystemTreeNodeResponse> childResponses = [.. response.Children];
            List<FileSystemItem> directoryItems = [.. directory.Items];

            for (int j = 0; j < childResponses.Count; j++)
            {
                FileSystemTreeNodeResponse childResponse = childResponses[j];
                FileSystemItem item = directoryItems[j];

                childResponse.Path.Should().Be(item.Id.Path);
                childResponse.Name.Should().Be(item.Name);
                childResponse.ItemType.Should().Be(item.Type);
                childResponse.IsExpanded.Should().BeFalse();
                childResponse.ChildrenLoaded.Should().BeFalse();

                if (item is Directory)
                    childResponse.Children.Should().NotBeNull();
                else
                    childResponse.Children.Should().BeEmpty();
            }
        }
        _mockDirectoryService.Received(1).GetSubdirectories(getDirectoriesQuery.Path!, getDirectoriesQuery.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenDirectoryServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetTreeDirectoriesQuery query = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery();
        Error error = Error.Failure("DirectoryService.Error", "An error occurred");
        _mockDirectoryService.GetSubdirectories(query.Path!, query.IncludeHiddenElements)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path!, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenDirectoryServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetTreeDirectoriesQuery query = _fixture.Create<GetTreeDirectoriesQuery>();
        ErrorOr<IEnumerable<Directory>> emptyList = ErrorOrFactory.From(Enumerable.Empty<Directory>());
        _mockDirectoryService.GetSubdirectories(query.Path!, query.IncludeHiddenElements)
            .Returns(emptyList);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path!, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenCalledWithNestedDirectories_ShouldReturnCorrectlyNestedResponse()
    {
        // Arrange
        GetTreeDirectoriesQuery query = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery(true);
        IEnumerable<Directory> nestedDirectories = _directoryFixture.CreateManyNested(2, 2, 2);

        _mockDirectoryService.GetSubdirectories(query.Path!, query.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(nestedDirectories));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().HaveCount(2); // top level directories

        List<FileSystemTreeNodeResponse> resultList = result.Value.ToList();
        List<Directory> directoriesList = nestedDirectories.ToList();

        for (int i = 0; i < resultList.Count; i++)
        {
            FileSystemTreeNodeResponse response = resultList[i];
            Directory directory = directoriesList[i];

            response.Path.Should().Be(directory.Id.Path);
            response.Name.Should().Be(directory.Name);
            response.ItemType.Should().Be(FileSystemItemType.Directory);
            response.IsExpanded.Should().BeFalse();
            response.ChildrenLoaded.Should().BeFalse();
            response.Children.Should().HaveCount(directory.Items.Count);

            // check first level of children
            for (int j = 0; j < response.Children.Count; j++)
            {
                FileSystemTreeNodeResponse childResponse = response.Children[j];
                FileSystemItem childItem = directory.Items.ElementAt(j);

                childResponse.Path.Should().Be(childItem.Id.Path);
                childResponse.Name.Should().Be(childItem.Name);
                childResponse.ItemType.Should().Be(childItem.Type);
                childResponse.IsExpanded.Should().BeFalse();
                childResponse.ChildrenLoaded.Should().BeFalse();

                if (childItem is Directory dir)
                {
                    childResponse.Children.Should().NotBeNull();
                    childResponse.Children.Should().HaveCount(dir.Items.Count);
                }
                else
                    childResponse.Children.Should().BeEmpty();
            }
        }

        result.Value.Any(r => r.Children.Count != 0).Should().BeTrue();
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path!, query.IncludeHiddenElements);
    }
}
