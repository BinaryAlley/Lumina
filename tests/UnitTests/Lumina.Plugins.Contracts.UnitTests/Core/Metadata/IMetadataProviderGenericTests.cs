#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.UnitTests.Core.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="IMetadataProvider{TLookup, TMetadata}"/> interface.
/// </summary>
[ExcludeFromCodeCoverage]
public class IMetadataProviderGenericTests
{
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();
    private readonly BookMetadataDtoFixture _bookMetadataDtoFixture = new();
    private readonly OtherMetadataLookupDtoFixture _otherMetadataLookupDtoFixture = new();

    [Fact]
    public async Task GetSearchResultsAsync_WhenCalledThroughBaseInterface_ShouldForwardToTypedImplementation()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookMetadataDto expectedMetadata = _bookMetadataDtoFixture.Create(title: "Search Result", includeOptionalProperties: false);
        TestBookMetadataProvider provider = new(expectedMetadata);
        IMetadataProvider baseProvider = provider;

        // Act
        IReadOnlyList<MetadataDto> result = await baseProvider.GetSearchResultsAsync(lookup, cancellationToken);

        // Assert
        BookMetadataDto metadata = Assert.IsType<BookMetadataDto>(Assert.Single(result));
        Assert.Equal(expectedMetadata, metadata);
        Assert.Same(lookup, provider.ReceivedLookup);
        Assert.Equal(cancellationToken, provider.ReceivedCancellationToken);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenCalledThroughBaseInterface_ShouldForwardToTypedImplementation()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookMetadataDto expectedMetadata = _bookMetadataDtoFixture.Create(title: "Exact Result", includeOptionalProperties: false);
        TestBookMetadataProvider provider = new(expectedMetadata);
        IMetadataProvider baseProvider = provider;

        // Act
        MetadataDto? result = await baseProvider.GetMetadataAsync(lookup, cancellationToken);

        // Assert
        Assert.Equal(expectedMetadata, Assert.IsType<BookMetadataDto>(result));
        Assert.Same(lookup, provider.ReceivedLookup);
        Assert.Equal(cancellationToken, provider.ReceivedCancellationToken);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupIsOfAnotherRuntimeType_ShouldThrowInvalidCastException()
    {
        // Arrange
        MetadataLookupDto otherLookup = _otherMetadataLookupDtoFixture.Create();
        TestBookMetadataProvider provider = new(_bookMetadataDtoFixture.Create(title: "Search Result", includeOptionalProperties: false));
        IMetadataProvider baseProvider = provider;

        // Act
        async Task Act()
        {
            await baseProvider.GetSearchResultsAsync(otherLookup, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidCastException>(Act);
        Assert.Null(provider.ReceivedLookup);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenLookupIsOfAnotherRuntimeType_ShouldThrowInvalidCastException()
    {
        // Arrange
        MetadataLookupDto otherLookup = _otherMetadataLookupDtoFixture.Create();
        TestBookMetadataProvider provider = new(_bookMetadataDtoFixture.Create(title: "Exact Result", includeOptionalProperties: false));
        IMetadataProvider baseProvider = provider;

        // Act
        async Task Act()
        {
            await baseProvider.GetMetadataAsync(otherLookup, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidCastException>(Act);
        Assert.Null(provider.ReceivedLookup);
    }

    /// <summary>
    /// Test double for the typed metadata provider interface that records the forwarded arguments.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class TestBookMetadataProvider : IMetadataProvider<BookMetadataLookupDto, BookMetadataDto>
    {
        private readonly BookMetadataDto _metadata;

        /// <summary>
        /// Gets the lookup that was forwarded to the typed implementation.
        /// </summary>
        public BookMetadataLookupDto? ReceivedLookup { get; private set; }

        /// <summary>
        /// Gets the cancellation token that was forwarded to the typed implementation.
        /// </summary>
        public CancellationToken ReceivedCancellationToken { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestBookMetadataProvider"/> class.
        /// </summary>
        /// <param name="metadata">The metadata returned by the typed implementation.</param>
        public TestBookMetadataProvider(BookMetadataDto metadata)
        {
            _metadata = metadata;
        }

        /// <inheritdoc/>
        public string Name => "Test Provider";

        /// <inheritdoc/>
        public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.Book];

        /// <inheritdoc/>
        public bool RequiresWebAccess => false;

        /// <inheritdoc/>
        public Task<IReadOnlyList<BookMetadataDto>> GetSearchResultsAsync(BookMetadataLookupDto lookup, CancellationToken cancellationToken)
        {
            ReceivedLookup = lookup;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult<IReadOnlyList<BookMetadataDto>>([_metadata]);
        }

        /// <inheritdoc/>
        public Task<BookMetadataDto?> GetMetadataAsync(BookMetadataLookupDto lookup, CancellationToken cancellationToken)
        {
            ReceivedLookup = lookup;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult<BookMetadataDto?>(_metadata);
        }
    }
}
