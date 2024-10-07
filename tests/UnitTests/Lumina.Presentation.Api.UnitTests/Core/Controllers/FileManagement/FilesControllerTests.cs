#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Files.Queries.GetFiles;
using Lumina.Application.Core.FileManagement.Files.Queries.GetTreeFiles;
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
/// Contains unit tests for the <see cref="FilesController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FilesControllerTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    private readonly ISender _mockMediator;
    private readonly FilesController _sut;
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesControllerTests"/> class.
    /// </summary>
    public FilesControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockMediator = Substitute.For<ISender>();
        _sut = new FilesController(_mockMediator);
        _fileSystemTreeNodeResponseFixture = new FileSystemTreeNodeResponseFixture();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task GetTreeFiles_WhenCalled_ShouldReturnFileSystemTreeNodeResponses()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        List<FileSystemTreeNodeResponse> expectedResponses = _fileSystemTreeNodeResponseFixture.CreateMany(3, 3, 5);
        _mockMediator.Send(Arg.Any<GetTreeFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<FileSystemTreeNodeResponse> result = _sut.GetTreeFiles(path, includeHiddenElements, cancellationToken);

        // Assert
        List<FileSystemTreeNodeResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetTreeFiles_WhenMediatorReturnsError_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        _mockMediator.Send(Arg.Any<GetTreeFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Errors.FileManagement.DirectoryNotFound);

        // Act
        IAsyncEnumerable<FileSystemTreeNodeResponse> result = _sut.GetTreeFiles(path, includeHiddenElements, cancellationToken);

        // Assert
        List<FileSystemTreeNodeResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFiles_WhenCalled_ShouldReturnFileResponses()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        List<FileResponse> expectedResponses = _fixture.CreateMany<FileResponse>(3).ToList();
        _mockMediator.Send(Arg.Any<GetFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<FileResponse> result = _sut.GetFiles(path, includeHiddenElements, cancellationToken);

        // Assert
        List<FileResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetFiles_WhenMediatorReturnsError_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationToken cancellationToken = CancellationToken.None;

        _mockMediator.Send(Arg.Any<GetFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Errors.FileManagement.DirectoryNotFound);

        // Act
        IAsyncEnumerable<FileResponse> result = _sut.GetFiles(path, includeHiddenElements, cancellationToken);

        // Assert
        List<FileResponse> actualResponses = await result.ToListAsync(cancellationToken);
        actualResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTreeFiles_WhenCancellationRequested_ShouldCancelEnumeration()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationTokenSource cts = new();

        List<FileSystemTreeNodeResponse> expectedResponses = _fileSystemTreeNodeResponseFixture.CreateMany(100, 3, 5);
        _mockMediator.Send(Arg.Any<GetTreeFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<FileSystemTreeNodeResponse> result = _sut.GetTreeFiles(path, includeHiddenElements, cts.Token);
        cts.Cancel();

        // Assert
        List<FileSystemTreeNodeResponse> actualResponses = await result.ToListAsync();
        actualResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFiles_WhenCancellationRequested_ShouldCancelEnumeration()
    {
        // Arrange
        string path = @"C:\TestDir";
        bool includeHiddenElements = false;
        CancellationTokenSource cts = new();

        List<FileResponse> expectedResponses = _fixture.CreateMany<FileResponse>(100).ToList();
        _mockMediator.Send(Arg.Any<GetFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IAsyncEnumerable<FileResponse> result = _sut.GetFiles(path, includeHiddenElements, cts.Token);
        cts.Cancel();

        // Assert
        List<FileResponse> actualResponses = await result.ToListAsync();
        actualResponses.Should().BeEmpty();
    }
    #endregion
}
