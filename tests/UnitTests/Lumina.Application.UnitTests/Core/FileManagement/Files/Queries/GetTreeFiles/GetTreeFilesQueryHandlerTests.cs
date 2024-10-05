#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Files.Queries.GetTreeFiles;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Files.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Files.Queries.GetTreeFiles.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using MapsterMapper;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeFilesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesQueryHandlerTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    private readonly IMapper _mockMapper;
    private readonly IFileService _mockFileService;
    private readonly GetTreeFilesQueryHandler _sut;
    private readonly FileFixture _fileFixture;
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryHandler"/> class.
    /// </summary>
    public GetTreeFilesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockFileService = Substitute.For<IFileService>();
        _mockMapper = Substitute.For<IMapper>();
        _sut = new GetTreeFilesQueryHandler(_mockFileService, _mockMapper);
        _fileFixture = new FileFixture();
        _fileSystemTreeNodeResponseFixture = new FileSystemTreeNodeResponseFixture();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithoutHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeFilesQuery getFilesQuery = GetTreeFilesQueryFixture.CreateGetFilesQuery(false);

        IEnumerable<File> files = _fileFixture.CreateMany();
        IEnumerable<FileSystemTreeNodeResponse> fileResponses = _fileSystemTreeNodeResponseFixture.CreateMany(files.Count());

        _mockFileService.GetFiles(getFilesQuery.Path, getFilesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(files));

        _mockMapper.Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<File>>(f => f == files))
            .Returns(fileResponses);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getFilesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().BeEquivalentTo(fileResponses);
        _mockFileService.Received(1).GetFiles(getFilesQuery.Path, getFilesQuery.IncludeHiddenElements);
        _mockMapper.Received(1).Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<File>>(f => f == files));
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetTreeFilesQuery getFilesQuery = GetTreeFilesQueryFixture.CreateGetFilesQuery(true);

        IEnumerable<File> files = _fileFixture.CreateMany();
        IEnumerable<FileSystemTreeNodeResponse> fileResponses = _fileSystemTreeNodeResponseFixture.CreateMany(files.Count());

        _mockFileService.GetFiles(getFilesQuery.Path, getFilesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(files));

        _mockMapper.Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<File>>(f => f == files))
            .Returns(fileResponses);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getFilesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().BeEquivalentTo(fileResponses);
        _mockFileService.Received(1).GetFiles(getFilesQuery.Path, getFilesQuery.IncludeHiddenElements);
        _mockMapper.Received(1).Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<File>>(f => f == files));
    }

    [Fact]
    public async Task Handle_WhenFileServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetTreeFilesQuery query = _fixture.Create<GetTreeFilesQuery>();
        Error error = Error.Failure("FileService.Error", "An error occurred");
        _mockFileService.GetFiles(query.Path, query.IncludeHiddenElements)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockFileService.Received(1).GetFiles(query.Path, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenFileServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetTreeFilesQuery query = _fixture.Create<GetTreeFilesQuery>();
        ErrorOr<IEnumerable<File>> emptyList = ErrorOrFactory.From(Enumerable.Empty<File>());
        _mockFileService.GetFiles(query.Path, query.IncludeHiddenElements)
            .Returns(emptyList);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockFileService.Received(1).GetFiles(query.Path, query.IncludeHiddenElements);
    }
    #endregion
}
