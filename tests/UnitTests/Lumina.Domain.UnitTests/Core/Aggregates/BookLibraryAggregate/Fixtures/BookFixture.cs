#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.BookLibraryAggregate.Fixtures;

/// <summary>
/// Fixture class for the <see cref="Book"/> domain entity.
/// </summary>
[ExcludeFromCodeCoverage]
public static class BookFixture
{
    /// <summary>
    /// Creates a valid book domain entity with random data.
    /// </summary>
    /// <returns>The created <see cref="Book"/> domain entity.</returns>
    public static Book CreateValidBook()
    {
        Result<Book> bookResult = Book.Create(
            LibraryId.Create(Guid.NewGuid()),
            "/books/test.epub",
            WrittenContentMetadata.Create(
                "Test Title",
                Optional<string>.None(),
                Optional<string>.None(),
                ReleaseInfo.Create(
                    Optional<DateOnly>.None(),
                    Optional<int>.None(),
                    Optional<DateOnly>.None(),
                    Optional<int>.None(),
                    Optional<string>.None(),
                    Optional<string>.None()
                ).Value,
                [],
                [],
                Optional<LanguageInfo>.None(),
                Optional<LanguageInfo>.None(),
                Optional<string>.None(),
                Optional<int>.None()
            ).Value,
            Optional<BookFormat>.None(),
            Optional<string>.None(),
            Optional<int>.None(),
            Optional<BookSeries>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            [],
            [],
            []
        );
        if (bookResult.IsFailure)
            throw new InvalidOperationException("Failed to create a valid book in the test fixture.");
        return bookResult.Value;
    }
}
