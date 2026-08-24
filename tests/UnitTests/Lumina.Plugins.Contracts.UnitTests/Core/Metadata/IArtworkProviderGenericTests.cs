#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.UnitTests.Core.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="IArtworkProvider{TLookup}"/> interface.
/// </summary>
[ExcludeFromCodeCoverage]
public class IArtworkProviderGenericTests
{
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();
    private readonly ArtworkDtoFixture _artworkDtoFixture = new();
    private readonly OtherMetadataLookupDtoFixture _otherMetadataLookupDtoFixture = new();

    [Fact]
    public async Task GetArtworkAsync_WhenCalledThroughBaseInterface_ShouldForwardToTypedImplementation()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ArtworkDto expectedArtwork = _artworkDtoFixture.Create();
        TestBookArtworkProvider provider = new(expectedArtwork);
        IArtworkProvider baseProvider = provider;

        // Act
        ArtworkDto? result = await baseProvider.GetArtworkAsync(lookup, cancellationToken);

        // Assert
        Assert.Equal(expectedArtwork, result);
        Assert.Same(lookup, provider.ReceivedLookup);
        Assert.Equal(cancellationToken, provider.ReceivedCancellationToken);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenLookupIsOfAnotherRuntimeType_ShouldReturnNull()
    {
        // Arrange
        MetadataLookupDto otherLookup = _otherMetadataLookupDtoFixture.Create();
        TestBookArtworkProvider provider = new(_artworkDtoFixture.Create());
        IArtworkProvider baseProvider = provider;

        // Act
        ArtworkDto? result = await baseProvider.GetArtworkAsync(otherLookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Null(provider.ReceivedLookup);
    }

    /// <summary>
    /// Test double for the typed artwork provider interface that records the forwarded arguments.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class TestBookArtworkProvider : IArtworkProvider<BookMetadataLookupDto>
    {
        private readonly ArtworkDto _artwork;

        /// <summary>
        /// Gets the lookup that was forwarded to the typed implementation.
        /// </summary>
        public BookMetadataLookupDto? ReceivedLookup { get; private set; }

        /// <summary>
        /// Gets the cancellation token that was forwarded to the typed implementation.
        /// </summary>
        public CancellationToken ReceivedCancellationToken { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestBookArtworkProvider"/> class.
        /// </summary>
        /// <param name="artwork">The artwork returned by the typed implementation.</param>
        public TestBookArtworkProvider(ArtworkDto artwork)
        {
            _artwork = artwork;
        }

        /// <inheritdoc/>
        public string Name => "Test Provider";

        /// <inheritdoc/>
        public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.Book];

        /// <inheritdoc/>
        public bool RequiresWebAccess => false;

        /// <inheritdoc/>
        public Task<ArtworkDto?> GetArtworkAsync(BookMetadataLookupDto lookup, CancellationToken cancellationToken)
        {
            ReceivedLookup = lookup;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult<ArtworkDto?>(_artwork);
        }
    }
}
