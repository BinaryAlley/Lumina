#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.Infrastructure.Models.MediaLibraryScanJobPayloads;
using Lumina.Application.Common.Infrastructure.Security;
using Microsoft.Extensions.ObjectPool;
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
    private readonly ThreadLocal<byte[]> _threadLocalBuffer = new(() => new byte[DEFAULT_BUFFER_SIZE]); // thread-local buffer to minimize memory allocations during parallel processing
    private static readonly ObjectPool<XxHash64> s_hasherPool = new DefaultObjectPool<XxHash64>(new DefaultPooledObjectPolicy<XxHash64>()); // object pool for reusing hashers, to reduce GC pressure

    /// <summary>
    /// Hashes <paramref name="files"/> by sampling chunks from them.
    /// </summary>
    /// <param name="files">The collection of files to hash.</param>
    /// <param name="previousScanResults">Lookup dictionary of a previous media library scan, used to determine if a file changed or not.</param>
    /// <param name="callback">Callback to invoke during processing of elements.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A collection of files that changed since last library scan, along with their hashes.</returns>
    public async Task<List<ChangedFileSystemFile>> HashFilesAsync(
        List<FileInfo> files, 
        Dictionary<string, LibraryScanResultEntity> previousScanResults,
        Func<Task> callback, 
        CancellationToken cancellationToken)
    {
        List<ChangedFileSystemFile> changedFiles = [];
        object listLock = new();
        int totalFiles = files.Count;

        // make use of all available processors
        ParallelOptions parallelOptions = new()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
        await Parallel.ForEachAsync(files, parallelOptions, async (file, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                // invoke the method that triggers progress reporting
                await callback().ConfigureAwait(false);
                // if file has no contents, it needs no hashing
                if (file.Length == 0)
                    return;
                // buffer management with thread-local reuse
                byte[] buffer = _threadLocalBuffer.Value ?? new byte[DEFAULT_BUFFER_SIZE];
                ushort bufferSize = (ushort)Math.Min(DEFAULT_BUFFER_SIZE, file.Length);

                // get the file hash and check if it differs from the one stored on previous scan
                ulong currentHash = ComputeFileHash(file.FullName, file.Length, bufferSize, buffer);
                bool isChanged = !previousScanResults.TryGetValue(file.FullName, out LibraryScanResultEntity? previous) || previous.ContentHash != currentHash;

                if (isChanged) // hash changed, file needs to be re-scanned
                    lock (listLock)
                        changedFiles.Add(new ChangedFileSystemFile(file, currentHash));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing {file.FullName}: {ex.Message}");
            }
        });

        return changedFiles;
    }

    /// <summary>
    /// Computes content hash using memory-mapped sampling.
    /// </summary>
    /// <param name="bufferSize">Sample window size (automatically clamped to file size).</param>
    /// <param name="buffer">Reusable read buffer (thread-local allocation optimized).</param>
    private unsafe ulong ComputeFileHash(string filePath, long fileSize, ushort bufferSize, byte[] buffer)
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
            // get a hasher from the pool to avoid allocations
            XxHash64 hasher = s_hasherPool.Get();

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
        yield return fileSize / 2; // file of the file
        yield return 3 * fileSize / 4; // three quarters of the file
        yield return Math.Max(0, fileSize - bufferSize); // end of the file (ensure we don't go past the end)

        // additional evenly spaced samples, if needed
        long interval = fileSize / (SAMPLE_COUNT + 1);
        for (int i = 1; i <= SAMPLE_COUNT - 4; i++)
            yield return interval * i;
    }
}
