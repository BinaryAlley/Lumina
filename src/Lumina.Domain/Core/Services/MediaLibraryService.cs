#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.VideoLibrary.Services;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.Services;
#endregion

namespace Lumina.Domain.Core.Services;

/// <summary>
/// Service for managing media libraries.
/// </summary>
public class MediaLibraryService : IMediaLibraryService
{
    /// <summary>
    /// Gets the service for managing a video library.
    /// </summary>
    public IVideoLibraryService VideoLibraryService { get; private set; }

    /// <summary>
    /// Gets the service for managing a written content library.
    /// </summary>
    public IWrittenContentLibraryService WrittenContentLibraryService { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryService"/> class.
    /// </summary>
    /// <param name="videoLibraryService">Injected video library service.</param>
    /// <param name="writtenContentLibraryService">Injected written content library service.</param>
    public MediaLibraryService(IVideoLibraryService videoLibraryService, IWrittenContentLibraryService writtenContentLibraryService)
    {
        VideoLibraryService = videoLibraryService;
        WrittenContentLibraryService = writtenContentLibraryService;
    }
}