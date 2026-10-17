#region ========================================================================= USING =====================================================================================
using System;
using System.IO;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Tracks the size of the temporary output of a running normalization job without walking the growing directory on every monitor tick.
/// </summary>
/// <remarks>
/// Segment files are written in sequence and only the last one grows. The tracker discovers each new segment once, keeps the size of the completed
/// segments, and only re-stats the current segment and the segment list on every tick, so the monitor stays O(1) per tick instead of O(n).
/// </remarks>
internal sealed class TranscodingOutputSizeTracker
{
    private readonly string _directoryPath;
    private readonly string _segmentFilePrefix;
    private readonly string _segmentFileExtension;
    private readonly string _segmentListPath;
    private long _totalBytes;
    private long _segmentListLength;
    private long _lastSegmentLength;
    private int _nextSegmentIndex;
    private string? _lastSegmentPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingOutputSizeTracker"/> class.
    /// </summary>
    /// <param name="directoryPath">The temporary directory the job writes into.</param>
    /// <param name="segmentFilePrefix">The prefix of the generated segment file names.</param>
    /// <param name="segmentFileExtension">The file extension of a segment, including the leading dot.</param>
    /// <param name="segmentListFileName">The file name of the segment list written by ffmpeg.</param>
    internal TranscodingOutputSizeTracker(string directoryPath, string segmentFilePrefix, string segmentFileExtension, string segmentListFileName)
    {
        _directoryPath = directoryPath;
        _segmentFilePrefix = segmentFilePrefix;
        _segmentFileExtension = segmentFileExtension;
        _segmentListPath = Path.Combine(directoryPath, segmentListFileName);
    }

    /// <summary>
    /// Gets the current total size of the produced output, in bytes.
    /// </summary>
    /// <returns>The current total size of the produced output, in bytes.</returns>
    public long GetCurrentSize()
    {
        // The previous tick's last segment may have grown before a new segment was started, so it is measured before new files are discovered.
        RecalculateLastSegment();
        RecalculateSegmentList();
        DiscoverNewSegments();
        RecalculateLastSegment();
        return _totalBytes;
    }

    /// <summary>
    /// Updates the total with the growth of the segment that is currently being written, when there is one.
    /// </summary>
    private void RecalculateLastSegment()
    {
        if (_lastSegmentPath is null)
            return;
        long currentLength = GetFileLength(_lastSegmentPath, _lastSegmentLength);
        _totalBytes += currentLength - _lastSegmentLength;
        _lastSegmentLength = currentLength;
    }

    /// <summary>
    /// Updates the total with the growth of the segment list.
    /// </summary>
    private void RecalculateSegmentList()
    {
        long currentLength = GetFileLength(_segmentListPath, _segmentListLength);
        _totalBytes += currentLength - _segmentListLength;
        _segmentListLength = currentLength;
    }

    /// <summary>
    /// Adds every segment that has appeared since the previous tick, in sequence, and leaves the last one as the growing segment.
    /// </summary>
    private void DiscoverNewSegments()
    {
        while (true)
        {
            string segmentPath = Path.Combine(_directoryPath, $"{_segmentFilePrefix}{_nextSegmentIndex:D6}{_segmentFileExtension}");
            FileInfo segmentFile = new(segmentPath);
            if (!segmentFile.Exists)
                return;

            long segmentLength;
            try
            {
                segmentLength = segmentFile.Length;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {               
                return; // A file that cannot be measured yet is retried on the next tick.
            }

            _totalBytes += segmentLength;
            _lastSegmentPath = segmentPath;
            _lastSegmentLength = segmentLength;
            _nextSegmentIndex++;
        }
    }

    /// <summary>
    /// Gets the length of a file, falling back to its last known length when it cannot currently be measured.
    /// </summary>
    /// <param name="filePath">The file to measure.</param>
    /// <param name="fallbackLength">The length to use when the file cannot be measured.</param>
    /// <returns>The current or fallback length of the file, in bytes.</returns>
    private static long GetFileLength(string filePath, long fallbackLength)
    {
        try
        {
            FileInfo file = new(filePath);
            return file.Exists ? file.Length : fallbackLength;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return fallbackLength;
        }
    }
}
