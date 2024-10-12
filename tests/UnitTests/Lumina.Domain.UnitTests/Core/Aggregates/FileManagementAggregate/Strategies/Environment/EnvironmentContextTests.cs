#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Environment;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Strategies.Environment;

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
        environmentContext.DirectoryProviderService.Should().Be(_mockDirectoryProviderService);
        environmentContext.FileProviderService.Should().Be(_mockFileProviderService);
        environmentContext.FileTypeService.Should().Be(_mockFileTypeService);
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
        result.Should().Be(_mockFileTypeService);
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
        result.Should().Be(_mockFileProviderService);
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
        result.Should().Be(_mockDirectoryProviderService);
    }
}
