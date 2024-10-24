#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectoryTree;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Queries.GetDirectoryTree.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Queries.GetDirectoryTree;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoryTreeQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoryTreeQueryHandlerTests
{
    private readonly IDriveService _mockDriveService;
    private readonly IDirectoryService _mockDirectoryService;
    private readonly IFileService _mockFileService;
    private readonly IPathService _mockPathService;
    private readonly GetDirectoryTreeQueryHandler _sut;
    private readonly DirectoryFixture _directoryFixture;
    private readonly PathSegmentFixture _pathSegmentFixture;
    private readonly FileFixture _fileFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoryTreeQueryHandlerTests"/> class.
    /// </summary>
    public GetDirectoryTreeQueryHandlerTests()
    {
        _mockDriveService = Substitute.For<IDriveService>();
        _mockDirectoryService = Substitute.For<IDirectoryService>();
        _mockFileService = Substitute.For<IFileService>();
        _mockPathService = Substitute.For<IPathService>();
        _sut = new GetDirectoryTreeQueryHandler(_mockDriveService, _mockDirectoryService, _mockFileService, _mockPathService);
        _directoryFixture = new DirectoryFixture();
        _pathSegmentFixture = new PathSegmentFixture();
        _fileFixture = new FileFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryAndNoIncludeFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery(false, true);

        // Create a drive that matches what the implementation expects
        string drivePath = "C:\\";
        IEnumerable<FileSystemItem> drives =
        [
            WindowsRootItem.Create(drivePath, "C:").Value
        ];

        // Ensure the first path segment matches the drive path
        PathSegment firstPathSegment = _pathSegmentFixture.CreatePathSegment(name: "C:", isDrive: true);
        List<PathSegment> otherPathSegments = _pathSegmentFixture.CreateMany(2);
        IEnumerable<PathSegment> pathSegments = new[] { firstPathSegment }.Concat(otherPathSegments);

        IEnumerable<Directory> subdirectories = _directoryFixture.CreateMany(2);
        IEnumerable<File> files = _fileFixture.CreateMany(2);

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.ParsePath(query.Path!).Returns(ErrorOrFactory.From(pathSegments));
        _mockPathService.CombinePath(Arg.Is<string>(x => true), Arg.Is<string>(y => true)).Returns(callInfo => $"{callInfo.ArgAt<string>(0)}\\{callInfo.ArgAt<string>(1)}");
        _mockDirectoryService.GetSubdirectories(Arg.Any<string>(), query.IncludeHiddenElements).Returns(ErrorOrFactory.From(subdirectories));
        _mockFileService.GetFiles(Arg.Any<string>(), query.IncludeHiddenElements).Returns(ErrorOrFactory.From(files));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeEmpty();
        result.Value.First().Children.Should().NotBeEmpty();
        _mockDriveService.Received(1).GetDrives();
        _mockPathService.Received(1).ParsePath(query.Path!);
        _mockDirectoryService.Received(1).GetSubdirectories(Arg.Any<string>(), query.IncludeHiddenElements);
        _mockFileService.Received(0).GetFiles(Arg.Any<string>(), query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQueryAndIncludeFiles_ShouldReturnSuccessResult()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery(true, true);

        // Create a drive that matches what the implementation expects
        string drivePath = "C:\\";
        IEnumerable<FileSystemItem> drives =
        [
            WindowsRootItem.Create(drivePath, "C:").Value
        ];

        // Ensure the first path segment matches the drive path
        PathSegment firstPathSegment = _pathSegmentFixture.CreatePathSegment(name: "C:", isDrive: true);
        List<PathSegment> otherPathSegments = _pathSegmentFixture.CreateMany(2);
        IEnumerable<PathSegment> pathSegments = new[] { firstPathSegment }.Concat(otherPathSegments);

        IEnumerable<Directory> subdirectories = _directoryFixture.CreateMany(2);
        IEnumerable<File> files = _fileFixture.CreateMany(2);

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.ParsePath(query.Path!).Returns(ErrorOrFactory.From(pathSegments));
        _mockPathService.CombinePath(Arg.Is<string>(x => true), Arg.Is<string>(y => true)).Returns(callInfo => $"{callInfo.ArgAt<string>(0)}\\{callInfo.ArgAt<string>(1)}");
        _mockDirectoryService.GetSubdirectories(Arg.Any<string>(), query.IncludeHiddenElements).Returns(ErrorOrFactory.From(subdirectories));
        _mockFileService.GetFiles(Arg.Any<string>(), query.IncludeHiddenElements).Returns(ErrorOrFactory.From(files));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeEmpty();
        result.Value.First().Children.Should().NotBeEmpty();
        _mockDriveService.Received(1).GetDrives();
        _mockPathService.Received(1).ParsePath(query.Path!);
        _mockDirectoryService.Received(1).GetSubdirectories(Arg.Any<string>(), query.IncludeHiddenElements);
        _mockFileService.Received(1).GetFiles(Arg.Any<string>(), query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenGetDrivesReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        Error error = Error.Failure("DriveService.Error", "An error occurred");
        _mockDriveService.GetDrives().Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockDriveService.Received(1).GetDrives();
    }

    [Fact]
    public async Task Handle_WhenParsePathReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        IEnumerable<FileSystemItem> drives =
        [
            WindowsRootItem.Create("C:\\", "Local Disk").Value
        ];
        Error error = Error.Failure("PathService.Error", "An error occurred");

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.ParsePath(query.Path!).Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockDriveService.Received(1).GetDrives();
        _mockPathService.Received(1).ParsePath(query.Path!);
    }

    [Fact]
    public async Task Handle_WhenCombinePathReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        IEnumerable<FileSystemItem> drives =
        [
            WindowsRootItem.Create("C:\\", "C:").Value
        ];
        PathSegment firstPathSegment = _pathSegmentFixture.CreatePathSegment(name: "C:", isDrive: true);
        IEnumerable<PathSegment> pathSegments = new[] { firstPathSegment }.Concat(_pathSegmentFixture.CreateMany(2));
        Error error = Error.Failure("PathService.Error", "An error occurred");

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.ParsePath(query.Path!).Returns(ErrorOrFactory.From(pathSegments));
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>()).Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockDriveService.Received(1).GetDrives();
        _mockPathService.Received(1).ParsePath(query.Path!);
        _mockPathService.Received(1).CombinePath(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenGetSubdirectoriesReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        IEnumerable<FileSystemItem> drives =
        [
            WindowsRootItem.Create("C:\\", "C:").Value
        ];
        PathSegment firstPathSegment = _pathSegmentFixture.CreatePathSegment(name: "C:", isDrive: true);
        IEnumerable<PathSegment> pathSegments = new[] { firstPathSegment }.Concat(_pathSegmentFixture.CreateMany(2));
        Error error = Error.Failure("DirectoryService.Error", "An error occurred");

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.ParsePath(query.Path!).Returns(ErrorOrFactory.From(pathSegments));
        _mockPathService.CombinePath(Arg.Is<string>(x => true), Arg.Is<string>(y => true)).Returns(callInfo => $"{callInfo.ArgAt<string>(0)}\\{callInfo.ArgAt<string>(1)}");
        _mockDirectoryService.GetSubdirectories(Arg.Any<string>(), query.IncludeHiddenElements).Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockDriveService.Received(1).GetDrives();
        _mockPathService.Received(1).ParsePath(query.Path!);
        _mockDirectoryService.Received(1).GetSubdirectories(Arg.Any<string>(), query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenGetFilesReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery(true);
        IEnumerable<FileSystemItem> drives =
        [
            WindowsRootItem.Create("C:\\", "C:").Value
        ];
        PathSegment firstPathSegment = _pathSegmentFixture.CreatePathSegment(name: "C:", isDrive: true);
        IEnumerable<PathSegment> pathSegments = new[] { firstPathSegment }.Concat(_pathSegmentFixture.CreateMany(2));
        IEnumerable<Directory> subdirectories = _directoryFixture.CreateMany(2);
        Error error = Error.Failure("FileService.Error", "An error occurred");

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.ParsePath(query.Path!).Returns(ErrorOrFactory.From(pathSegments));
        _mockPathService.CombinePath(Arg.Is<string>(x => true), Arg.Is<string>(y => true)).Returns(callInfo => $"{callInfo.ArgAt<string>(0)}\\{callInfo.ArgAt<string>(1)}");
        _mockDirectoryService.GetSubdirectories(Arg.Any<string>(), query.IncludeHiddenElements).Returns(ErrorOrFactory.From(subdirectories));
        _mockFileService.GetFiles(Arg.Any<string>(), query.IncludeHiddenElements).Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockDriveService.Received(1).GetDrives();
        _mockPathService.Received(1).ParsePath(query.Path!);
        _mockDirectoryService.Received(1).GetSubdirectories(Arg.Any<string>(), query.IncludeHiddenElements);
        _mockFileService.Received(1).GetFiles(Arg.Any<string>(), query.IncludeHiddenElements);
    }

    [Fact]
    public async Task Handle_WhenPathOnlyContainsRoot_ShouldReturnOnlyRootNode()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        query = query with { Path = "C:" };
        IEnumerable<FileSystemItem> drives = [WindowsRootItem.Create("C:\\", "C:").Value];
        PathSegment rootSegment = _pathSegmentFixture.CreatePathSegment(name: "C:", isDrive: true);

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.ParsePath(query.Path).Returns(ErrorOrFactory.From((IEnumerable<PathSegment>)[rootSegment]));
        _mockDirectoryService.GetSubdirectories(Arg.Any<string>(), Arg.Any<bool>())
          .Returns(ErrorOrFactory.From(Enumerable.Empty<Directory>()));
        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.First().Name.Should().Be("C:");
        result.Value.First().Children.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPathHasManySegments_ShouldCreateCorrectTreeStructure()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        IEnumerable<FileSystemItem> drives = [WindowsRootItem.Create("C:\\", "C:").Value];
        List<PathSegment> pathSegments =
        [
            _pathSegmentFixture.CreatePathSegment(name: "C:", isDrive: true),
            _pathSegmentFixture.CreatePathSegment(name: "Users"),
            _pathSegmentFixture.CreatePathSegment(name: "Username"),
            _pathSegmentFixture.CreatePathSegment(name: "Documents"),
            _pathSegmentFixture.CreatePathSegment(name: "Projects"),
            _pathSegmentFixture.CreatePathSegment(name: "MyProject")
        ];

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.ParsePath(query.Path!).Returns(ErrorOrFactory.From((IEnumerable<PathSegment>)pathSegments));
        _mockPathService.Exists(Arg.Any<string>(), Arg.Any<bool>()).Returns(true);
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => $"{callInfo.ArgAt<string>(0)}\\{callInfo.ArgAt<string>(1)}");

        _mockDirectoryService.GetSubdirectories(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(ErrorOrFactory.From(Enumerable.Empty<Directory>()));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        FileSystemTreeNodeResponse root = result.Value.First();
        root.Name.Should().Be("C:");
        root.Children.Should().HaveCount(1);
        root.Children.First().Name.Should().Be("Users");
        root.Children.First().Children.First().Name.Should().Be("Username");
        root.Children.First().Children.First().Children.First().Name.Should().Be("Documents");
        root.Children.First().Children.First().Children.First().Children.First().Name.Should().Be("Projects");
        root.Children.First().Children.First().Children.First().Children.First().Children.First().Name.Should().Be("MyProject");
    }

    [Fact]
    public async Task Handle_WhenPathContainsSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        IEnumerable<FileSystemItem> drives = [WindowsRootItem.Create("C:\\", "C:").Value];
        List<PathSegment> pathSegments =
        [
            _pathSegmentFixture.CreatePathSegment(name: "C:", isDrive: true),
            _pathSegmentFixture.CreatePathSegment(name: "Program Files"),
            _pathSegmentFixture.CreatePathSegment(name: "My Awesome App!"),
            _pathSegmentFixture.CreatePathSegment(name: "Data Files")
        ];

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.Exists(Arg.Any<string>(), Arg.Any<bool>()).Returns(true);
        _mockPathService.ParsePath(query.Path!).Returns(ErrorOrFactory.From((IEnumerable<PathSegment>)pathSegments));
        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => $"{callInfo.ArgAt<string>(0)}\\{callInfo.ArgAt<string>(1)}");
        _mockDirectoryService.GetSubdirectories(Arg.Any<string>(), Arg.Any<bool>())
          .Returns(ErrorOrFactory.From(Enumerable.Empty<Directory>()));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        FileSystemTreeNodeResponse root = result.Value.First();
        root.Children.First().Name.Should().Be("Program Files");
        root.Children.First().Children.First().Name.Should().Be("My Awesome App!");
    }

    [Fact]
    public async Task Handle_WhenIncludeHiddenElementsIsFalse_ShouldNotIncludeHiddenItems()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery(includeHiddenElements: false);
        IEnumerable<FileSystemItem> drives = [WindowsRootItem.Create("C:\\", "C:").Value];
        PathSegment rootSegment = _pathSegmentFixture.CreatePathSegment(name: "C:", isDrive: true);

        _mockDriveService.GetDrives().Returns(ErrorOrFactory.From(drives));
        _mockPathService.ParsePath(query.Path!).Returns(ErrorOrFactory.From((IEnumerable<PathSegment>)[rootSegment]));

        IEnumerable<Directory> visibleDirectories = _directoryFixture.CreateMany(2);
        _mockDirectoryService.GetSubdirectories(Arg.Any<string>(), false).Returns(ErrorOrFactory.From(visibleDirectories));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.First().Children.Should().HaveCount(2);
        _mockDirectoryService.Received(1).GetSubdirectories(Arg.Any<string>(), false);
    }
}
