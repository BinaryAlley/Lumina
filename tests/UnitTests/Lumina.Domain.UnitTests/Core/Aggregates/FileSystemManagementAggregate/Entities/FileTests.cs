#region ========================================================================= USING =====================================================================================
using ErrorOr;
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
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(dateCreated, result.Value.DateCreated);
        Assert.Equal(dateModified, result.Value.DateModified);
        Assert.Equal(size, result.Value.Size);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
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
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void Create_WhenCalledWithFileSystemPathId_ShouldReturnSuccessfulResult()
    {
        // Arrange
        ErrorOr<FileSystemPathId> pathIdResult = FileSystemPathId.Create("/valid/path/file.txt");
        Assert.False(pathIdResult.IsError);
        FileSystemPathId pathId = pathIdResult.Value;
        string name = "file.txt";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;
        long size = 1024;

        // Act
        ErrorOr<File> result = File.Create(pathId, name, dateCreated, dateModified, size);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(pathId, result.Value.Id);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(dateCreated, result.Value.DateCreated);
        Assert.Equal(dateModified, result.Value.DateModified);
        Assert.Equal(size, result.Value.Size);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void UpdateLastModified_WhenCalled_ShouldUpdateDateModified()
    {
        // Arrange
        ErrorOr<File> fileResult = File.Create("/valid/path/file.txt", "file.txt", Optional<DateTime>.None(), Optional<DateTime>.None(), 1024);
        Assert.False(fileResult.IsError);
        File file = fileResult.Value;
        DateTime newDate = DateTime.Now;

        // Act
        ErrorOr<Updated> result = file.UpdateLastModified(newDate);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newDate, file.DateModified.Value);
    }

    [Fact]
    public void UpdateSize_WhenCalled_ShouldUpdateFileSize()
    {
        // Arrange
        ErrorOr<File> fileResult = File.Create("/valid/path/file.txt", "file.txt", Optional<DateTime>.None(), Optional<DateTime>.None(), 1024);
        Assert.False(fileResult.IsError);
        File file = fileResult.Value;
        long newSize = 2048;

        // Act
        ErrorOr<Updated> result = file.UpdateSize(newSize);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newSize, file.Size);
    }

    [Fact]
    public void Rename_WhenCalledWithValidName_ShouldUpdateFileName()
    {
        // Arrange
        ErrorOr<File> fileResult = File.Create("/valid/path/file.txt", "file.txt", Optional<DateTime>.None(), Optional<DateTime>.None(), 1024);
        Assert.False(fileResult.IsError);
        File file = fileResult.Value;
        string newName = "newfile.txt";

        // Act
        ErrorOr<Updated> result = file.Rename(newName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newName, file.Name);
    }

    [Fact]
    public void Rename_WhenCalledWithEmptyName_ShouldReturnError()
    {
        // Arrange
        ErrorOr<File> fileResult = File.Create("/valid/path/file.txt", "file.txt", Optional<DateTime>.None(), Optional<DateTime>.None(), 1024);
        Assert.False(fileResult.IsError);
        File file = fileResult.Value;
        string emptyName = "";

        // Act
        ErrorOr<Updated> result = file.Rename(emptyName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.NameCannotBeEmpty, result.FirstError);
        Assert.Equal("file.txt", file.Name);
    }
}
