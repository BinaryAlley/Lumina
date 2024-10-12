#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Controllers.Books.Fixtures;

/// <summary>
/// Fixture class for the <see cref="Book"/> domain entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class DomainBookFixture
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly Fixture _fixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainBookFixture"/> class.
    /// </summary>
    public DomainBookFixture()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
        _fixture.Customizations.Add(new NullableDateOnlySpecimenBuilder());
        ConfigureCustomDomainTypes();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
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

        _fixture.Register(() => ReleaseInfo.Create(
                _fixture.Create<Optional<DateOnly>>(),
                _fixture.Create<Optional<int>>(),
                _fixture.Create<Optional<DateOnly>>(),
                _fixture.Create<Optional<int>>(),
                _fixture.Create<Optional<string>>(),
                _fixture.Create<Optional<string>>()
            ).Value);

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
    #endregion
}