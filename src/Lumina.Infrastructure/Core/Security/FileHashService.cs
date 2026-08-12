#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
using Lumina.Application.Common.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Hashing;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Security;

/// <summary>
/// Service for hashing files by sampling chunks from them.
/// </summary>
internal class FileHashService : IFileHashService
{
    private const int SAMPLE_COUNT = 6; // number of sample points for hash computation
    private const ushort DEFAULT_BUFFER_SIZE = 65535; // buffer size (64KB - 1) for reading file chunks
    private static readonly ThreadLocal<XxHash64> s_threadLocalHasher = new(() => new XxHash64()); // thread-local hasher to avoid contention and pool management overhead during parallel processing
    private static readonly ulong s_emptyFileHash = ComputeEmptyHash();

    /// <summary>
    /// Hashes <paramref name="inputFiles"/> by sampling chunks from them.
    /// </summary>
    /// <param name="inputFiles">The collection of files to hash.</param>
    /// <param name="callback">Callback to invoke during processing of elements.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A collection of the hashed files, along with their hashes.</returns>
    public async Task<List<HashedFileSystemFileDto>> HashFilesAsync(IReadOnlyCollection<HashedFileSystemFileDto> inputFiles, Func<Task> callback, CancellationToken cancellationToken)
    {
        List<HashedFileSystemFileDto> outputFiles = [];
        object listLock = new();

        // make use of all available processors
        ParallelOptions parallelOptions = new()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount // TODO: make this configurable, such that parallelism can be turned off for mechanical hard drives
        };
        await Parallel.ForEachAsync(inputFiles, parallelOptions, async (file, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                // invoke the method that triggers progress reporting
                await callback().ConfigureAwait(false);

                ushort bufferSize = (ushort)Math.Min(DEFAULT_BUFFER_SIZE, file.Size);

                // get the file hash and check if it differs from the one stored on previous scan
                ulong currentHash = file.Size == 0
                    ? s_emptyFileHash // precomputed constant: XxHash64 hash of zero-length input
                    : ComputeFileHash(file.Path, file.Size, bufferSize);

                // the decision of whether a file changed is already made before hashing, so all files that reach this point get their hash stored
                lock (listLock)
                    outputFiles.Add(file with { CurrentHash = currentHash });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing {file.Path}: {ex.Message}");
            }
        });

        return outputFiles;
    }

    /// <summary>
    /// Computes content hash using memory-mapped sampling.
    /// </summary>
    /// <param name="filePath">The path of the file to hash.</param>
    /// <param name="fileSize">The size of the file, in bytes.</param>
    /// <param name="bufferSize">Sample window size (automatically clamped to file size).</param>
    private static unsafe ulong ComputeFileHash(string filePath, long fileSize, ushort bufferSize)
    {
        // create a memory-mapped file for efficient access to large files
        using MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        // create a view accessor to read from the memory-mapped file
        using MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(0, fileSize, MemoryMappedFileAccess.Read);

        byte* memoryPointer = null;

        try
        {
            // get a direct pointer to the memory-mapped file, for faster access
            memoryMappedViewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref memoryPointer);

            // get the thread-local hasher and reset it before use; resetting at entry (rather than at exit) ensures
            // the hasher is always in a clean state even if a previous call on this thread threw mid-computation
            XxHash64 hasher = s_threadLocalHasher.Value!;
            hasher.Reset();

            // allocate hash result buffer on the stack, to avoid heap allocation
            Span<byte> hashSpan = stackalloc byte[sizeof(ulong)];

            // process only predefined sampled segments of the file, instead of the entire file
            foreach (long offset in GetByteBufferSampleOffsets(fileSize, bufferSize))
            {
                // calculate how many bytes to read (handle edge case at end of file, or when the file size is smaller than the buffer)
                ushort readSize = (ushort)Math.Min(bufferSize, fileSize - offset);
                // create a span directly over the memory-mapped region
                ReadOnlySpan<byte> dataSpan = new(memoryPointer + offset, readSize);
                // update the hash with this chunk of data
                hasher.Append(dataSpan);
            }

            // get the final hash value
            if (!hasher.TryGetCurrentHash(hashSpan, out int bytesWritten) || bytesWritten != sizeof(ulong))
                throw new InvalidOperationException("Hash computation failed");
            // convert the hash byte span to a long, and return it
            return BitConverter.ToUInt64(hashSpan);
        }
        finally
        {
            if (memoryPointer is not null) // always release the pointer, to prevent memory leaks
                memoryMappedViewAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
        }
    }

    /// <summary>
    /// Generates sample offsets for reading portions of a file.
    /// </summary>
    /// <param name="fileSize">The size of the file, in bytes.</param>
    /// <param name="bufferSize">The buffer size used for reading.</param>
    /// <returns>An enumerable of byte offsets to sample from the file.</returns>
    private static IEnumerable<long> GetByteBufferSampleOffsets(long fileSize, ushort bufferSize)
    {
        // for small files, just read from the beginning
        if (fileSize <= bufferSize * SAMPLE_COUNT)
        {
            yield return 0;
            yield break;
        }
        // for larger files, sample at strategic points:
        yield return 0; // beginning of the file
        yield return fileSize / 4; // quarter of the file
        yield return fileSize / 2; // half of the file
        yield return 3 * fileSize / 4; // three quarters of the file
        yield return Math.Max(0, fileSize - bufferSize); // end of the file (ensure we don't go past the end)

        // additional evenly spaced samples, if needed
        long interval = fileSize / (SAMPLE_COUNT + 1);
        for (int i = 1; i <= SAMPLE_COUNT - 4; i++)
            yield return interval * i;
    }

    /// <summary>
    /// Computes the hash for an empty file, which is a special case, since it has no content to sample.
    /// </summary>
    /// <returns>The hash value for an empty file.</returns>
    private static ulong ComputeEmptyHash()
    {
        XxHash64 hasher = new();
        Span<byte> hashSpan = stackalloc byte[sizeof(ulong)];
        hasher.TryGetCurrentHash(hashSpan, out _);
        return BitConverter.ToUInt64(hashSpan);
    }
}
