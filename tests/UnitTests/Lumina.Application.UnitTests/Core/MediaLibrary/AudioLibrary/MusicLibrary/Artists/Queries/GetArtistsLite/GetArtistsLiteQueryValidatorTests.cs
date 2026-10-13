#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistsLiteQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsLiteQueryValidatorTests
{
    private readonly GetArtistsLiteQueryFixture _getArtistsLiteQueryFixture = new();
    private readonly GetArtistsLiteQueryValidator _validator = new();

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-a-library-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        GetArtistsLiteQuery query = _getArtistsLiteQueryFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetArtistsLiteQuery query = _getArtistsLiteQueryFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        GetArtistsLiteQuery query = _getArtistsLiteQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("AB")] // more than one character
    [InlineData("1")] // a digit, which is not a letter, the number bucket, or the symbol bucket
    public void Validate_WhenFilterAlphaKeyIsNotASingleLetterOrSpecialKey_ShouldHaveValidationError(string filterAlphaKey)
    {
        // Arrange
        GetArtistsLiteQuery query = _getArtistsLiteQueryFixture.Create(filterAlphaKey: filterAlphaKey);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.InvalidFilterAlphaKey);
    }

    [Theory]
    [InlineData("Q")] // a single letter
    [InlineData("q")] // a single lower case letter
    [InlineData("#")] // the number bucket
    [InlineData("*")] // the symbol bucket
    public void Validate_WhenFilterAlphaKeyIsASingleLetterOrSpecialKey_ShouldNotHaveValidationError(string filterAlphaKey)
    {
        // Arrange
        GetArtistsLiteQuery query = _getArtistsLiteQueryFixture.Create(filterAlphaKey: filterAlphaKey);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenFilterAlphaKeyIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        GetArtistsLiteQuery query = _getArtistsLiteQueryFixture.Create(filterAlphaKey: null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(LibraryItemAlphaKeys.NUMBER)] // the number bucket constant
    [InlineData(LibraryItemAlphaKeys.SYMBOL)] // the symbol bucket constant
    public void Validate_WhenFilterAlphaKeyIsTheSpecialConstant_ShouldNotHaveValidationError(string filterAlphaKey)
    {
        // Arrange
        GetArtistsLiteQuery query = _getArtistsLiteQueryFixture.Create(filterAlphaKey: filterAlphaKey);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("A")] // upper case A
    [InlineData("B")] // upper case B
    [InlineData("C")] // upper case C
    [InlineData("D")] // upper case D
    [InlineData("E")] // upper case E
    [InlineData("F")] // upper case F
    [InlineData("G")] // upper case G
    [InlineData("H")] // upper case H
    [InlineData("I")] // upper case I
    [InlineData("J")] // upper case J
    [InlineData("K")] // upper case K
    [InlineData("L")] // upper case L
    [InlineData("M")] // upper case M
    [InlineData("N")] // upper case N
    [InlineData("O")] // upper case O
    [InlineData("P")] // upper case P
    [InlineData("Q")] // upper case Q
    [InlineData("R")] // upper case R
    [InlineData("S")] // upper case S
    [InlineData("T")] // upper case T
    [InlineData("U")] // upper case U
    [InlineData("V")] // upper case V
    [InlineData("W")] // upper case W
    [InlineData("X")] // upper case X
    [InlineData("Y")] // upper case Y
    [InlineData("Z")] // upper case Z
    [InlineData("a")] // lower case a
    [InlineData("b")] // lower case b
    [InlineData("c")] // lower case c
    [InlineData("d")] // lower case d
    [InlineData("e")] // lower case e
    [InlineData("f")] // lower case f
    [InlineData("g")] // lower case g
    [InlineData("h")] // lower case h
    [InlineData("i")] // lower case i
    [InlineData("j")] // lower case j
    [InlineData("k")] // lower case k
    [InlineData("l")] // lower case l
    [InlineData("m")] // lower case m
    [InlineData("n")] // lower case n
    [InlineData("o")] // lower case o
    [InlineData("p")] // lower case p
    [InlineData("q")] // lower case q
    [InlineData("r")] // lower case r
    [InlineData("s")] // lower case s
    [InlineData("t")] // lower case t
    [InlineData("u")] // lower case u
    [InlineData("v")] // lower case v
    [InlineData("w")] // lower case w
    [InlineData("x")] // lower case x
    [InlineData("y")] // lower case y
    [InlineData("z")] // lower case z
    public void Validate_WhenFilterAlphaKeyIsAnySingleAsciiLetter_ShouldNotHaveValidationError(string filterAlphaKey)
    {
        // Arrange
        GetArtistsLiteQuery query = _getArtistsLiteQueryFixture.Create(filterAlphaKey: filterAlphaKey);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")] // empty string, which is not a single letter or a special key
    [InlineData("AB")] // two letters
    [InlineData("ABC")] // several letters
    [InlineData("1")] // a digit, which is not a letter, a number bucket or a symbol bucket
    [InlineData("9")] // another digit
    [InlineData("_")] // an underscore, which is not an ASCII letter
    [InlineData("é")] // a non-ASCII letter
    [InlineData("#!")] // the number bucket followed by another character
    [InlineData("**")] // two symbol buckets, which is not the exact symbol constant
    [InlineData("#A")] // the number bucket followed by a letter
    [InlineData("*A")] // the symbol bucket followed by a letter
    [InlineData(" A")] // a leading space followed by a letter
    [InlineData("A ")] // a letter followed by a trailing space
    public void Validate_WhenFilterAlphaKeyIsNotAValidAlphaKey_ShouldHaveValidationError(string filterAlphaKey)
    {
        // Arrange
        GetArtistsLiteQuery query = _getArtistsLiteQueryFixture.Create(filterAlphaKey: filterAlphaKey);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.InvalidFilterAlphaKey);
    }
}
