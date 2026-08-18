#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="File"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileTests
{
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly FileFixture _fileFixture = new();

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
        Result<File> result = File.Create(path, name, dateCreated, dateModified, size);

        // Assert
        Assert.False(result.IsFailure);
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
        Result<File> result = File.Create(invalidPath, name, dateCreated, dateModified, size);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void Create_WhenCalledWithFileSystemPathId_ShouldReturnSuccessfulResult()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path: "/valid/path/file.txt");
        string name = "file.txt";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;
        long size = 1024;

        // Act
        Result<File> result = File.Create(pathId, name, dateCreated, dateModified, size);

        // Assert
        Assert.False(result.IsFailure);
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
        File file = _fileFixture.Create(
            path: "/valid/path/file.txt",
            name: "file.txt",
            dateCreated: Optional<DateTime>.None(),
            dateModified: Optional<DateTime>.None(),
            size: 1024);
        DateTime newDate = DateTime.Now;

        // Act
        Result<Updated> result = file.UpdateLastModified(newDate);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(newDate, file.DateModified.Value);
    }

    [Fact]
    public void UpdateSize_WhenCalled_ShouldUpdateFileSize()
    {
        // Arrange
        File file = _fileFixture.Create(
            path: "/valid/path/file.txt",
            name: "file.txt",
            dateCreated: Optional<DateTime>.None(),
            dateModified: Optional<DateTime>.None(),
            size: 1024);
        long newSize = 2048;

        // Act
        Result<Updated> result = file.UpdateSize(newSize);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(newSize, file.Size);
    }

    [Fact]
    public void Rename_WhenCalledWithValidName_ShouldUpdateFileName()
    {
        // Arrange
        File file = _fileFixture.Create(
            path: "/valid/path/file.txt",
            name: "file.txt",
            dateCreated: Optional<DateTime>.None(),
            dateModified: Optional<DateTime>.None(),
            size: 1024);
        string newName = "newfile.txt";

        // Act
        Result<Updated> result = file.Rename(newName);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(newName, file.Name);
    }

    [Fact]
    public void Rename_WhenCalledWithEmptyName_ShouldReturnError()
    {
        // Arrange
        File file = _fileFixture.Create(
            path: "/valid/path/file.txt",
            name: "file.txt",
            dateCreated: Optional<DateTime>.None(),
            dateModified: Optional<DateTime>.None(),
            size: 1024);
        string emptyName = "";

        // Act
        Result<Updated> result = file.Rename(emptyName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.NameCannotBeEmpty, result.FirstError);
        Assert.Equal("file.txt", file.Name);
    }
}
