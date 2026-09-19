#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Common.Persistence;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Persistence;

/// <summary>
/// Contains unit tests for the <see cref="CollectionReconciler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CollectionReconcilerTests
{
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly IsbnEntityFixture _isbnEntityFixture = new();
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();

    [Fact]
    public void Reconcile_WhenAnItemIsNew_ShouldAddItToTheTrackedCollection()
    {
        // Arrange
        List<TagEntity> trackedTags = [];
        List<TagEntity> incomingTags = [_tagEntityFixture.Create(name: "jazz")];

        // Act
        CollectionReconciler.Reconcile(
            trackedTags,
            incomingTags,
            trackedTag => trackedTag.Name!,
            incomingTag => incomingTag.Name!,
            shouldReplace: (trackedTag, incomingTag) => false,
            createNew: incomingTag => incomingTag);

        // Assert
        TagEntity addedTag = Assert.Single(trackedTags);
        Assert.Same(incomingTags[0], addedTag);
    }

    [Fact]
    public void Reconcile_WhenATrackedItemIsAbsentFromTheIncomingSet_ShouldRemoveIt()
    {
        // Arrange
        TagEntity removedTag = _tagEntityFixture.Create(name: "rock");
        TagEntity keptTag = _tagEntityFixture.Create(name: "jazz");
        List<TagEntity> trackedTags = [removedTag, keptTag];
        List<TagEntity> incomingTags = [_tagEntityFixture.Create(name: "jazz")];

        // Act
        CollectionReconciler.Reconcile(
            trackedTags,
            incomingTags,
            trackedTag => trackedTag.Name!,
            incomingTag => incomingTag.Name!,
            shouldReplace: (trackedTag, incomingTag) => false,
            createNew: incomingTag => incomingTag);

        // Assert
        TagEntity keptTrackedTag = Assert.Single(trackedTags);
        Assert.Same(keptTag, keptTrackedTag);
        Assert.DoesNotContain(removedTag, trackedTags);
    }

    [Fact]
    public void Reconcile_WhenAMatchedItemDidNotChange_ShouldKeepTheSameTrackedInstance()
    {
        // Arrange
        TagEntity trackedTag = _tagEntityFixture.Create(name: "rock");
        List<TagEntity> trackedTags = [trackedTag];
        List<TagEntity> incomingTags = [_tagEntityFixture.Create(name: "rock")];

        // Act
        CollectionReconciler.Reconcile(
            trackedTags,
            incomingTags,
            existingTag => existingTag.Name!,
            incomingTag => incomingTag.Name!,
            shouldReplace: (existingTag, incomingTag) => false,
            createNew: incomingTag => incomingTag);

        // Assert
        TagEntity keptTrackedTag = Assert.Single(trackedTags);
        Assert.Same(trackedTag, keptTrackedTag);
    }

    [Fact]
    public void Reconcile_WhenAMatchedItemChanged_ShouldReplaceOnlyThatItem()
    {
        // Arrange
        IsbnEntity trackedIsbn = _isbnEntityFixture.Create(value: "978-3-16-148410-0", format: IsbnFormat.Isbn10);
        IsbnEntity unchangedIsbn = _isbnEntityFixture.Create(value: "978-0-306-40615-7", format: IsbnFormat.Isbn13);
        List<IsbnEntity> trackedIsbns = [trackedIsbn, unchangedIsbn];
        IsbnEntity incomingChangedIsbn = _isbnEntityFixture.Create(value: "978-3-16-148410-0", format: IsbnFormat.Isbn13);
        IsbnEntity incomingUnchangedIsbn = _isbnEntityFixture.Create(value: "978-0-306-40615-7", format: IsbnFormat.Isbn13);
        List<IsbnEntity> incomingIsbns = [incomingChangedIsbn, incomingUnchangedIsbn];

        // Act
        CollectionReconciler.Reconcile(
            trackedIsbns,
            incomingIsbns,
            existingIsbn => existingIsbn.Value!,
            incomingIsbn => incomingIsbn.Value!,
            shouldReplace: (existingIsbn, incomingIsbn) => !existingIsbn.Equals(incomingIsbn),
            createNew: incomingIsbn => incomingIsbn);

        // Assert
        Assert.Equal(2, trackedIsbns.Count);
        Assert.DoesNotContain(trackedIsbn, trackedIsbns);
        Assert.Contains(incomingChangedIsbn, trackedIsbns);
        IsbnEntity keptIsbn = trackedIsbns.Single(isbn => isbn.Value == unchangedIsbn.Value);
        Assert.Same(unchangedIsbn, keptIsbn);
    }

    [Fact]
    public void Reconcile_WhenSeveralChangesAreRequested_ShouldApplyAddRemoveAndKeepInASinglePass()
    {
        // Arrange
        BookContributorEntity removedContributor = _bookContributorEntityFixture.Create(role: MediaContributorRole.Author);
        BookContributorEntity keptContributor = _bookContributorEntityFixture.Create(role: MediaContributorRole.Illustrator);
        List<BookContributorEntity> trackedContributors = [removedContributor, keptContributor];
        BookContributorEntity incomingKeptContributor = _bookContributorEntityFixture.Create(
            bookId: keptContributor.BookId,
            mediaContributorId: keptContributor.MediaContributorId,
            role: keptContributor.Role);
        BookContributorEntity incomingNewContributor = _bookContributorEntityFixture.Create(role: MediaContributorRole.Translator);
        List<BookContributorEntity> incomingContributors = [incomingKeptContributor, incomingNewContributor];

        // Act
        CollectionReconciler.Reconcile(
            trackedContributors,
            incomingContributors,
            existingContributor => (existingContributor.MediaContributorId, existingContributor.Role),
            incomingContributor => (incomingContributor.MediaContributorId, incomingContributor.Role),
            shouldReplace: (existingContributor, incomingContributor) => false,
            createNew: incomingContributor => incomingContributor);

        // Assert
        Assert.Equal(2, trackedContributors.Count);
        Assert.DoesNotContain(removedContributor, trackedContributors);
        Assert.Same(keptContributor, trackedContributors.Single(contributor => contributor.MediaContributorId == keptContributor.MediaContributorId));
        Assert.Same(incomingNewContributor, trackedContributors.Single(contributor => contributor.MediaContributorId == incomingNewContributor.MediaContributorId));
    }
}
