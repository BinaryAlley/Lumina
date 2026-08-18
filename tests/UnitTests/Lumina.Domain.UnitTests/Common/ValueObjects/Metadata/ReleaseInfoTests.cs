#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.ValueObjects.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="ReleaseInfo"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReleaseInfoTests
{
    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateReleaseInfo()
    {
        // Act
        Result<ReleaseInfo> result = ReleaseInfo.Create(
            Optional<DateOnly>.Some(new DateOnly(2001, 9, 14)),
            Optional<int>.Some(2001),
            Optional<DateOnly>.Some(new DateOnly(2010, 6, 1)),
            Optional<int>.Some(2010),
            Optional<string>.Some("US"),
            Optional<string>.Some("Director's Cut"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(new DateOnly(2001, 9, 14), result.Value.OriginalReleaseDate.Value);
        Assert.Equal(2001, result.Value.OriginalReleaseYear.Value);
        Assert.Equal(new DateOnly(2010, 6, 1), result.Value.ReReleaseDate.Value);
        Assert.Equal(2010, result.Value.ReReleaseYear.Value);
        Assert.Equal("US", result.Value.ReleaseCountry.Value);
        Assert.Equal("Director's Cut", result.Value.ReleaseVersion.Value);
    }

    [Fact]
    public void Create_WhenOnlyOriginalReleaseYearProvided_ShouldCreateReleaseInfo()
    {
        // Act
        Result<ReleaseInfo> result = ReleaseInfo.Create(
            Optional<DateOnly>.None(),
            Optional<int>.Some(2001),
            Optional<DateOnly>.None(),
            Optional<int>.None(),
            Optional<string>.None(),
            Optional<string>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2001, result.Value.OriginalReleaseYear.Value);
        Assert.False(result.Value.OriginalReleaseDate.HasValue);
    }

    [Fact]
    public void Create_WhenOriginalReleaseDateAndYearMismatch_ShouldReturnError()
    {
        // Act
        Result<ReleaseInfo> result = ReleaseInfo.Create(
            Optional<DateOnly>.Some(new DateOnly(2001, 9, 14)),
            Optional<int>.Some(2002),
            Optional<DateOnly>.None(),
            Optional<int>.None(),
            Optional<string>.None(),
            Optional<string>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.OriginalReleaseDateAndYearMustMatch, result.FirstError);
    }

    [Fact]
    public void Create_WhenReReleaseDateAndYearMismatch_ShouldReturnError()
    {
        // Act
        Result<ReleaseInfo> result = ReleaseInfo.Create(
            Optional<DateOnly>.None(),
            Optional<int>.None(),
            Optional<DateOnly>.Some(new DateOnly(2010, 6, 1)),
            Optional<int>.Some(2011),
            Optional<string>.None(),
            Optional<string>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.ReReleaseDateAndYearMustMatch, result.FirstError);
    }

    [Fact]
    public void Create_WhenReReleaseDateIsEarlierThanOriginalReleaseDate_ShouldReturnError()
    {
        // Act
        Result<ReleaseInfo> result = ReleaseInfo.Create(
            Optional<DateOnly>.Some(new DateOnly(2010, 6, 1)),
            Optional<int>.Some(2010),
            Optional<DateOnly>.Some(new DateOnly(2001, 9, 14)),
            Optional<int>.Some(2001),
            Optional<string>.None(),
            Optional<string>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate, result.FirstError);
    }

    [Fact]
    public void Create_WhenReReleaseYearIsEarlierThanOriginalReleaseYear_ShouldReturnError()
    {
        // Act
        Result<ReleaseInfo> result = ReleaseInfo.Create(
            Optional<DateOnly>.None(),
            Optional<int>.Some(2010),
            Optional<DateOnly>.None(),
            Optional<int>.Some(2001),
            Optional<string>.None(),
            Optional<string>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear, result.FirstError);
    }
}
