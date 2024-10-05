#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Files.Queries.GetFiles;
using Lumina.Application.UnitTests.Core.FileManagement.Files.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Files.Queries.GetFiles.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using MapsterMapper;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Files.Queries.GetFiles;


/// <summary>
/// Contains unit tests for the <see cref="GetFilesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesQueryHandlerTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    private readonly IMapper _mockMapper;
    private readonly IFileService _mockFileService;
    private readonly GetFilesQueryHandler _sut;
    private readonly FileFixture _fileFixture;
    private readonly FileResponseFixture _fileResponseFixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesQueryHandlerTests"/> class.
    /// </summary>
    public GetFilesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockFileService = Substitute.For<IFileService>();
        _mockMapper = Substitute.For<IMapper>();
        _sut = new GetFilesQueryHandler(_mockFileService, _mockMapper);
        _fileFixture = new FileFixture();
        _fileResponseFixture = new FileResponseFixture();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithoutHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetFilesQuery getFilesQuery = GetFilesQueryFixture.CreateGetFilesQuery(false);

        IEnumerable<File> files = _fileFixture.CreateMany();
        IEnumerable<FileResponse> fileResponses = _fileResponseFixture.CreateMany(files.Count());

        _mockFileService.GetFiles(getFilesQuery.Path, getFilesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(files));

        _mockMapper.Map<IEnumerable<FileResponse>>(Arg.Is<IEnumerable<File>>(f => f == files))
            .Returns(fileResponses);

        // Act
        ErrorOr<IEnumerable<FileResponse>> result = await _sut.Handle(getFilesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileResponse>>();
        result.Value.Should().BeEquivalentTo(fileResponses);
        _mockFileService.Received(1).GetFiles(getFilesQuery.Path, getFilesQuery.IncludeHiddenElements);
        _mockMapper.Received(1).Map<IEnumerable<FileResponse>>(Arg.Is<IEnumerable<File>>(f => f == files));
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetFilesQuery getFilesQuery = GetFilesQueryFixture.CreateGetFilesQuery(true);

        IEnumerable<File> files = _fileFixture.CreateMany();
        IEnumerable<FileResponse> fileResponses = _fileResponseFixture.CreateMany(files.Count());

        _mockFileService.GetFiles(getFilesQuery.Path, getFilesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(files));

        _mockMapper.Map<IEnumerable<FileResponse>>(Arg.Is<IEnumerable<File>>(f => f == files))
            .Returns(fileResponses);

        // Act
        ErrorOr<IEnumerable<FileResponse>> result = await _sut.Handle(getFilesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileResponse>>();
        result.Value.Should().BeEquivalentTo(fileResponses);
        _mockFileService.Received(1).GetFiles(getFilesQuery.Path, getFilesQuery.IncludeHiddenElements);
        _mockMapper.Received(1).Map<IEnumerable<FileResponse>>(Arg.Is<IEnumerable<File>>(f => f == files));
    }

    [Fact]
    public async Task Handle_WhenFileServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetFilesQuery query = _fixture.Create<GetFilesQuery>();
        Error error = Error.Failure("FileService.Error", "An error occurred");
        _mockFileService.GetFiles(query.Path, query.IncludeHiddenElements)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<FileResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockFileService.Received(1).GetFiles(query.Path, query.IncludeHiddenElements);
        _mockMapper.DidNotReceive().Map<IEnumerable<FileResponse>>(Arg.Any<IEnumerable<File>>());
    }

    [Fact]
    public async Task Handle_WhenFileServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetFilesQuery query = _fixture.Create<GetFilesQuery>();
        ErrorOr<IEnumerable<File>> emptyList = ErrorOrFactory.From(Enumerable.Empty<File>());
        _mockFileService.GetFiles(query.Path, query.IncludeHiddenElements)
            .Returns(emptyList);

        _mockMapper.Map<IEnumerable<FileResponse>>(Arg.Is<IEnumerable<File>>(f => !f.Any()))
            .Returns([]);

        // Act
        ErrorOr<IEnumerable<FileResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockFileService.Received(1).GetFiles(query.Path, query.IncludeHiddenElements);
        _mockMapper.Received(1).Map<IEnumerable<FileResponse>>(Arg.Is<IEnumerable<File>>(f => !f.Any()));
    }
    #endregion
}
