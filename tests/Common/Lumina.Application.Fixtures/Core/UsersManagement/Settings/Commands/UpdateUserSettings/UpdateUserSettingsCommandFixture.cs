#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Fixture class for the <see cref="UpdateUserSettingsCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsCommandFixture
{
    /// <summary>
    /// Creates a random valid command to update the settings of the current user.
    /// </summary>
    /// <param name="isPaginationEnabled">Optional. Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">Optional. The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional. Whether the "The" prefix is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Optional. Whether the theme data served to this user is cached, or not.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Optional. Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing.</param>
    /// <param name="shouldRenderPdfAsImages">Optional. Whether PDF books are rendered as page images for the user.</param>
    /// <param name="shouldPreserveBookStyles">Optional. Whether the styles of the book content are preserved when it is rendered for the user.</param>
    /// <returns>The created command.</returns>
    public UpdateUserSettingsCommand Create(
        bool? isPaginationEnabled = null,
        int? itemsPerPage = null,
        bool? shouldIgnoreThePrefixForAlphaPicker = null,
        bool? isThemeCachingEnabled = null,
        bool? shouldAggregateMetadataWhenMissing = null,
        bool? shouldRenderPdfAsImages = null,
        bool? shouldPreserveBookStyles = null)
    {
        return new Faker<UpdateUserSettingsCommand>()
            .CustomInstantiator(f => new UpdateUserSettingsCommand(default, default, default, default, default, default, default))
            .RuleFor(x => x.IsPaginationEnabled, f => isPaginationEnabled ?? f.Random.Bool())
            .RuleFor(x => x.ItemsPerPage, f => itemsPerPage ?? f.Random.Int(1, 100))
            .RuleFor(x => x.ShouldIgnoreThePrefixForAlphaPicker, f => shouldIgnoreThePrefixForAlphaPicker ?? f.Random.Bool())
            .RuleFor(x => x.IsThemeCachingEnabled, f => isThemeCachingEnabled ?? f.Random.Bool())
            .RuleFor(x => x.ShouldAggregateMetadataWhenMissing, f => shouldAggregateMetadataWhenMissing ?? f.Random.Bool())
            .RuleFor(x => x.ShouldRenderPdfAsImages, f => shouldRenderPdfAsImages ?? f.Random.Bool())
            .RuleFor(x => x.ShouldPreserveBookStyles, f => shouldPreserveBookStyles ?? f.Random.Bool())
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateUserSettingsCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateUserSettingsCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
