#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="File"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileTests
{
    [Fact]
    public void Create_WhenCalledWithValidParameters_ShouldReturnSuccessfulResult()
    {
        // Arrange
        string path = "/valid/path/file.txt";
        string name = "file.txt";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;
        long size = 1024;

        // Act
        ErrorOr<File> result = File.Create(path, name, dateCreated, dateModified, size);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.DateCreated.Should().Be(dateCreated);
        result.Value.DateModified.Should().Be(dateModified);
        result.Value.Size.Should().Be(size);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void Create_WhenCalledWithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = "";
        string name = "file.txt";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;
        long size = 1024;

        // Act
        ErrorOr<File> result = File.Create(invalidPath, name, dateCreated, dateModified, size);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void Create_WhenCalledWithFileSystemPathId_ShouldReturnSuccessfulResult()
    {
        // Arrange
        ErrorOr<FileSystemPathId> pathIdResult = FileSystemPathId.Create("/valid/path/file.txt");
        pathIdResult.IsError.Should().BeFalse();
        FileSystemPathId pathId = pathIdResult.Value;
        string name = "file.txt";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;
        long size = 1024;

        // Act
        ErrorOr<File> result = File.Create(pathId, name, dateCreated, dateModified, size);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(pathId);
        result.Value.Name.Should().Be(name);
        result.Value.DateCreated.Should().Be(dateCreated);
        result.Value.DateModified.Should().Be(dateModified);
        result.Value.Size.Should().Be(size);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void UpdateLastModified_WhenCalled_ShouldUpdateDateModified()
    {
        // Arrange
        ErrorOr<File> fileResult = File.Create("/valid/path/file.txt", "file.txt", Optional<DateTime>.None(), Optional<DateTime>.None(), 1024);
        fileResult.IsError.Should().BeFalse();
        File file = fileResult.Value;
        DateTime newDate = DateTime.Now;

        // Act
        ErrorOr<Updated> result = file.UpdateLastModified(newDate);

        // Assert
        result.IsError.Should().BeFalse();
        file.DateModified.Value.Should().Be(newDate);
    }

    [Fact]
    public void UpdateSize_WhenCalled_ShouldUpdateFileSize()
    {
        // Arrange
        ErrorOr<File> fileResult = File.Create("/valid/path/file.txt", "file.txt", Optional<DateTime>.None(), Optional<DateTime>.None(), 1024);
        fileResult.IsError.Should().BeFalse();
        File file = fileResult.Value;
        long newSize = 2048;

        // Act
        ErrorOr<Updated> result = file.UpdateSize(newSize);

        // Assert
        result.IsError.Should().BeFalse();
        file.Size.Should().Be(newSize);
    }

    [Fact]
    public void Rename_WhenCalledWithValidName_ShouldUpdateFileName()
    {
        // Arrange
        ErrorOr<File> fileResult = File.Create("/valid/path/file.txt", "file.txt", Optional<DateTime>.None(), Optional<DateTime>.None(), 1024);
        fileResult.IsError.Should().BeFalse();
        File file = fileResult.Value;
        string newName = "newfile.txt";

        // Act
        ErrorOr<Updated> result = file.Rename(newName);

        // Assert
        result.IsError.Should().BeFalse();
        file.Name.Should().Be(newName);
    }

    [Fact]
    public void Rename_WhenCalledWithEmptyName_ShouldReturnError()
    {
        // Arrange
        ErrorOr<File> fileResult = File.Create("/valid/path/file.txt", "file.txt", Optional<DateTime>.None(), Optional<DateTime>.None(), 1024);
        fileResult.IsError.Should().BeFalse();
        File file = fileResult.Value;
        string emptyName = "";

        // Act
        ErrorOr<Updated> result = file.Rename(emptyName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.NameCannotBeEmpty);
        file.Name.Should().Be("file.txt");
    }
}
