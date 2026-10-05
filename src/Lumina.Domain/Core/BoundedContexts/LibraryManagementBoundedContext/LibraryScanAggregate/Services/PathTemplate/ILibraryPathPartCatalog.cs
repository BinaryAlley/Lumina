#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;

/// <summary>
/// Interface for the catalog of the path parts available for a given media library type.
/// </summary>
public interface ILibraryPathPartCatalog
{
    /// <summary>
    /// Gets the media library type that this catalog supports.
    /// </summary>
    LibraryType SupportedLibraryType { get; }

    /// <summary>
    /// Gets the parts that can be used in the path template of the supported library type.
    /// </summary>
    /// <returns>The available path parts.</returns>
    IReadOnlyList<LibraryPathPartDefinition> GetPartDefinitions();

    /// <summary>
    /// Gets the default path template of the supported library type, describing the ideal structure the application expects.
    /// </summary>
    /// <returns>The default path template.</returns>
    LibraryPathTemplate GetDefaultTemplate();
}
