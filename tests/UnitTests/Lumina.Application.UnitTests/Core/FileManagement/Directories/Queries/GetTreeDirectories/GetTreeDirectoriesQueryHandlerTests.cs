#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectoryTree;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetTreeDirectories;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetDirectoryTree.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetTreeDirectories.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Files.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetTreeDirectories;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeDirectoriesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeDirectoriesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IMapper _mockMapper;
    private readonly IDirectoryService _mockDirectoryService;
    private readonly GetTreeDirectoriesQueryHandler _sut;
    private readonly GetTreeDirectoriesQueryFixture _getTreeDirectoriesQueryFixture;
    private readonly DirectoryFixture _directoryFixture;
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeDirectoriesQueryHandlerTests"/> class.
    /// </summary>
    public GetTreeDirectoriesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockDirectoryService = Substitute.For<IDirectoryService>();
        _mockMapper = Substitute.For<IMapper>();
        _sut = new GetTreeDirectoriesQueryHandler(_mockDirectoryService, _mockMapper);
        _getTreeDirectoriesQueryFixture = new GetTreeDirectoriesQueryFixture();
        _directoryFixture = new DirectoryFixture();
        _fileSystemTreeNodeResponseFixture = new FileSystemTreeNodeResponseFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithoutFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeDirectoriesQuery getDirectoriesQuery = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery(false);

        IEnumerable<Directory> directories = _directoryFixture.CreateMany();
        IEnumerable<FileSystemTreeNodeResponse> directoryResponses = _fileSystemTreeNodeResponseFixture.CreateMany(directories.Count());

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(directories));

        _mockMapper.Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<Directory>>(d => d == directories))
            .Returns(directoryResponses);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getDirectoriesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().BeEquivalentTo(directoryResponses);
        _mockDirectoryService.Received(1).GetSubdirectories(getDirectoriesQuery.Path, getDirectoriesQuery.IncludeHiddenElements);
        _mockMapper.Received(1).Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<Directory>>(d => d == directories));
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeDirectoriesQuery getDirectoriesQuery = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery(true);

        IEnumerable<Directory> directories = _directoryFixture.CreateMany();
        IEnumerable<FileSystemTreeNodeResponse> directoryResponses = _fileSystemTreeNodeResponseFixture.CreateMany(directories.Count());

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(directories));

        _mockMapper.Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<Directory>>(d => d == directories))
            .Returns(directoryResponses);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getDirectoriesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().BeEquivalentTo(directoryResponses);
        _mockDirectoryService.Received(1).GetSubdirectories(getDirectoriesQuery.Path, getDirectoriesQuery.IncludeHiddenElements);
        _mockMapper.Received(1).Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<Directory>>(d => d == directories));
    }

    [Fact]
    public async Task Handle_WhenDirectoryServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetTreeDirectoriesQuery query = _fixture.Create<GetTreeDirectoriesQuery>();
        Error error = Error.Failure("DirectoryService.Error", "An error occurred");
        _mockDirectoryService.GetSubdirectories(query.Path, query.IncludeHiddenElements)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenDirectoryServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetTreeDirectoriesQuery query = _fixture.Create<GetTreeDirectoriesQuery>();
        ErrorOr<IEnumerable<Directory>> emptyList = ErrorOrFactory.From(Enumerable.Empty<Directory>());
        _mockDirectoryService.GetSubdirectories(query.Path, query.IncludeHiddenElements)
            .Returns(emptyList);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenCalledWithNestedDirectories_ShouldReturnCorrectlyNestedResponse()
    {
        // Arrange
        GetTreeDirectoriesQuery query = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery(true);
        IEnumerable<Directory> nestedDirectories = _directoryFixture.CreateManyNested();
        IEnumerable<FileSystemTreeNodeResponse> nestedResponses = _fileSystemTreeNodeResponseFixture.CreateMany(2, 2, 2);

        _mockDirectoryService.GetSubdirectories(query.Path, query.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(nestedDirectories));

        _mockMapper.Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<Directory>>(d => d == nestedDirectories))
            .Returns(nestedResponses);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().BeEquivalentTo(nestedResponses, options => options.IncludingNestedObjects());
        result.Value.Any(r => r.Children.Count != 0).Should().BeTrue();
    }
}
