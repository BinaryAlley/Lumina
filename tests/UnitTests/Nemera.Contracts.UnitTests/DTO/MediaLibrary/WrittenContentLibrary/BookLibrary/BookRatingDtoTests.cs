#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="BookRatingDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingDtoTests
{
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();

    [Fact]
    public void Create_WhenOmittingOptionalProperties_ShouldReturnNullSourceAndVoteCount()
    {
        // Act
        BookRatingDto sut = _bookRatingDtoFixture.Create(includeSource: false, includeVoteCount: false);

        // Assert
        Assert.Null(sut.Source);
        Assert.Null(sut.VoteCount);
    }
}
