#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.Services;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.Services;

/// <summary>
/// Contains unit tests for the <see cref="WrittenContentLibraryService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentLibraryServiceTests
{
    [Fact]
    public void Constructor_WhenCalled_ShouldSetInjectedService()
    {
        // Arrange
        IBookLibraryService bookLibraryService = Substitute.For<IBookLibraryService>();

        // Act
        WrittenContentLibraryService sut = new(bookLibraryService);

        // Assert
        Assert.Same(bookLibraryService, sut.BookLibraryService);
    }
}
