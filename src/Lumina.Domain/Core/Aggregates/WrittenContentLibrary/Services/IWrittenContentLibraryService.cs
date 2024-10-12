#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Services;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.Services;

/// <summary>
/// Interface for the service for managing written content.
/// </summary>
public interface IWrittenContentLibraryService
{
    /// <summary>
    /// Gets the service for managing book libraries.
    /// </summary>
    public IBookLibraryService BookLibraryService { get; }
}