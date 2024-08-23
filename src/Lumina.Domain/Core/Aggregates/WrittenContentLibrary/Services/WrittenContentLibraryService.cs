#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Services;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.Services;

/// <summary>
/// Service for managing written content.
/// </summary>
public class WrittenContentLibraryService : IWrittenContentLibraryService
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the service for managing book libraries.
    /// </summary>
    public IBookLibraryService BookLibraryService { get; private set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="WrittenContentLibraryService"/> class.
    /// </summary>
    /// <param name="bookLibraryService">Injected book library service.</param>
    public WrittenContentLibraryService(IBookLibraryService bookLibraryService)
    {
        BookLibraryService = bookLibraryService;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    #endregion
}