#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetFiles;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Queries.GetFiles.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Queries.GetFiles;


/// <summary>
/// Contains unit tests for the <see cref="GetFilesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IFileService _mockFileService;
    private readonly GetFilesQueryHandler _sut;
    private readonly FileFixture _fileFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesQueryHandlerTests"/> class.
    /// </summary>
    public GetFilesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockFileService = Substitute.For<IFileService>();
        _sut = new GetFilesQueryHandler(_mockFileService);
        _fileFixture = new FileFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithoutHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetFilesQuery getFilesQuery = GetFilesQueryFixture.CreateGetFilesQuery(false);

        IEnumerable<File> files = _fileFixture.CreateMany();

        _mockFileService.GetFiles(getFilesQuery.Path!, getFilesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(files));

        // Act
        ErrorOr<IEnumerable<FileResponse>> result = await _sut.Handle(getFilesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileResponse>>();
        result.Value.Should().HaveCount(files.Count());

        List<FileResponse> resultList = result.Value.ToList();
        List<File> filesList = files.ToList();

        for (int i = 0; i < resultList.Count; i++)
        {
            resultList[i].Path.Should().Be(filesList[i].Id.Path);
            resultList[i].Name.Should().Be(filesList[i].Name);
            resultList[i].DateCreated.Should().Be(filesList[i].DateCreated.Value);
            resultList[i].DateModified.Should().Be(filesList[i].DateModified.Value);
            resultList[i].Size.Should().Be(filesList[i].Size);
        }

        _mockFileService.Received(1).GetFiles(getFilesQuery.Path!, getFilesQuery.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithHiddenFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetFilesQuery getFilesQuery = GetFilesQueryFixture.CreateGetFilesQuery(true);

        IEnumerable<File> files = _fileFixture.CreateMany();

        _mockFileService.GetFiles(getFilesQuery.Path!, getFilesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(files));

        // Act
        ErrorOr<IEnumerable<FileResponse>> result = await _sut.Handle(getFilesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileResponse>>();
        result.Value.Should().HaveCount(files.Count());

        List<FileResponse> resultList = result.Value.ToList();
        List<File> filesList = files.ToList();

        for (int i = 0; i < resultList.Count; i++)
        {
            resultList[i].Path.Should().Be(filesList[i].Id.Path);
            resultList[i].Name.Should().Be(filesList[i].Name);
            resultList[i].DateCreated.Should().Be(filesList[i].DateCreated.Value);
            resultList[i].DateModified.Should().Be(filesList[i].DateModified.Value);
            resultList[i].Size.Should().Be(filesList[i].Size);
        }

        _mockFileService.Received(1).GetFiles(getFilesQuery.Path!, getFilesQuery.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenFileServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetFilesQuery query = _fixture.Create<GetFilesQuery>();
        Error error = Error.Failure("FileService.Error", "An error occurred");
        _mockFileService.GetFiles(query.Path!, query.IncludeHiddenElements)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<FileResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockFileService.Received(1).GetFiles(query.Path!, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenFileServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetFilesQuery query = _fixture.Create<GetFilesQuery>();
        ErrorOr<IEnumerable<File>> emptyList = ErrorOrFactory.From(Enumerable.Empty<File>());
        _mockFileService.GetFiles(query.Path!, query.IncludeHiddenElements)
            .Returns(emptyList);

        // Act
        ErrorOr<IEnumerable<FileResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockFileService.Received(1).GetFiles(query.Path!, query.IncludeHiddenElements);
    }
}
