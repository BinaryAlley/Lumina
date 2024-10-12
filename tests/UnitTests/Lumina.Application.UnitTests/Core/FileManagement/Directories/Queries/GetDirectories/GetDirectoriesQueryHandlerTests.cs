#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectories;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetDirectories.Fixtures;
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

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IMapper _mockMapper;
    private readonly IDirectoryService _mockDirectoryService;
    private readonly GetDirectoriesQueryHandler _sut;
    private readonly DirectoryFixture _directoryFixture;
    private readonly DirectoryResponseFixture _directoryResponseFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesQueryHandlerTests"/> class.
    /// </summary>
    public GetDirectoriesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockDirectoryService = Substitute.For<IDirectoryService>();
        _mockMapper = Substitute.For<IMapper>();
        _sut = new GetDirectoriesQueryHandler(_mockDirectoryService, _mockMapper);
        _directoryFixture = new DirectoryFixture();
        _directoryResponseFixture = new DirectoryResponseFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithoutFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetDirectoriesQuery getDirectoriesQuery = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery(false);

        IEnumerable<Directory> directories = _directoryFixture.CreateMany();
        IEnumerable<DirectoryResponse> directoryResponses = _directoryResponseFixture.CreateMany(directories.Count());

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(directories));

        _mockMapper.Map<IEnumerable<DirectoryResponse>>(Arg.Is<IEnumerable<Directory>>(d => d == directories))
            .Returns(directoryResponses);

        // Act
        ErrorOr<IEnumerable<DirectoryResponse>> result = await _sut.Handle(getDirectoriesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<DirectoryResponse>>();
        result.Value.Should().BeEquivalentTo(directoryResponses);
        _mockDirectoryService.Received(1).GetSubdirectories(getDirectoriesQuery.Path, getDirectoriesQuery.IncludeHiddenElements);
        _mockMapper.Received(1).Map<IEnumerable<DirectoryResponse>>(Arg.Is<IEnumerable<Directory>>(d => d == directories));
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryWithFilesIncluded_ShouldReturnSuccessResult()
    {
        // Arrange
        GetDirectoriesQuery getDirectoriesQuery = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery(true);

        IEnumerable<Directory> directories = _directoryFixture.CreateMany();
        IEnumerable<DirectoryResponse> directoryResponses = _directoryResponseFixture.CreateMany(directories.Count());

        _mockDirectoryService.GetSubdirectories(getDirectoriesQuery.Path, getDirectoriesQuery.IncludeHiddenElements)
            .Returns(ErrorOrFactory.From(directories));

        _mockMapper.Map<IEnumerable<DirectoryResponse>>(Arg.Is<IEnumerable<Directory>>(d => d == directories))
            .Returns(directoryResponses);

        // Act
        ErrorOr<IEnumerable<DirectoryResponse>> result = await _sut.Handle(getDirectoriesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<DirectoryResponse>>();
        result.Value.Should().BeEquivalentTo(directoryResponses);
        _mockDirectoryService.Received(1).GetSubdirectories(getDirectoriesQuery.Path, getDirectoriesQuery.IncludeHiddenElements);
        _mockMapper.Received(1).Map<IEnumerable<DirectoryResponse>>(Arg.Is<IEnumerable<Directory>>(d => d == directories));
    }

    [Fact]
    public async Task Handle_WhenDirectoryServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetDirectoriesQuery query = _fixture.Create<GetDirectoriesQuery>();
        Error error = Error.Failure("DirectoryService.Error", "An error occurred");
        _mockDirectoryService.GetSubdirectories(query.Path, query.IncludeHiddenElements)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<DirectoryResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path, query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenDirectoryServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetDirectoriesQuery query = _fixture.Create<GetDirectoriesQuery>();
        ErrorOr<IEnumerable<Directory>> emptyList = ErrorOrFactory.From(Enumerable.Empty<Directory>());
        _mockDirectoryService.GetSubdirectories(query.Path, query.IncludeHiddenElements)
            .Returns(emptyList);

        // Act
        ErrorOr<IEnumerable<DirectoryResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockDirectoryService.Received(1).GetSubdirectories(query.Path, query.IncludeHiddenElements);
    }
}
