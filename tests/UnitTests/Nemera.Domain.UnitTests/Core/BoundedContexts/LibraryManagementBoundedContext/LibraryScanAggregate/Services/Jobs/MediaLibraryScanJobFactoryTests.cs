#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanJobFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanJobFactoryTests
{
    private readonly LibraryIdFixture _libraryIdFixture = new();

    [Fact]
    public void CreateJob_WhenJobTypeIsRegistered_ShouldResolveJobAndSetLibraryId()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        IMediaLibraryScanJob expectedJob = Substitute.For<IMediaLibraryScanJob>();
        ServiceCollection services = new();
        services.AddSingleton(expectedJob);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        MediaLibraryScanJobFactory sut = new(serviceProvider);

        // Act
        IMediaLibraryScanJob job = sut.CreateJob<IMediaLibraryScanJob>(libraryId);

        // Assert
        Assert.Same(expectedJob, job);
        Assert.Equal(libraryId, job.LibraryId);
    }

    [Fact]
    public void CreateJob_WhenJobTypeIsNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        MediaLibraryScanJobFactory sut = new(serviceProvider);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sut.CreateJob<IMediaLibraryScanJob>(_libraryIdFixture.Create()));
    }

    [Fact]
    public void CreateJob_WhenServiceProviderReturnsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        MediaLibraryScanJobFactory sut = new(new NullServiceProvider());

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => sut.CreateJob<IMediaLibraryScanJob>(_libraryIdFixture.Create()));
        Assert.Contains("IMediaLibraryScanJob", exception.Message);
    }

    /// <summary>
    /// Test service provider that returns <see langword="null"/> for every requested service.
    /// </summary>
    private sealed class NullServiceProvider : IServiceProvider
    {
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of service to get.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? GetService(Type serviceType)
        {
            return null;
        }
    }
}
