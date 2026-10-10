#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Contains unit tests for the <see cref="BooksMediaLibraryScanItemMaterializer"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksMediaLibraryScanItemMaterializerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryPathTemplateService _mockPathTemplateService;
    private readonly IPathService _mockPathService;
    private readonly BooksMediaLibraryScanItemMaterializer _sut;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly Guid _libraryId;
    private readonly Guid _scanId;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksMediaLibraryScanItemMaterializerTests"/> class.
    /// </summary>
    public BooksMediaLibraryScanItemMaterializerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockPathTemplateService = Substitute.For<ILibraryPathTemplateService>();
        _mockPathService = Substitute.For<IPathService>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _libraryId = Guid.NewGuid();
        _scanId = Guid.NewGuid();

        // Default stubs: the library has no path template of its own, no book is stored yet, and the separator is the Windows one.
        _mockPathTemplateService.ResolveTemplate(Arg.Any<LibraryType>(), Arg.Any<IReadOnlyList<LibraryPathPart>>())
            .Returns(Result.From(LibraryPathTemplate.Empty()));
        _mockPathService.PathSeparator.Returns('\\');
        _mockPathService.GetFileNameWithoutExtension(Arg.Any<string>()).Returns("track");
        _mockBookRepository.GetExistingPathsAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyCollection<string>>([]));
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Created));
        _mockPathTemplateService.Parse(Arg.Any<LibraryType>(), Arg.Any<LibraryPathTemplate>(), Arg.Any<string>(), Arg.Any<char>())
            .Returns(Result.From(Optional<ParsedLibraryPath>.None()));

        _sut = new BooksMediaLibraryScanItemMaterializer(_mockPathTemplateService, _mockPathService);
    }

    [Fact]
    public async Task MaterializeItemsAsync_WhenPathSharesTheContentLocationPrefixButIsOutside_ShouldFallBackToTheFileNameRelativePath()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create(id: _libraryId, libraryType: LibraryType.Book, contentLocations: ["C:\\Music"]);
        _mockLibraryRepository.GetByIdAsync(_libraryId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockPathService.IsPathWithin("C:\\MusicBackup\\track.epub", "C:\\Music").Returns(false);
        _mockPathService.GetFileName("C:\\MusicBackup\\track.epub").Returns("track.epub");

        // Act
        Result<Success> result = await _sut.MaterializeItemsAsync(_mockUnitOfWork, _libraryId, _scanId, ["C:\\MusicBackup\\track.epub"], CancellationToken.None);

        // Assert
        // A sibling directory whose name only starts with the content location name is not inside the library, so the item is not parsed against
        // the path template, and the file name is used as the relative path instead.
        Assert.False(result.IsFailure);
        _mockPathService.Received(1).IsPathWithin("C:\\MusicBackup\\track.epub", "C:\\Music");
        _mockPathTemplateService.Received(1).Parse(Arg.Any<LibraryType>(), Arg.Any<LibraryPathTemplate>(), "track.epub", '\\');
    }

    [Fact]
    public async Task MaterializeItemsAsync_WhenPathIsInsideTheContentLocation_ShouldParseThePathRelativeToTheContentLocation()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create(id: _libraryId, libraryType: LibraryType.Book, contentLocations: ["C:\\Music"]);
        _mockLibraryRepository.GetByIdAsync(_libraryId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockPathService.IsPathWithin("C:\\Music\\Artist\\track.epub", "C:\\Music").Returns(true);

        // Act
        Result<Success> result = await _sut.MaterializeItemsAsync(_mockUnitOfWork, _libraryId, _scanId, ["C:\\Music\\Artist\\track.epub"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockPathService.Received(1).IsPathWithin("C:\\Music\\Artist\\track.epub", "C:\\Music");
        _mockPathTemplateService.Received(1).Parse(Arg.Any<LibraryType>(), Arg.Any<LibraryPathTemplate>(), "Artist\\track.epub", '\\');
    }
}
