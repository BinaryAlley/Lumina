#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;

/// <summary>
/// Interface for the service that validates media library path templates and derives metadata from paths using them.
/// </summary>
public interface ILibraryPathTemplateService
{
    /// <summary>
    /// Gets the default path template of the provided <paramref name="libraryType"/>, or an empty template when the type has no path part catalog yet.
    /// </summary>
    /// <param name="libraryType">The media library type whose default path template is retrieved.</param>
    /// <returns>The default path template.</returns>
    LibraryPathTemplate GetDefaultTemplate(LibraryType libraryType);

    /// <summary>
    /// Resolves the effective path template of a media library of the provided <paramref name="libraryType"/>: the template built from the provided
    /// <paramref name="providedParts"/>, or the ideal structure of the library type when no parts are provided.
    /// </summary>
    /// <param name="libraryType">The media library type the template belongs to.</param>
    /// <param name="providedParts">The parts provided by the user. An empty collection means no template was provided, so the library type default is used.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the resolved path template, or an error.
    /// </returns>
    Result<LibraryPathTemplate> ResolveTemplate(LibraryType libraryType, IReadOnlyList<LibraryPathPart> providedParts);

    /// <summary>
    /// Validates the provided <paramref name="template"/> against the catalog of the provided <paramref name="libraryType"/>.
    /// </summary>
    /// <param name="libraryType">The media library type the template belongs to.</param>
    /// <param name="template">The path template to validate.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Result<Success> Validate(LibraryType libraryType, LibraryPathTemplate template);

    /// <summary>
    /// Derives the metadata of a media library item from its relative path, using the provided <paramref name="template"/>.
    /// </summary>
    /// <param name="libraryType">The media library type the template belongs to.</param>
    /// <param name="template">The path template used to parse the path.</param>
    /// <param name="relativePath">The path of the media library item, relative to the root of its content location.</param>
    /// <param name="pathSeparator">The character used to separate path segments on the current platform.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the derived metadata, or an error. A successful result whose value has no value
    /// means the path does not match the template.
    /// </returns>
    Result<Optional<ParsedLibraryPath>> Parse(LibraryType libraryType, LibraryPathTemplate template, string relativePath, char pathSeparator);
}
