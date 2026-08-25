#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Common.Setup;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;

/// <summary>
/// Fixture class for the <see cref="Book"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookFixture
{
    private readonly Fixture _fixture = new();
    private readonly IsbnFixture _isbnFixture = new();
    private readonly BookRatingFixture _bookRatingFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookFixture"/> class.
    /// </summary>
    public BookFixture()
    {
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
        _fixture.Customizations.Add(new NullableDateOnlySpecimenBuilder());
        ConfigureCustomDomainTypes();
    }

    /// <summary>
    /// Creates a random valid <see cref="Book"/> domain aggregate.
    /// </summary>
    /// <returns>The created <see cref="Book"/> domain aggregate.</returns>
    public Book Create()
    {
        return Book.Create(
            LibraryId.Create(_fixture.Create<Guid>()),
            _fixture.Create<string>(),
            _fixture.Create<WrittenContentMetadata>(),
            _fixture.Create<BookFormat>(),
            _fixture.Create<Optional<string>>(),
            _fixture.Create<Optional<float>>(),
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

    /// <summary>
    /// Creates multiple <see cref="Book"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Book"/> instances.</returns>
    public List<Book> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    private void ConfigureCustomDomainTypes()
    {
        _fixture.Register(() => Optional<string>.Some(_fixture.Create<string>()));
        _fixture.Register(() => Optional<int>.Some(_fixture.Create<int>()));

        _fixture.Register(() =>
        {
            int originalYear = _fixture.Create<Generator<int>>().First(year => year >= 1900 && year <= 2025);
            int reReleaseYear = _fixture.Create<Generator<int>>().First(year => year >= originalYear && year <= 2025);

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

        _fixture.Register(() => _isbnFixture.Create());

        _fixture.Register(() => _bookRatingFixture.Create());
    }
}
