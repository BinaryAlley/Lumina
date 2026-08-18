#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.Services;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.Services;
using Lumina.Domain.Core.Services;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Services;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryServiceTests
{
    [Fact]
    public void Constructor_WhenCalled_ShouldSetInjectedServices()
    {
        // Arrange
        IVideoLibraryService videoLibraryService = Substitute.For<IVideoLibraryService>();
        IWrittenContentLibraryService writtenContentLibraryService = Substitute.For<IWrittenContentLibraryService>();

        // Act
        MediaLibraryService sut = new(videoLibraryService, writtenContentLibraryService);

        // Assert
        Assert.Same(videoLibraryService, sut.VideoLibraryService);
        Assert.Same(writtenContentLibraryService, sut.WrittenContentLibraryService);
    }
}
