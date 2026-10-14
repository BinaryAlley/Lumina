#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.ID3.Core.Tags;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.ID3.UnitTests.Core.Tags;

/// <summary>
/// Contains unit tests for the <see cref="AudioContainerResolver"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioContainerResolverTests
{
    [Theory]
    [InlineData(null, null)] // an absent mime type leaves the container unrecognized
    [InlineData("", null)] // an empty mime type leaves the container unrecognized
    [InlineData("   ", null)] // a blank mime type leaves the container unrecognized
    [InlineData("taglib/wav", "WAV")] // the taglib prefix is dropped and the container is upper cased
    [InlineData("taglib/png", "PNG")] // a container the resolver has no dedicated parser for still yields its name
    [InlineData("audio/x-wavpack", "X-WAVPACK")] // the last segment of a standard mime type names the container
    public void ResolveFromMimeType_WhenCalled_ShouldReturnTheContainerNamedByTheMimeType(string? mimeType, string? expectedContainer)
    {
        // Act
        string? container = AudioContainerResolver.ResolveFromMimeType(mimeType);

        // Assert
        Assert.Equal(expectedContainer, container);
    }
}
