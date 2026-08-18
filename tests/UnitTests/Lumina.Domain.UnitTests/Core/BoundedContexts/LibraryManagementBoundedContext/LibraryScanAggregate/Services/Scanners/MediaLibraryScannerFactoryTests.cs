#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Scanners;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScannerFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScannerFactoryTests
{
    [Fact]
    public void CreateLibraryScanner_WhenLibraryTypeIsBook_ShouldReturnBookLibraryTypeScanner()
    {
        // Arrange
        IBookLibraryTypeScanner expectedScanner = Substitute.For<IBookLibraryTypeScanner>();
        ServiceCollection services = new();
        services.AddSingleton(expectedScanner);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        MediaLibraryScannerFactory sut = new(serviceProvider);

        // Act
        IMediaTypeScanner scanner = sut.CreateLibraryScanner(LibraryType.Book);

        // Assert
        Assert.Same(expectedScanner, scanner);
    }

    [Theory]
    [InlineData(LibraryType.EBook)] // eBook scanner is not implemented
    [InlineData(LibraryType.Movie)] // movie scanner is not implemented
    [InlineData(LibraryType.Music)] // music scanner is not implemented
    [InlineData(LibraryType.Photo)] // photo scanner is not implemented
    public void CreateLibraryScanner_WhenLibraryTypeIsNotImplemented_ShouldThrowNotImplementedException(LibraryType libraryType)
    {
        // Arrange
        ServiceCollection services = new();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        MediaLibraryScannerFactory sut = new(serviceProvider);

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => sut.CreateLibraryScanner(libraryType));
    }

    [Fact]
    public void CreateLibraryScanner_WhenLibraryTypeIsUnknown_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        ServiceCollection services = new();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        MediaLibraryScannerFactory sut = new(serviceProvider);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.CreateLibraryScanner((LibraryType)999));
    }
}
