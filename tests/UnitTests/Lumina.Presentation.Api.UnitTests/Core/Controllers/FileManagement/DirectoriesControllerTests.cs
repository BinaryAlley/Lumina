#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectories;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectoryTree;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetTreeDirectories;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Common.Errors;
using Lumina.Presentation.Api.Core.Controllers.FileManagement;
using Lumina.Presentation.Api.UnitTests.Core.Controllers.FileManagement.Fixtures;
using Mediator;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Controllers.FileManagement;

/// <summary>
/// Contains unit tests for the <see cref="DirectoriesController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoriesControllerTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    private readonly ISender _mockMediator;
    private readonly DirectoriesController _sut;
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoriesControllerTests"/> class.
    /// </summary>
    public DirectoriesControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockMediator = Substitute.For<ISender>();
        _sut = new DirectoriesController(_mockMediator);
        _fileSystemTreeNodeResponseFixture = new FileSystemTreeNodeResponseFixture();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task GetDirectoryTree_WhenCalled_ShouldReturnFileSystemTreeNodeResponses()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeFiles = true;
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        List<FileSystemTreeNodeResponse> expectedResponses = _fileSystemTreeNodeResponseFixture.CreateMany(3, 3, 5);
        _mockMediator.Send(Arg.Any<GetDirectoryTreeQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<FileSystemTreeNodeResponse> result = _sut.GetDirectoryTree(path, includeFiles, includeHiddenElements, cancellationToken);

        // Assert
        List<FileSystemTreeNodeResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetDirectoryTree_WhenMediatorReturnsError_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeFiles = true;
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        _mockMediator.Send(Arg.Any<GetDirectoryTreeQuery>(), Arg.Any<CancellationToken>())
            .Returns(Errors.FileManagement.DirectoryNotFound);

        // Act
        IAsyncEnumerable<FileSystemTreeNodeResponse> result = _sut.GetDirectoryTree(path, includeFiles, includeHiddenElements, cancellationToken);

        // Assert
        List<FileSystemTreeNodeResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTreeDirectories_WhenCalled_ShouldReturnFileSystemTreeNodeResponses()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        List<FileSystemTreeNodeResponse> expectedResponses = _fileSystemTreeNodeResponseFixture.CreateMany(3, 3, 5);
        _mockMediator.Send(Arg.Any<GetTreeDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<FileSystemTreeNodeResponse> result = _sut.GetTreeDirectories(path, includeHiddenElements, cancellationToken);

        // Assert
        List<FileSystemTreeNodeResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetTreeDirectories_WhenMediatorReturnsError_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        _mockMediator.Send(Arg.Any<GetTreeDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Errors.FileManagement.DirectoryNotFound);

        // Act
        IAsyncEnumerable<FileSystemTreeNodeResponse> result = _sut.GetTreeDirectories(path, includeHiddenElements, cancellationToken);

        // Assert
        List<FileSystemTreeNodeResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDirectories_WhenCalled_ShouldReturnDirectoryResponses()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        List<DirectoryResponse> expectedResponses = _fixture.CreateMany<DirectoryResponse>(3).ToList();
        _mockMediator.Send(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<DirectoryResponse> result = _sut.GetDirectories(path, includeHiddenElements, cancellationToken);

        // Assert
        List<DirectoryResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetDirectories_WhenMediatorReturnsError_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        _mockMediator.Send(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Errors.FileManagement.DirectoryNotFound);

        // Act
        IAsyncEnumerable<DirectoryResponse> result = _sut.GetDirectories(path, includeHiddenElements, cancellationToken);

        // Assert
        List<DirectoryResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDirectoryTree_WhenCancellationRequested_ShouldCancelEnumeration()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeFiles = true;
        bool includeHiddenElements = false;
        CancellationTokenSource cts = new();

        List<FileSystemTreeNodeResponse> expectedResponses = _fileSystemTreeNodeResponseFixture.CreateMany(100, 3, 5);
        _mockMediator.Send(Arg.Any<GetDirectoryTreeQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<FileSystemTreeNodeResponse> result = _sut.GetDirectoryTree(path, includeFiles, includeHiddenElements, cts.Token);
        cts.Cancel();

        // Assert
        List<FileSystemTreeNodeResponse> actualResponses = await result.ToListAsync();
        actualResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTreeDirectories_WhenCalled_ShouldCancelEnumerationWhenCancellationRequested()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationTokenSource cts = new();

        List<FileSystemTreeNodeResponse> expectedResponses = _fileSystemTreeNodeResponseFixture.CreateMany(100, 3, 5);
        _mockMediator.Send(Arg.Any<GetTreeDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<FileSystemTreeNodeResponse> result = _sut.GetTreeDirectories(path, includeHiddenElements, cts.Token);
        cts.Cancel();

        // Assert
        List<FileSystemTreeNodeResponse> actualResponses = await result.ToListAsync();
        actualResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDirectories_WhenCancellationRequested_ShouldCancelEnumeration()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationTokenSource cts = new();

        List<DirectoryResponse> expectedResponses = _fixture.CreateMany<DirectoryResponse>(100).ToList();
        _mockMediator.Send(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<DirectoryResponse> result = _sut.GetDirectories(path, includeHiddenElements, cts.Token);
        cts.Cancel();

        // Assert
        List<DirectoryResponse> actualResponses = await result.ToListAsync();
        actualResponses.Should().BeEmpty();
    }
    #endregion
}
