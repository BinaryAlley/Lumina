#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.DataAccess.Common.Persistence;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Persistence;

/// <summary>
/// Contains unit tests for the <see cref="SharedReferenceResolver"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SharedReferenceResolverTests
{
    private readonly LuminaDbContext _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();

    [Fact]
    public async Task ResolveAsync_WhenANameIsNotStored_ShouldReturnTheIncomingInstance()
    {
        // Arrange
        TagEntity incomingTag = _tagEntityFixture.Create(name: "jazz");

        // Act
        Result<IReadOnlyDictionary<string, TagEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync(_mockContext, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(resolveResult.IsFailure);
        Assert.Same(incomingTag, resolveResult.Value["jazz"]);
    }

    [Fact]
    public async Task ResolveAsync_WhenANameIsAlreadyStoredAndTracked_ShouldReturnTheTrackedInstance()
    {
        // Arrange
        TagEntity storedTag = _tagEntityFixture.Create(name: "jazz");
        _mockContext.Set<TagEntity>().Add(storedTag);
        await _mockContext.SaveChangesAsync();

        TagEntity incomingTag = _tagEntityFixture.Create(name: "jazz");

        // Act
        Result<IReadOnlyDictionary<string, TagEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync(_mockContext, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(resolveResult.IsFailure);
        Assert.Same(storedTag, resolveResult.Value["jazz"]);
    }

    [Fact]
    public async Task ResolveAsync_WhenANameIsAlreadyStoredButNotTracked_ShouldLoadTheStoredInstanceFromStorage()
    {
        // Arrange
        TagEntity storedTag = _tagEntityFixture.Create(name: "jazz");
        _mockContext.Set<TagEntity>().Add(storedTag);
        await _mockContext.SaveChangesAsync();

        // Detach the stored row, so that the resolution cannot take the tracked shortcut and must read it from the storage medium.
        _mockContext.ChangeTracker.Clear();
        TagEntity incomingTag = _tagEntityFixture.Create(name: "jazz");

        // Act
        Result<IReadOnlyDictionary<string, TagEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync(_mockContext, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(resolveResult.IsFailure);
        TagEntity resolvedTag = resolveResult.Value["jazz"];
        // A different instance than the incoming one proves the row was materialized from the storage medium instead of the tracked shortcut.
        Assert.NotSame(incomingTag, resolvedTag);
        Assert.Equal("jazz", resolvedTag.Name);
    }

    [Fact]
    public async Task ResolveAsync_WhenTheSameNameAppearsMoreThanOnce_ShouldCollapseItToASingleInstance()
    {
        // Arrange
        TagEntity firstTag = _tagEntityFixture.Create(name: "jazz");
        TagEntity secondTag = _tagEntityFixture.Create(name: "jazz");

        // Act
        Result<IReadOnlyDictionary<string, TagEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync(_mockContext, [firstTag, secondTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(resolveResult.IsFailure);
        Assert.Single(resolveResult.Value);
        Assert.Same(firstTag, resolveResult.Value["jazz"]);
    }

    [Fact]
    public async Task ResolveAsync_WhenThereAreNoIncomingEntities_ShouldReturnAnEmptyDictionary()
    {
        // Act
        Result<IReadOnlyDictionary<string, GenreEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync<GenreEntity>(_mockContext, [], Errors.Metadata.GenreNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(resolveResult.IsFailure);
        Assert.Empty(resolveResult.Value);
    }

    [Fact]
    public async Task ReconcileAsync_WhenCalledWithAnotherSharedReferenceType_ShouldReconcileItTheSameWay()
    {
        // Arrange
        List<GenreEntity> trackedGenres = [_genreEntityFixture.Create(name: "rock")];
        GenreEntity incomingGenre = _genreEntityFixture.Create(name: "jazz");

        // Act
        Result<Updated> result = await SharedReferenceResolver.ReconcileAsync(_mockContext, trackedGenres, [incomingGenre], Errors.Metadata.GenreNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(incomingGenre, Assert.Single(trackedGenres));
    }

    [Fact]
    public async Task Normalize_WhenCalled_ShouldMapEveryEntityToItsResolvedInstance()
    {
        // Arrange
        TagEntity firstTag = _tagEntityFixture.Create(name: "jazz");
        TagEntity secondTag = _tagEntityFixture.Create(name: "jazz");
        Result<IReadOnlyDictionary<string, TagEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync(_mockContext, [firstTag, secondTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Act
        IReadOnlyCollection<TagEntity> normalizedTags = SharedReferenceResolver.Normalize([firstTag, secondTag], resolveResult.Value);

        // Assert
        TagEntity normalizedTag = Assert.Single(normalizedTags);
        Assert.Same(firstTag, normalizedTag);
    }

    [Fact]
    public async Task ReconcileAsync_WhenANameIsNew_ShouldAddItToTheTrackedCollection()
    {
        // Arrange
        List<TagEntity> trackedTags = [];
        TagEntity incomingTag = _tagEntityFixture.Create(name: "jazz");

        // Act
        Result<Updated> result = await SharedReferenceResolver.ReconcileAsync(_mockContext, trackedTags, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(incomingTag, Assert.Single(trackedTags));
    }

    [Fact]
    public async Task ReconcileAsync_WhenANameIsAlreadyStored_ShouldReuseTheStoredInstance()
    {
        // Arrange
        TagEntity storedTag = _tagEntityFixture.Create(name: "jazz");
        _mockContext.Set<TagEntity>().Add(storedTag);
        await _mockContext.SaveChangesAsync();

        List<TagEntity> trackedTags = [];
        TagEntity incomingTag = _tagEntityFixture.Create(name: "jazz");

        // Act
        Result<Updated> result = await SharedReferenceResolver.ReconcileAsync(_mockContext, trackedTags, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(storedTag, Assert.Single(trackedTags));
    }

    [Fact]
    public async Task ReconcileAsync_WhenATrackedNameIsMissingFromTheIncomingSet_ShouldRemoveIt()
    {
        // Arrange
        TagEntity removedTag = _tagEntityFixture.Create(name: "rock");
        TagEntity keptTag = _tagEntityFixture.Create(name: "jazz");
        List<TagEntity> trackedTags = [removedTag, keptTag];
        TagEntity incomingTag = _tagEntityFixture.Create(name: "jazz");

        // Act
        Result<Updated> result = await SharedReferenceResolver.ReconcileAsync(_mockContext, trackedTags, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(keptTag, Assert.Single(trackedTags));
        Assert.DoesNotContain(removedTag, trackedTags);
    }

    [Fact]
    public async Task ReconcileAsync_WhenTheSameNewNameAppearsMoreThanOnce_ShouldTrackASingleInstance()
    {
        // Arrange
        List<TagEntity> trackedTags = [];
        TagEntity firstTag = _tagEntityFixture.Create(name: "jazz");
        TagEntity secondTag = _tagEntityFixture.Create(name: "jazz");

        // Act
        Result<Updated> result = await SharedReferenceResolver.ReconcileAsync(_mockContext, trackedTags, [firstTag, secondTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(firstTag, Assert.Single(trackedTags));
    }

    [Fact]
    public async Task ResolveAsync_WhenANameIsNull_ShouldReturnNameCannotBeEmptyError()
    {
        // Arrange
        TagEntity incomingTag = _tagEntityFixture.Create(includeName: false);

        // Act
        Result<IReadOnlyDictionary<string, TagEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync(_mockContext, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.True(resolveResult.IsFailure);
        Assert.Equal(Errors.Metadata.TagNameCannotBeEmpty, resolveResult.FirstError);
    }

    [Fact]
    public async Task ResolveAsync_WhenANameIsWhitespace_ShouldReturnNameCannotBeEmptyError()
    {
        // Arrange
        TagEntity incomingTag = _tagEntityFixture.Create(name: "   ");

        // Act
        Result<IReadOnlyDictionary<string, TagEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync(_mockContext, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.True(resolveResult.IsFailure);
        Assert.Equal(Errors.Metadata.TagNameCannotBeEmpty, resolveResult.FirstError);
    }

    [Fact]
    public async Task ResolveAsync_WhenTheNameIsAlreadyTrackedButNotStored_ShouldReturnTheTrackedInstance()
    {
        // Arrange
        TagEntity trackedTag = _tagEntityFixture.Create(name: "jazz");
        _mockContext.Set<TagEntity>().Add(trackedTag);

        TagEntity incomingTag = _tagEntityFixture.Create(name: "jazz");

        // Act
        Result<IReadOnlyDictionary<string, TagEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync(_mockContext, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.False(resolveResult.IsFailure);
        Assert.Same(trackedTag, resolveResult.Value["jazz"]);
    }

    [Fact]
    public async Task ReconcileAsync_WhenANameIsNull_ShouldReturnNameCannotBeEmptyErrorWithoutChangingTheTrackedCollection()
    {
        // Arrange
        List<TagEntity> trackedTags = [];
        TagEntity incomingTag = _tagEntityFixture.Create(includeName: false);

        // Act
        Result<Updated> result = await SharedReferenceResolver.ReconcileAsync(_mockContext, trackedTags, [incomingTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.TagNameCannotBeEmpty, result.FirstError);
        Assert.Empty(trackedTags);
    }

    [Fact]
    public async Task Normalize_WhenAnEntityCannotBeResolved_ShouldSkipIt()
    {
        // Arrange
        TagEntity namedTag = _tagEntityFixture.Create(name: "jazz");
        TagEntity unnamedTag = _tagEntityFixture.Create(includeName: false);
        Result<IReadOnlyDictionary<string, TagEntity>> resolveResult = await SharedReferenceResolver.ResolveAsync(_mockContext, [namedTag], Errors.Metadata.TagNameCannotBeEmpty, CancellationToken.None);

        // Act
        IReadOnlyCollection<TagEntity> normalizedTags = SharedReferenceResolver.Normalize([namedTag, unnamedTag], resolveResult.Value);

        // Assert
        Assert.Same(namedTag, Assert.Single(normalizedTags));
    }
}
