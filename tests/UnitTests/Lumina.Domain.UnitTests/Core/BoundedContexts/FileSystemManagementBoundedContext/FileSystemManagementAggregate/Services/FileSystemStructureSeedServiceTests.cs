#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemStructureSeedService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemStructureSeedServiceTests
{
    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly IDirectoryProviderService _mockDirectoryProviderService;
    private readonly IPathService _mockPathService;
    private readonly FileSystemStructureSeedService _sut;
    private static readonly bool s_isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private readonly string _rootPath = s_isUnix ? "/Lumina" : @"C:\Lumina";

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemStructureSeedServiceTests"/> class.
    /// </summary>
    public FileSystemStructureSeedServiceTests()
    {
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockDirectoryProviderService = Substitute.For<IDirectoryProviderService>();
        _mockPathService = Substitute.For<IPathService>();
        _mockEnvironmentContext.DirectoryProviderService.Returns(_mockDirectoryProviderService);
        _sut = new FileSystemStructureSeedService(_mockEnvironmentContext, _mockPathService);
    }

    [Fact]
    public void SetDefaultDirectories_WhenDirectoriesDoNotExist_ShouldCreateThem()
    {
        // Arrange
        _mockPathService.CombinePath(_rootPath, "libraries").Returns(Path.Combine(_rootPath, "libraries"));
        _mockPathService.CombinePath(_rootPath, "books").Returns(Path.Combine(_rootPath, "books"));
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(false);
        _mockDirectoryProviderService.CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(callInfo => Result<FileSystemPathId>.Success(FileSystemPathId.Create(Path.Combine(_rootPath, callInfo.ArgAt<string>(1))).Value));

        // Act
        Result<Created> result = _sut.SetDefaultDirectories(_rootPath);

        // Assert
        Assert.True(result.IsSuccess);
        _mockDirectoryProviderService.Received(2).DirectoryExists(Arg.Any<FileSystemPathId>());
        _mockDirectoryProviderService.Received(2).CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>());
    }

    [Fact]
    public void SetDefaultDirectories_WhenDirectoriesAlreadyExist_ShouldNotCreateThem()
    {
        // Arrange
        _mockPathService.CombinePath(_rootPath, "libraries").Returns(Path.Combine(_rootPath, "libraries"));
        _mockPathService.CombinePath(_rootPath, "books").Returns(Path.Combine(_rootPath, "books"));
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(true);

        // Act
        Result<Created> result = _sut.SetDefaultDirectories(_rootPath);

        // Assert
        Assert.True(result.IsSuccess);
        _mockDirectoryProviderService.Received(2).DirectoryExists(Arg.Any<FileSystemPathId>());
        _mockDirectoryProviderService.DidNotReceive().CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>());
    }

    [Fact]
    public void SetDefaultDirectories_WhenRootPathIsInvalid_ShouldReturnInvalidPathError()
    {
        // Act
        Result<Created> result = _sut.SetDefaultDirectories("   ");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
        _mockPathService.DidNotReceive().CombinePath(Arg.Any<string>(), Arg.Any<string>());
        _mockDirectoryProviderService.DidNotReceive().DirectoryExists(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void SetDefaultDirectories_WhenCombiningPathFails_ShouldReturnTheError()
    {
        // Arrange
        _mockPathService.CombinePath(_rootPath, "libraries").Returns(Errors.FileSystemManagement.InvalidPath);

        // Act
        Result<Created> result = _sut.SetDefaultDirectories(_rootPath);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
        _mockDirectoryProviderService.DidNotReceive().DirectoryExists(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void SetDefaultDirectories_WhenCheckingDirectoryExistenceFails_ShouldReturnTheError()
    {
        // Arrange
        _mockPathService.CombinePath(_rootPath, "libraries").Returns(Path.Combine(_rootPath, "libraries"));
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(Errors.FileSystemManagement.DirectoryNotFound);

        // Act
        Result<Created> result = _sut.SetDefaultDirectories(_rootPath);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.DirectoryNotFound, result.FirstError);
        _mockDirectoryProviderService.DidNotReceive().CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>());
    }

    [Fact]
    public void SetDefaultDirectories_WhenCreatingDirectoryFails_ShouldReturnTheError()
    {
        // Arrange
        _mockPathService.CombinePath(_rootPath, "libraries").Returns(Path.Combine(_rootPath, "libraries"));
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(false);
        _mockDirectoryProviderService.CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(Errors.FileSystemManagement.DirectoryAlreadyExists);

        // Act
        Result<Created> result = _sut.SetDefaultDirectories(_rootPath);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.DirectoryAlreadyExists, result.FirstError);
    }
}
