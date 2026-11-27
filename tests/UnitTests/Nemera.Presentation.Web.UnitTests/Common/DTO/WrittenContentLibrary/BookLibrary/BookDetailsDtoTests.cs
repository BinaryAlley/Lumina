#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Web.Common.Enums.MediaContributors;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Nemera.Presentation.Web.UnitTests.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="BookDetailsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookDetailsDtoTests
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void Deserialize_WhenBookResponseHasContributorReferences_ShouldBindContributorIdAndRole()
    {
        // Arrange
        Guid firstContributorId = Guid.NewGuid();
        Guid secondContributorId = Guid.NewGuid();
        string json = $$"""
        {
            "id": "{{Guid.NewGuid()}}",
            "libraryId": "{{Guid.NewGuid()}}",
            "path": "C:\\books\\book.epub",
            "contributors": [
                { "contributorId": "{{firstContributorId}}", "role": "Author" },
                { "contributorId": "{{secondContributorId}}", "role": "Illustrator" }
            ]
        }
        """;

        // Act
        BookDetailsDto? result = JsonSerializer.Deserialize<BookDetailsDto>(json, s_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Contributors);
        Assert.Equal(2, result.Contributors.Count);
        Assert.Equal(firstContributorId, result.Contributors[0].ContributorId);
        Assert.Equal(MediaContributorRole.Author, result.Contributors[0].Role);
        Assert.Equal(secondContributorId, result.Contributors[1].ContributorId);
        Assert.Equal(MediaContributorRole.Illustrator, result.Contributors[1].Role);
    }
}
