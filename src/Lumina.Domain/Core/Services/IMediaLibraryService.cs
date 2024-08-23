#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.VideoLibrary.Services;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.Services;
#endregion

namespace Lumina.Domain.Core.Services;

/// <summary>
/// Interface for the service for managing a media library.
/// </summary>
public interface IMediaLibraryService
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the service for managing a audio library.
    /// </summary>
    //public IAudioLibraryService AudioLibraryService { get; }

    /// <summary>
    /// Gets the service for managing a video library.
    /// </summary>
    public IVideoLibraryService VideoLibraryService { get; }

    /// <summary>
    /// Gets the service for managing a written content library.
    /// </summary>
    public IWrittenContentLibraryService WrittenContentLibraryService { get; }
    #endregion
}