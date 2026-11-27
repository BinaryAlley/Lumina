#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Reading;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.UnitTests.Core.Reading;

/// <summary>
/// Contains unit tests for the <see cref="IBookReader"/> interface.
/// </summary>
[ExcludeFromCodeCoverage]
public class IBookReaderGenericTests
{
    private readonly ReadingDocumentDtoFixture _readingDocumentDtoFixture = new();

    [Fact]
    public async Task OpenAsync_WhenCalled_ShouldForwardArgumentsToTheReaderImplementation()
    {
        // Arrange
        string path = "/books/test.epub";
        string workingDirectory = "/tmp/reading-cache";
        bool shouldRenderPdfAsImages = true;
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingDocumentDto expectedDocument = _readingDocumentDtoFixture.Create();
        TestBookReader reader = new(expectedDocument, [1, 2, 3]);

        // Act
        ReadingDocumentDto result = await reader.OpenAsync(path, workingDirectory, shouldRenderPdfAsImages, cancellationToken);

        // Assert
        Assert.Equal(expectedDocument, result);
        Assert.Equal(path, reader.ReceivedPath);
        Assert.Equal(workingDirectory, reader.ReceivedWorkingDirectory);
        Assert.Equal(shouldRenderPdfAsImages, reader.ReceivedShouldRenderPdfAsImages);
        Assert.Equal(cancellationToken, reader.ReceivedCancellationToken);
    }

    [Fact]
    public async Task GetResourceAsync_WhenCalled_ShouldForwardArgumentsToTheReaderImplementation()
    {
        // Arrange
        string path = "/books/test.epub";
        string workingDirectory = "/tmp/reading-cache";
        string resourceKey = "resources/cover.png";
        CancellationToken cancellationToken = CancellationToken.None;
        byte[] expectedResource = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        TestBookReader reader = new(_readingDocumentDtoFixture.Create(), expectedResource);

        // Act
        byte[] result = await reader.GetResourceAsync(path, workingDirectory, resourceKey, cancellationToken);

        // Assert
        Assert.Equal(expectedResource, result);
        Assert.Equal(path, reader.ReceivedResourcePath);
        Assert.Equal(workingDirectory, reader.ReceivedResourceWorkingDirectory);
        Assert.Equal(resourceKey, reader.ReceivedResourceKey);
        Assert.Equal(cancellationToken, reader.ReceivedResourceCancellationToken);
    }

    /// <summary>
    /// Test double for the book reader interface that records the forwarded arguments.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class TestBookReader : IBookReader
    {
        private readonly ReadingDocumentDto _document;
        private readonly byte[] _resource;

        /// <summary>
        /// Gets the path that was forwarded to the OpenAsync implementation.
        /// </summary>
        public string? ReceivedPath { get; private set; }

        /// <summary>
        /// Gets the working directory that was forwarded to the OpenAsync implementation.
        /// </summary>
        public string? ReceivedWorkingDirectory { get; private set; }

        /// <summary>
        /// Gets the rendering preference that was forwarded to the OpenAsync implementation.
        /// </summary>
        public bool ReceivedShouldRenderPdfAsImages { get; private set; }

        /// <summary>
        /// Gets the cancellation token that was forwarded to the OpenAsync implementation.
        /// </summary>
        public CancellationToken ReceivedCancellationToken { get; private set; }

        /// <summary>
        /// Gets the path that was forwarded to the GetResourceAsync implementation.
        /// </summary>
        public string? ReceivedResourcePath { get; private set; }

        /// <summary>
        /// Gets the working directory that was forwarded to the GetResourceAsync implementation.
        /// </summary>
        public string? ReceivedResourceWorkingDirectory { get; private set; }

        /// <summary>
        /// Gets the resource key that was forwarded to the GetResourceAsync implementation.
        /// </summary>
        public string? ReceivedResourceKey { get; private set; }

        /// <summary>
        /// Gets the cancellation token that was forwarded to the GetResourceAsync implementation.
        /// </summary>
        public CancellationToken ReceivedResourceCancellationToken { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestBookReader"/> class.
        /// </summary>
        /// <param name="document">The document returned by the OpenAsync implementation.</param>
        /// <param name="resource">The resource data returned by the GetResourceAsync implementation.</param>
        public TestBookReader(ReadingDocumentDto document, byte[] resource)
        {
            _document = document;
            _resource = resource;
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> SupportedExtensions => [".epub"];

        /// <inheritdoc/>
        public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.EBook];

        /// <inheritdoc/>
        public Task<ReadingDocumentDto> OpenAsync(string path, string workingDirectory, bool shouldRenderPdfAsImages, CancellationToken cancellationToken)
        {
            ReceivedPath = path;
            ReceivedWorkingDirectory = workingDirectory;
            ReceivedShouldRenderPdfAsImages = shouldRenderPdfAsImages;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(_document);
        }

        /// <inheritdoc/>
        public Task<byte[]> GetResourceAsync(string path, string workingDirectory, string resourceKey, CancellationToken cancellationToken)
        {
            ReceivedResourcePath = path;
            ReceivedResourceWorkingDirectory = workingDirectory;
            ReceivedResourceKey = resourceKey;
            ReceivedResourceCancellationToken = cancellationToken;
            return Task.FromResult(_resource);
        }
    }
}
