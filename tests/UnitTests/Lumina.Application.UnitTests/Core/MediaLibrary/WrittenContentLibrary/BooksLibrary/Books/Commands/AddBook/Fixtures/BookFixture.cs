#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;

/// <summary>
/// Fixture class for the <see cref="Book"/> domain entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookFixture
{
    private readonly Fixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookFixture"/> class.
    /// </summary>
    public BookFixture()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
        _fixture.Customizations.Add(new NullableDateOnlySpecimenBuilder());
        ConfigureCustomDomainTypes();
    }

    /// <summary>
    /// Creates a random valid book domain entity.
    /// </summary>
    /// <returns>The created book domain entity.</returns>
    public Book CreateDomainBook()
    {
        return Book.Create(
            _fixture.Create<WrittenContentMetadata>(),
            _fixture.Create<BookFormat>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<int>>(),
            _fixture.Create<Optional<BookSeries>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<List<Isbn>>(),
            _fixture.Create<List<MediaContributorId>>(),
            _fixture.Create<List<BookRating>>()
        ).Value;
    }

    private void ConfigureCustomDomainTypes()
    {
        _fixture.Register(() => Optional<string>.Some(_fixture.Create<string>()));
        _fixture.Register(() => Optional<int>.Some(_fixture.Create<int>()));

        _fixture.Register(() =>
        {
            int originalYear = _fixture.Create<Generator<int>>().First(y => y >= 1900 && y <= 2025);
            int reReleaseYear = _fixture.Create<Generator<int>>().First(y => y >= originalYear && y <= 2025);

            return ReleaseInfo.Create(
                Optional<DateOnly>.Some(DateOnly.FromDateTime(new DateTime(originalYear, 1, 1))),
                Optional<int>.Some(originalYear),
                Optional<DateOnly>.Some(DateOnly.FromDateTime(new DateTime(reReleaseYear, 1, 1))),
                Optional<int>.Some(reReleaseYear),
                _fixture.Create<Optional<string>>(),
                _fixture.Create<Optional<string>>()
            ).Value;
        });

        _fixture.Register(() => Genre.Create(
            _fixture.Create<string>()
        ).Value);

        _fixture.Register(() => Tag.Create(
            _fixture.Create<string>()
        ).Value);

        _fixture.Register(() => WrittenContentMetadata.Create(
            _fixture.Create<string>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<ReleaseInfo>(),
            _fixture.Create<List<Genre>>(),
            _fixture.Create<List<Tag>>(),
            _fixture.Create<Optional<LanguageInfo>>(),
            _fixture.Create<Optional<LanguageInfo>>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<int>>()
        ).Value);

        _fixture.Register(() => Isbn.Create(
            _fixture.Create<string>(),
            _fixture.Create<IsbnFormat>()
        ).Value);

        _fixture.Register(() => BookRating.Create(
            _fixture.Create<decimal>(),
            _fixture.Create<decimal>(),
            _fixture.Create<Optional<BookRatingSource>>(),
            _fixture.Create<Optional<int>>()
        ).Value);
    }
}
