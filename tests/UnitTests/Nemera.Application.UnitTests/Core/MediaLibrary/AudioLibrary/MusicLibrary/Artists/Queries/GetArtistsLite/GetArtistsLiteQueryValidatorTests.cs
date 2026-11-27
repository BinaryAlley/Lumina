#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistsLiteQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsLiteQueryValidatorTests
{
    private readonly GetArtistsLiteQueryFixture _getArtistsLiteQueryFixture = new();
    private readonly GetArtistsLiteQueryValidator _validator = new();

    [Theory]
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("   ")] // Whitespace route value, which both the not empty rule and the format rule reject.
    [InlineData("not-a-library-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
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
    [InlineData("AB")] // More than one character.
    [InlineData("1")] // A digit, which is not a letter, the number bucket, or the symbol bucket.
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
    [InlineData("Q")] // A single letter.
    [InlineData("q")] // A single lower case letter.
    [InlineData("#")] // The number bucket.
    [InlineData("*")] // The symbol bucket.
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
    [InlineData(LibraryItemAlphaKeys.NUMBER)] // The number bucket constant.
    [InlineData(LibraryItemAlphaKeys.SYMBOL)] // The symbol bucket constant.
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
    [InlineData("A")] // Upper case A.
    [InlineData("B")] // Upper case B.
    [InlineData("C")] // Upper case C.
    [InlineData("D")] // Upper case D.
    [InlineData("E")] // Upper case E.
    [InlineData("F")] // Upper case F.
    [InlineData("G")] // Upper case G.
    [InlineData("H")] // Upper case H.
    [InlineData("I")] // Upper case I.
    [InlineData("J")] // Upper case J.
    [InlineData("K")] // Upper case K.
    [InlineData("L")] // Upper case L.
    [InlineData("M")] // Upper case M.
    [InlineData("N")] // Upper case N.
    [InlineData("O")] // Upper case O.
    [InlineData("P")] // Upper case P.
    [InlineData("Q")] // Upper case Q.
    [InlineData("R")] // Upper case R.
    [InlineData("S")] // Upper case S.
    [InlineData("T")] // Upper case T.
    [InlineData("U")] // Upper case U.
    [InlineData("V")] // Upper case V.
    [InlineData("W")] // Upper case W.
    [InlineData("X")] // Upper case X.
    [InlineData("Y")] // Upper case Y.
    [InlineData("Z")] // Upper case Z.
    [InlineData("a")] // Lower case a.
    [InlineData("b")] // Lower case b.
    [InlineData("c")] // Lower case c.
    [InlineData("d")] // Lower case d.
    [InlineData("e")] // Lower case e.
    [InlineData("f")] // Lower case f.
    [InlineData("g")] // Lower case g.
    [InlineData("h")] // Lower case h.
    [InlineData("i")] // Lower case i.
    [InlineData("j")] // Lower case j.
    [InlineData("k")] // Lower case k.
    [InlineData("l")] // Lower case l.
    [InlineData("m")] // Lower case m.
    [InlineData("n")] // Lower case n.
    [InlineData("o")] // Lower case o.
    [InlineData("p")] // Lower case p.
    [InlineData("q")] // Lower case q.
    [InlineData("r")] // Lower case r.
    [InlineData("s")] // Lower case s.
    [InlineData("t")] // Lower case t.
    [InlineData("u")] // Lower case u.
    [InlineData("v")] // Lower case v.
    [InlineData("w")] // Lower case w.
    [InlineData("x")] // Lower case x.
    [InlineData("y")] // Lower case y.
    [InlineData("z")] // Lower case z.
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
    [InlineData("")] // Empty string, which is not a single letter or a special key.
    [InlineData("AB")] // Two letters.
    [InlineData("ABC")] // Several letters.
    [InlineData("1")] // A digit, which is not a letter, a number bucket or a symbol bucket.
    [InlineData("9")] // Another digit.
    [InlineData("_")] // An underscore, which is not an ASCII letter.
    [InlineData("é")] // A non-ASCII letter.
    [InlineData("#!")] // The number bucket followed by another character.
    [InlineData("**")] // Two symbol buckets, which is not the exact symbol constant.
    [InlineData("#A")] // The number bucket followed by a letter.
    [InlineData("*A")] // The symbol bucket followed by a letter.
    [InlineData(" A")] // A leading space followed by a letter.
    [InlineData("A ")] // A letter followed by a trailing space.
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
