#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using System;
using System.Diagnostics.CodeAnalysis;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;
using Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Fixtures;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Directory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryTests
{
    [Fact]
    public void Create_WhenCalledWithValidParameters_ShouldReturnSuccessfulResult()
    {
        // Arrange
        string path = "/valid/path";
        string name = "TestDirectory";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;

        // Act
        ErrorOr<Directory> result = Directory.Create(path, name, dateCreated, dateModified);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.DateCreated.Should().Be(dateCreated);
        result.Value.DateModified.Should().Be(dateModified);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
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
        ErrorOr<Directory> result = Directory.Create(invalidPath, name, dateCreated, dateModified);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
    }

    [Fact]
    public void Create_WhenCalledWithFileSystemPathId_ShouldReturnSuccessfulResult()
    {
        // Arrange
        ErrorOr<FileSystemPathId> pathIdResult = FileSystemPathId.Create("/valid/path");
        pathIdResult.IsError.Should().BeFalse();
        FileSystemPathId pathId = pathIdResult.Value;
        string name = "TestDirectory";
        Optional<DateTime> dateCreated = DateTime.Now;
        Optional<DateTime> dateModified = DateTime.Now;

        // Act
        ErrorOr<Directory> result = Directory.Create(pathId, name, dateCreated, dateModified);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(pathId);
        result.Value.Name.Should().Be(name);
        result.Value.DateCreated.Should().Be(dateCreated);
        result.Value.DateModified.Should().Be(dateModified);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void UpdateLastModified_WhenCalled_ShouldUpdateDateModified()
    {
        // Arrange
        ErrorOr<Directory> directoryResult = Directory.Create("/valid/path", "TestDirectory", Optional<DateTime>.None(), Optional<DateTime>.None());
        directoryResult.IsError.Should().BeFalse();
        Directory directory = directoryResult.Value;
        DateTime newDate = DateTime.Now;

        // Act
        ErrorOr<Updated> result = directory.UpdateLastModified(newDate);

        // Assert
        result.IsError.Should().BeFalse();
        directory.DateModified.Value.Should().Be(newDate);
    }

    [Fact]
    public void AddItem_WhenCalled_ShouldAddItemToCollection()
    {
        // Arrange
        ErrorOr<Directory> directoryResult = Directory.Create("/valid/path", "TestDirectory", Optional<DateTime>.None(), Optional<DateTime>.None());
        directoryResult.IsError.Should().BeFalse();
        Directory directory = directoryResult.Value;
        FileSystemItem item = new FileSystemItemFixture(FileSystemPathId.Create("/mock/path").Value, "MockItem", FileSystemItemType.File);

        // Act
        ErrorOr<Updated> result = directory.AddItem(item);

        // Assert
        result.IsError.Should().BeFalse();
        directory.Items.Should().Contain(item);
    }

    [Fact]
    public void RemoveItem_WhenCalledWithExistingItem_ShouldRemoveItemFromCollection()
    {
        // Arrange
        ErrorOr<Directory> directoryResult = Directory.Create("/valid/path", "TestDirectory", Optional<DateTime>.None(), Optional<DateTime>.None());
        directoryResult.IsError.Should().BeFalse();
        Directory directory = directoryResult.Value;
        FileSystemItem item = new FileSystemItemFixture(FileSystemPathId.Create("/mock/path").Value, "MockItem", FileSystemItemType.File);
        directory.AddItem(item);

        // Act
        ErrorOr<Updated> result = directory.RemoveItem(item);

        // Assert
        result.IsError.Should().BeFalse();
        directory.Items.Should().NotContain(item);
    }
}
