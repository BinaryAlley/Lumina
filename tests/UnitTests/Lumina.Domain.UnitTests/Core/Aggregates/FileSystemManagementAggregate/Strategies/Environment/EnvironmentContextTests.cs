#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Strategies.Environment;

/// <summary>
/// Contains unit tests for the <see cref="EnvironmentContext"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EnvironmentContextTests
{
    private readonly IDirectoryProviderService _mockDirectoryProviderService;
    private readonly IFileProviderService _mockFileProviderService;
    private readonly IFileTypeService _mockFileTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentContextTests"/> class.
    /// </summary>
    public EnvironmentContextTests()
    {
        _mockDirectoryProviderService = Substitute.For<IDirectoryProviderService>();
        _mockFileProviderService = Substitute.For<IFileProviderService>();
        _mockFileTypeService = Substitute.For<IFileTypeService>();
    }

    [Fact]
    public void Constructor_WhenCalledWithValidParameters_ShouldInitializeAllProperties()
    {
        // Arrange & Act
        EnvironmentContext environmentContext = new(
            _mockDirectoryProviderService,
            _mockFileProviderService,
            _mockFileTypeService);

        // Assert
        Assert.Same(_mockDirectoryProviderService, environmentContext.DirectoryProviderService);
        Assert.Same(_mockFileProviderService, environmentContext.FileProviderService);
        Assert.Same(_mockFileTypeService, environmentContext.FileTypeService);
    }

    [Fact]
    public void FileTypeService_WhenAccessed_ShouldReturnInjectedService()
    {
        // Arrange
        EnvironmentContext environmentContext = new(
            _mockDirectoryProviderService,
            _mockFileProviderService,
            _mockFileTypeService);

        // Act
        IFileTypeService result = environmentContext.FileTypeService;

        // Assert
        Assert.Same(_mockFileTypeService, result);
    }

    [Fact]
    public void FileProviderService_WhenAccessed_ShouldReturnInjectedService()
    {
        // Arrange
        EnvironmentContext environmentContext = new(
            _mockDirectoryProviderService,
            _mockFileProviderService,
            _mockFileTypeService);

        // Act
        IFileProviderService result = environmentContext.FileProviderService;

        // Assert
        Assert.Same(_mockFileProviderService, result);
    }

    [Fact]
    public void DirectoryProviderService_WhenAccessed_ShouldReturnInjectedService()
    {
        // Arrange
        EnvironmentContext environmentContext = new(
            _mockDirectoryProviderService,
            _mockFileProviderService,
            _mockFileTypeService);

        // Act
        IDirectoryProviderService result = environmentContext.DirectoryProviderService;

        // Assert
        Assert.Same(_mockDirectoryProviderService, result);
    }
}
