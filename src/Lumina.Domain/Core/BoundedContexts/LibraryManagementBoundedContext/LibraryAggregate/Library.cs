#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.MediaLibrary;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;

/// <summary>
/// Aggregate root for a media library.
/// </summary>
[DebuggerDisplay("Id: {Id} Title: {Title}")]
public class Library : AggregateRoot<LibraryId>
{
    private readonly List<FileSystemPathId> _contentLocations;

    /// <summary>
    /// Gets the id of the user owning this media library.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the type of the media library (e.g., Book, TvShow).
    /// </summary>
    public LibraryType LibraryType { get; private set; }

    /// <summary>
    /// Gets the title of the media library.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the list of file system paths that make up the media library.
    /// </summary>
    public IReadOnlyCollection<FileSystemPathId> ContentLocations => _contentLocations.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Library"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the media library.</param>
    /// <param name="userId">The unique identifier of the user owning the media library.</param>
    /// <param name="title">The title of the media library.</param>
    /// <param name="libraryType">The type of the media library (e.g., Book, TvShow).</param>
    /// <param name="contentLocations">The list of file system paths that make up the media library.</param>
    private Library(
        LibraryId id,
        UserId userId,
        string title,
        LibraryType libraryType,
        List<FileSystemPathId> contentLocations) : base(id)
    {
        UserId = userId;
        Title = title;
        LibraryType = libraryType;
        _contentLocations = contentLocations;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Library"/> class.
    /// </summary>
    /// <param name="userId">The unique identifier of the user owning the media library.</param>
    /// <param name="title">The title of the media library.</param>
    /// <param name="libraryType">The type of the media library (e.g., Book, TvShow).</param>
    /// <param name="contentLocations">The list of file system paths that make up the media library.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Library"/>, or an error message.
    /// </returns>
    public static ErrorOr<Library> Create(
        Guid userId,
        string title,
        LibraryType libraryType,
        IEnumerable<string> contentLocations)
    {
        List<FileSystemPathId> tempContentLocations = [];
        // go through all the file system paths that make up the media library and create domain objects from them
        foreach (string contentLocation in contentLocations)
        {
            ErrorOr<FileSystemPathId> contentLocationResult = FileSystemPathId.Create(contentLocation);
            if (contentLocationResult.IsError)
                return contentLocationResult.Errors;
            tempContentLocations.Add(contentLocationResult.Value);
        }
        return new Library(
            LibraryId.CreateUnique(),
            UserId.Create(userId),
            title,
            libraryType,
            tempContentLocations
        );
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Library"/>, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the id of the media library.</param>
    /// <param name="userId">The object representing the id of the user owning the media library.</param>
    /// <param name="title">The title of the media library.</param>
    /// <param name="libraryType">The type of the media library (e.g., Book, TvShow).</param>
    /// <param name="contentLocations">The list of file system paths that make up the media library.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Library"/>, or an error message.
    /// </returns>
    public static ErrorOr<Library> Create(
        LibraryId id,
        Guid userId,
        string title,
        LibraryType libraryType,
        IEnumerable<string> contentLocations)
    {
        List<FileSystemPathId> tempContentLocations = [];
        // go through all the file system paths that make up the media library and create domain objects from them
        foreach (string contentLocation in contentLocations)
        {
            ErrorOr<FileSystemPathId> contentLocationResult = FileSystemPathId.Create(contentLocation);
            if (contentLocationResult.IsError)
                return contentLocationResult.Errors;
            tempContentLocations.Add(contentLocationResult.Value);
        }
        return new Library(
            id,
            UserId.Create(userId),
            title,
            libraryType,
            tempContentLocations
        );
    }
}
