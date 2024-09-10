#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;

/// <summary>
/// Value Object for the information about a media stream.
/// </summary>
[DebuggerDisplay("{StreamId}")]
public class StreamInfo : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the id of the stream.
    /// </summary>
    public string StreamId { get; }
    
    /// <summary>
    /// Gets the MimeType of the stream.
    /// </summary>
    public string MimeType { get; }
    
    /// <summary>
    /// Gets the bitrate of the stream.
    /// </summary>
    public int Bitrate { get; }
    
    /// <summary>
    /// Gets the codec of the stream.
    /// </summary>
    public string Codec { get; }
    
    /// <summary>
    /// Gets the optional resolution of the stream.
    /// </summary>
    public Optional<string> Resolution { get; }
    
    /// <summary>
    /// Gets the optional frame rate of the stream.
    /// </summary>
    public Optional<float> FrameRate { get; }
    
    /// <summary>
    /// Gets the optional sample rate of the stream.
    /// </summary>
    public Optional<int> SampleRate { get; }
    
    /// <summary>
    /// Gets the optional channels of the stream.
    /// </summary>
    public Optional<int> Channels { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamInfo"/> class.
    /// </summary>
    /// <param name="streamId">The unique identifier for the stream.</param>
    /// <param name="mimeType">The MIME type of the stream content.</param>
    /// <param name="bitrate">The bitrate of the stream in bits per second.</param>
    /// <param name="codec">The codec used for the stream.</param>
    /// <param name="resolution">The resolution of the stream (for video).</param>
    /// <param name="frameRate">The frame rate of the stream (for video).</param>
    /// <param name="sampleRate">The sample rate of the stream (for audio).</param>
    /// <param name="channels">The number of channels (for audio).</param>
    private StreamInfo(
        string streamId, 
        string mimeType, 
        int bitrate, 
        string codec,
        Optional<string> resolution, 
        Optional<float> frameRate,
        Optional<int> sampleRate, 
        Optional<int> channels)
    {
        StreamId = streamId;
        MimeType = mimeType;
        Bitrate = bitrate;
        Codec = codec;
        Resolution = resolution;
        FrameRate = frameRate;
        SampleRate = sampleRate;
        Channels = channels;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="StreamInfo"/> class.
    /// </summary>
    /// <param name="streamId">The unique identifier for the stream.</param>
    /// <param name="mimeType">The MIME type of the stream content.</param>
    /// <param name="bitrate">The bitrate of the stream in bits per second.</param>
    /// <param name="codec">The codec used for the stream.</param>
    /// <param name="resolution">The resolution of the stream (for video).</param>
    /// <param name="frameRate">The frame rate of the stream (for video).</param>
    /// <param name="sampleRate">The sample rate of the stream (for audio).</param>
    /// <param name="channels">The number of channels (for audio).</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="StreamInfo"/>, or an error message.
    /// </returns>
    public static ErrorOr<StreamInfo> Create(
        string streamId, 
        string mimeType, 
        int bitrate, 
        string codec,
        Optional<string> resolution, 
        Optional<float> frameRate, 
        Optional<int> sampleRate, 
        Optional<int> channels)
    {
        if (streamId is null)
            return Errors.FileManagement.StreamIdCannotBeEmpty;
        if (codec is null)
            return Errors.FileManagement.CodecCannotBeEmpty;
        if (bitrate <= 0)
            return Errors.FileManagement.BitrateMustBeAPositiveNumber;
        return new StreamInfo(streamId, mimeType, bitrate, codec, resolution, frameRate, sampleRate, channels);
    }
    
    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return StreamId;
        yield return MimeType;
        yield return Bitrate;
        yield return Codec;
        yield return Resolution;
        yield return FrameRate;
        yield return SampleRate;
        yield return Channels;
    }
    #endregion
}