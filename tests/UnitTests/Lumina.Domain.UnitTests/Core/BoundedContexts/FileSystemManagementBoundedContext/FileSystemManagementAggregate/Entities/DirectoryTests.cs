#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
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
/// Contains unit tests for the <see cref="Directory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryTests
{
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly DirectoryFixture _directoryFixture = new();
    private readonly FileFixture _fileFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidParameters_ShouldReturnSuccessfulResult()
    {
        // Arrange
        string path = "/valid/path";
        string name = "TestDirectory";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;

        // Act
        Result<Directory> result = Directory.Create(path, name, dateCreated, dateModified);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(dateCreated, result.Value.DateCreated);
        Assert.Equal(dateModified, result.Value.DateModified);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void Create_WhenCalledWithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = "";
        string name = "TestDirectory";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;

        // Act
        Result<Directory> result = Directory.Create(invalidPath, name, dateCreated, dateModified);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void Create_WhenCalledWithFileSystemPathId_ShouldReturnSuccessfulResult()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path: "/valid/path");
        string name = "TestDirectory";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;

        // Act
        Result<Directory> result = Directory.Create(pathId, name, dateCreated, dateModified);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(pathId, result.Value.Id);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(dateCreated, result.Value.DateCreated);
        Assert.Equal(dateModified, result.Value.DateModified);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void UpdateLastModified_WhenCalled_ShouldUpdateDateModified()
    {
        // Arrange
        Directory directory = _directoryFixture.Create(
            path: "/valid/path",
            name: "TestDirectory",
            dateCreated: Optional<DateTime>.None(),
            dateModified: Optional<DateTime>.None());
        DateTime newDate = DateTime.Now;

        // Act
        Result<Updated> result = directory.UpdateLastModified(newDate);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(newDate, directory.DateModified.Value);
    }

    [Fact]
    public void AddItem_WhenCalled_ShouldAddItemToCollection()
    {
        // Arrange
        Directory directory = _directoryFixture.Create(
            path: "/valid/path",
            name: "TestDirectory",
            dateCreated: Optional<DateTime>.None(),
            dateModified: Optional<DateTime>.None());
        File item = _fileFixture.Create(path: "/mock/path", name: "MockItem");

        // Act
        Result<Updated> result = directory.AddItem(item);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Contains(item, directory.Items);
    }

    [Fact]
    public void RemoveItem_WhenCalledWithExistingItem_ShouldRemoveItemFromCollection()
    {
        // Arrange
        Directory directory = _directoryFixture.Create(
            path: "/valid/path",
            name: "TestDirectory",
            dateCreated: Optional<DateTime>.None(),
            dateModified: Optional<DateTime>.None());
        File item = _fileFixture.Create(path: "/mock/path", name: "MockItem");
        directory.AddItem(item);

        // Act
        Result<Updated> result = directory.RemoveItem(item);

        // Assert
        Assert.False(result.IsFailure);
        Assert.DoesNotContain(item, directory.Items);
    }
}
