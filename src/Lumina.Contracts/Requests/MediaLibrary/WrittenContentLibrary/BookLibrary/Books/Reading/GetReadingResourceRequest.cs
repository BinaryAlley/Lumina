#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents a request to get a resource of a book, for reading.
/// </summary>
/// <param name="BookId">The Id of the book whose resource is retrieved. Required.</param>
/// <param name="ResourceKey">The opaque resource key of the resource. Required.</param>
[DebuggerDisplay("BookId: {BookId}, ResourceKey: {ResourceKey}")]
public sealed record GetReadingResourceRequest(
    Guid BookId,
    string ResourceKey
);
