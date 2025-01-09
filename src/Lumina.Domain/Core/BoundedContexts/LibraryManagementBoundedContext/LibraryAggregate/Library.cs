#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
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
    /// Gets the path of the image file used as the cover for the library.
    /// </summary>
    public string? CoverImage { get; private set; }

    /// <summary>
    /// Gets the path of the image file chosen by the user to be used as the cover for the library.
    /// </summary>
    public string? CoverImageSourcePath { get; private set; }

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
    /// <param name="coverImage">The path of the image file used as the cover for the library.</param>
    private Library(
        LibraryId id,
        UserId userId,
        string title,
        LibraryType libraryType,
        List<FileSystemPathId> contentLocations,
        string? coverImage) : base(id)
    {
        UserId = userId;
        Title = title;
        LibraryType = libraryType;
        _contentLocations = contentLocations;
        CoverImage = coverImage;

        _domainEvents.Add(new LibrarySavedDomainEvent(Guid.NewGuid(), this, DateTime.UtcNow));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Library"/> class.
    /// </summary>
    /// <param name="userId">The unique identifier of the user owning the media library.</param>
    /// <param name="title">The title of the media library.</param>
    /// <param name="libraryType">The type of the media library (e.g., Book, TvShow).</param>
    /// <param name="contentLocations">The list of file system paths that make up the media library.</param>
    /// <param name="coverImageSourcePath">The path of the image file chosen by the user to be used as the cover for the library.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Library"/>, or an error message.
    /// </returns>
    public static ErrorOr<Library> Create(
        Guid userId,
        string title,
        LibraryType libraryType,
        IEnumerable<string> contentLocations,
        string? coverImageSourcePath)
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
            tempContentLocations,
            coverImageSourcePath
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
    /// <param name="coverImageSourcePath">The path of the image file chosen by the user to be used as the cover for the library.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Library"/>, or an error message.
    /// </returns>
    public static ErrorOr<Library> Create(
        LibraryId id,
        Guid userId,
        string title,
        LibraryType libraryType,
        IEnumerable<string> contentLocations,
        string? coverImageSourcePath)
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
            tempContentLocations,
            coverImageSourcePath
        );
    }

    /// <summary>
    /// Sets the file system path where the cover image was copied to from where it was located when the user selected it, to the internal library.
    /// </summary>
    /// <param name="path">The internal libray path location of the cover image file.</param>
    public void SetInternalLibraryCoverImagePath(string path)
    {
        CoverImage = path;
    }
}
