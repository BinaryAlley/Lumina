#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains unit tests for the <see cref="TranscodingToolchainStatus"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingToolchainStatusTests
{
    private readonly TranscodingToolchainStatus _sut;
    private readonly IReadOnlyList<string> _catalogEncoderNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingToolchainStatusTests"/> class.
    /// </summary>
    public TranscodingToolchainStatusTests()
    {
        _sut = new TranscodingToolchainStatus();
        _catalogEncoderNames = [.. PlaybackFormatCatalog.Formats.Select(format => format.EncoderName).Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    [Fact]
    public void IsTranscoderAvailable_WhenTheStatusIsFresh_ShouldReturnTrue()
    {
        // Act
        bool result = _sut.IsTranscoderAvailable;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEncoderAvailable_WhenTheStatusIsFresh_ShouldReturnTrueForEveryCatalogEncoder()
    {
        // Act
        IReadOnlyList<bool> results = [.. _catalogEncoderNames.Select(_sut.IsEncoderAvailable)];

        // Assert
        Assert.All(results, result => Assert.True(result));
    }

    [Fact]
    public void RecordUnavailable_WhenCalled_ShouldReportTheTranscoderAndEveryCatalogEncoderUnavailable()
    {
        // Act
        _sut.RecordUnavailable();

        // Assert
        Assert.False(_sut.IsTranscoderAvailable);
        Assert.All(_catalogEncoderNames, encoderName => Assert.False(_sut.IsEncoderAvailable(encoderName)));
    }

    [Fact]
    public void RecordMissingEncoders_WhenCalled_ShouldKeepTheTranscoderAvailableAndReportOnlyTheListedEncodersUnavailable()
    {
        // Arrange
        string missingEncoderName = _catalogEncoderNames[0];
        string availableEncoderName = _catalogEncoderNames[1];

        // Act
        _sut.RecordMissingEncoders([missingEncoderName]);

        // Assert
        Assert.True(_sut.IsTranscoderAvailable);
        Assert.False(_sut.IsEncoderAvailable(missingEncoderName));
        Assert.True(_sut.IsEncoderAvailable(availableEncoderName));
    }

    [Theory]
    [InlineData("AAC")] // an encoder name in upper case
    [InlineData("Aac")] // an encoder name in mixed case
    public void RecordMissingEncoders_WhenTheNameDiffersInCase_ShouldReportTheEncoderUnavailable(string missingEncoderName)
    {
        // Act
        _sut.RecordMissingEncoders([missingEncoderName]);

        // Assert
        Assert.True(_sut.IsTranscoderAvailable);
        Assert.False(_sut.IsEncoderAvailable("aac"));
    }

    [Fact]
    public void RecordMissingEncoders_WhenCalledTwice_ShouldReplaceThePreviousMissingSetInsteadOfAccumulatingIt()
    {
        // Act
        _sut.RecordMissingEncoders(["aac"]);
        _sut.RecordMissingEncoders(["libopus"]);

        // Assert
        Assert.True(_sut.IsTranscoderAvailable);
        Assert.True(_sut.IsEncoderAvailable("aac"));
        Assert.False(_sut.IsEncoderAvailable("libopus"));
    }

    [Fact]
    public void RecordMissingEncoders_WhenTheListIsEmpty_ShouldKeepEveryCatalogEncoderAvailable()
    {
        // Act
        _sut.RecordMissingEncoders([]);

        // Assert
        Assert.True(_sut.IsTranscoderAvailable);
        Assert.All(_catalogEncoderNames, encoderName => Assert.True(_sut.IsEncoderAvailable(encoderName)));
    }

    [Fact]
    public void RecordAvailable_WhenTheTranscoderWasUnavailable_ShouldRestoreTheTranscoderAndEveryCatalogEncoder()
    {
        // Arrange
        _sut.RecordUnavailable();

        // Act
        _sut.RecordAvailable();

        // Assert
        Assert.True(_sut.IsTranscoderAvailable);
        Assert.All(_catalogEncoderNames, encoderName => Assert.True(_sut.IsEncoderAvailable(encoderName)));
    }

    [Fact]
    public void RecordAvailable_WhenEncodersWereMissing_ShouldClearTheMissingSet()
    {
        // Arrange
        _sut.RecordMissingEncoders(["aac", "libopus"]);

        // Act
        _sut.RecordAvailable();

        // Assert
        Assert.True(_sut.IsTranscoderAvailable);
        Assert.True(_sut.IsEncoderAvailable("aac"));
        Assert.True(_sut.IsEncoderAvailable("libopus"));
    }
}
