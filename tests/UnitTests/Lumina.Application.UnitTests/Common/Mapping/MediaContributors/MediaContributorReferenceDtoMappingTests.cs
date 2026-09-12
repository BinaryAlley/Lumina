#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaContributors;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributorReferenceDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorReferenceDtoMappingTests
{
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();

    [Fact]
    public void ToBookDomainEntity_WhenMappingCompleteMediaContributorReferenceDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid contributorId = Guid.NewGuid();
        MediaContributorReferenceDto dto = _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Author);

        // Act
        Result<BookMediaContributor> result = dto.ToBookDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(dto.ContributorId, result.Value.ContributorId.Value);
        Assert.Equal(dto.Role, result.Value.Role);
    }

    [Fact]
    public void ToBookDomainEntities_WhenMappingMultipleValidMediaContributorReferenceDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<MediaContributorReferenceDto> dtos =
        [
            _mediaContributorReferenceDtoFixture.Create(role: MediaContributorRole.Author),
            _mediaContributorReferenceDtoFixture.Create(role: MediaContributorRole.Illustrator)
        ];

        // Act
        IEnumerable<Result<BookMediaContributor>> results = dtos.ToBookDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<Result<BookMediaContributor>> resultList = [.. results];
        Assert.All(resultList, result => Assert.False(result.IsFailure));
        Assert.Equal(dtos[0].ContributorId, resultList[0].Value.ContributorId.Value);
        Assert.Equal(dtos[0].Role, resultList[0].Value.Role);
        Assert.Equal(dtos[1].ContributorId, resultList[1].Value.ContributorId.Value);
        Assert.Equal(dtos[1].Role, resultList[1].Value.Role);
    }

    [Fact]
    public void ToMusicDomainEntity_WhenMappingCompleteMediaContributorReferenceDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid contributorId = Guid.NewGuid();
        MediaContributorReferenceDto dto = _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Producer);

        // Act
        Result<MusicMediaContributor> result = dto.ToMusicDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(dto.ContributorId, result.Value.ContributorId.Value);
        Assert.Equal(dto.Role, result.Value.Role);
    }

    [Fact]
    public void ToMusicDomainEntities_WhenMappingMultipleValidMediaContributorReferenceDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<MediaContributorReferenceDto> dtos =
        [
            _mediaContributorReferenceDtoFixture.Create(role: MediaContributorRole.Vocals),
            _mediaContributorReferenceDtoFixture.Create(role: MediaContributorRole.Guitar)
        ];

        // Act
        IEnumerable<Result<MusicMediaContributor>> results = dtos.ToMusicDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<Result<MusicMediaContributor>> resultList = [.. results];
        Assert.All(resultList, result => Assert.False(result.IsFailure));
        Assert.Equal(dtos[0].ContributorId, resultList[0].Value.ContributorId.Value);
        Assert.Equal(dtos[0].Role, resultList[0].Value.Role);
        Assert.Equal(dtos[1].ContributorId, resultList[1].Value.ContributorId.Value);
        Assert.Equal(dtos[1].Role, resultList[1].Value.Role);
    }
}
