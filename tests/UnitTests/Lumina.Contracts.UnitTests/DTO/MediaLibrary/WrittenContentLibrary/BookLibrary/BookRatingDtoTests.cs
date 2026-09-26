#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

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
        BookRatingDto sut = _bookRatingDtoFixture.Create(includeOptionalProperties: false);

        // Assert
        Assert.Null(sut.Source);
        Assert.Null(sut.VoteCount);
    }
}
