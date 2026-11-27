#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Application.Core.Favorites.Commands.AddFavorite;
using Lumina.Contracts.Fixtures.Core.Requests.Favorites;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="AddFavoriteRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteRequestMappingTests
{
    private readonly AddFavoriteRequestFixture _addFavoriteRequestFixture = new();

    [Fact]
    public void ToCommand_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(
            mediaKind: FavoriteMediaKind.Book,
            mediaItemId: mediaItemId);

        // Act
        AddFavoriteCommand command = request.ToCommand();

        // Assert
        Assert.Equal(FavoriteMediaKind.Book, command.MediaKind);
        Assert.Equal(mediaItemId, command.MediaItemId);
    }

    [Fact]
    public void ToCommand_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(includeMediaKind: false);

        // Act
        AddFavoriteCommand command = request.ToCommand();

        // Assert
        Assert.Null(command.MediaKind);
        Assert.NotNull(command.MediaItemId);
    }

    [Fact]
    public void ToCommand_WhenMediaItemIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(includeMediaItemId: false);

        // Act
        AddFavoriteCommand command = request.ToCommand();

        // Assert
        Assert.NotNull(command.MediaKind);
        Assert.Null(command.MediaItemId);
    }
}
