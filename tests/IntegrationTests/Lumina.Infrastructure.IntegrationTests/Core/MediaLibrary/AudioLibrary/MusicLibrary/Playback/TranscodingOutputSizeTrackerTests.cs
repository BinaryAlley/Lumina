#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains integration tests for the <see cref="TranscodingOutputSizeTracker"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingOutputSizeTrackerTests : IDisposable
{
    private const string SEGMENT_FILE_PREFIX = "segment";
    private const string SEGMENT_FILE_EXTENSION = ".m4s";
    private const string SEGMENT_LIST_FILE_NAME = "index.m3u8";

    private readonly string _temporaryDirectory;
    private readonly TranscodingOutputSizeTracker _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingOutputSizeTrackerTests"/> class.
    /// </summary>
    public TranscodingOutputSizeTrackerTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-transcoding-output-size-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
        _sut = new TranscodingOutputSizeTracker(_temporaryDirectory, SEGMENT_FILE_PREFIX, SEGMENT_FILE_EXTENSION, SEGMENT_LIST_FILE_NAME);
    }

    [Fact]
    public void GetCurrentSize_WhenCalledForTheFirstTime_ShouldCountTheSegmentListAndTheExistingSegments()
    {
        // Arrange
        CreateSegmentList(10);
        CreateSegment(0, 100);
        CreateSegment(1, 50);

        // Act
        long result = _sut.GetCurrentSize();

        // Assert
        Assert.Equal(160L, result);
    }

    [Fact]
    public void GetCurrentSize_WhenTheCurrentSegmentGrows_ShouldCountOnlyTheGrowth()
    {
        // Arrange
        CreateSegmentList(10);
        CreateSegment(0, 100);
        Assert.Equal(110L, _sut.GetCurrentSize());

        // Act
        AppendToSegment(0, 40);
        long result = _sut.GetCurrentSize();

        // Assert
        Assert.Equal(150L, result);
    }

    [Fact]
    public void GetCurrentSize_WhenTheSegmentListGrows_ShouldCountOnlyTheGrowth()
    {
        // Arrange
        CreateSegmentList(10);
        CreateSegment(0, 10);
        Assert.Equal(20L, _sut.GetCurrentSize());

        // Act
        AppendToFile(Path.Combine(_temporaryDirectory, SEGMENT_LIST_FILE_NAME), 5);
        long result = _sut.GetCurrentSize();

        // Assert
        Assert.Equal(25L, result);
    }

    [Fact]
    public void GetCurrentSize_WhenASegmentIsFinishedAndANewOneStarts_ShouldDiscoverEachSegmentOnce()
    {
        // Arrange
        CreateSegmentList(10);
        CreateSegment(0, 100);
        Assert.Equal(110L, _sut.GetCurrentSize());
        AppendToSegment(0, 50);
        CreateSegment(1, 30);

        // Act
        long resultAfterTheNewSegment = _sut.GetCurrentSize();
        AppendToSegment(1, 20);
        long resultAfterTheNewSegmentGrows = _sut.GetCurrentSize();

        // Assert
        Assert.Equal(190L, resultAfterTheNewSegment);
        Assert.Equal(210L, resultAfterTheNewSegmentGrows);
    }

    [Fact]
    public void GetCurrentSize_WhenFilesDoNotFollowTheSegmentNamePattern_ShouldIgnoreThem()
    {
        // Arrange
        CreateSegmentList(5);
        CreateSegment(0, 10);
        File.WriteAllBytes(Path.Combine(_temporaryDirectory, "segment000001.bak"), new byte[1000]);
        File.WriteAllBytes(Path.Combine(_temporaryDirectory, "other000000.m4s"), new byte[1000]);
        File.WriteAllBytes(Path.Combine(_temporaryDirectory, "segment-1.m4s"), new byte[1000]);
        File.WriteAllBytes(Path.Combine(_temporaryDirectory, "segment000000.txt"), new byte[1000]);

        // Act
        long result = _sut.GetCurrentSize();

        // Assert
        Assert.Equal(15L, result);
    }

    [Fact]
    public void GetCurrentSize_WhenTheLastSegmentDisappears_ShouldKeepItsLastMeasuredLength()
    {
        // Arrange
        CreateSegmentList(10);
        CreateSegment(0, 100);
        Assert.Equal(110L, _sut.GetCurrentSize());

        // Act
        File.Delete(GetSegmentPath(0));
        long result = _sut.GetCurrentSize();

        // Assert
        Assert.Equal(110L, result);
    }

    [Fact]
    public void GetCurrentSize_WhenTheSegmentListDisappears_ShouldKeepItsLastMeasuredLength()
    {
        // Arrange
        CreateSegmentList(10);
        CreateSegment(0, 100);
        Assert.Equal(110L, _sut.GetCurrentSize());

        // Act
        File.Delete(Path.Combine(_temporaryDirectory, SEGMENT_LIST_FILE_NAME));
        long result = _sut.GetCurrentSize();

        // Assert
        Assert.Equal(110L, result);
    }

    /// <summary>
    /// Creates the segment list file with the requested size.
    /// </summary>
    /// <param name="size">The size of the segment list, in bytes.</param>
    private void CreateSegmentList(int size)
    {
        File.WriteAllBytes(Path.Combine(_temporaryDirectory, SEGMENT_LIST_FILE_NAME), new byte[size]);
    }

    /// <summary>
    /// Creates a segment file with the requested zero based index and size.
    /// </summary>
    /// <param name="segmentIndex">The zero based index of the segment.</param>
    /// <param name="size">The size of the segment, in bytes.</param>
    private void CreateSegment(int segmentIndex, int size)
    {
        File.WriteAllBytes(GetSegmentPath(segmentIndex), new byte[size]);
    }

    /// <summary>
    /// Appends the requested number of bytes to a segment file.
    /// </summary>
    /// <param name="segmentIndex">The zero based index of the segment.</param>
    /// <param name="size">The number of bytes to append.</param>
    private void AppendToSegment(int segmentIndex, int size)
    {
        AppendToFile(GetSegmentPath(segmentIndex), size);
    }

    /// <summary>
    /// Appends the requested number of bytes to a file.
    /// </summary>
    /// <param name="filePath">The file to append to.</param>
    /// <param name="size">The number of bytes to append.</param>
    private static void AppendToFile(string filePath, int size)
    {
        using (FileStream stream = new(filePath, FileMode.Append, FileAccess.Write))
            stream.Write(new byte[size], 0, size);
    }

    /// <summary>
    /// Gets the path of the segment file of the requested zero based index.
    /// </summary>
    /// <param name="segmentIndex">The zero based index of the segment.</param>
    /// <returns>The path of the segment file.</returns>
    private string GetSegmentPath(int segmentIndex)
    {
        return Path.Combine(_temporaryDirectory, $"{SEGMENT_FILE_PREFIX}{segmentIndex:D6}{SEGMENT_FILE_EXTENSION}");
    }

    /// <summary>
    /// Removes the temporary directory created for the test.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_temporaryDirectory))
                Directory.Delete(_temporaryDirectory, recursive: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // The directory is under the process temporary directory.
        }
        GC.SuppressFinalize(this);
    }
}
