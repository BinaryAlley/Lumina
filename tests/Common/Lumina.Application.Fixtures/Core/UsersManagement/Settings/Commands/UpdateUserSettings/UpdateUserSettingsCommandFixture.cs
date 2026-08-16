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
    /// <param name="ignoreThePrefixForAlphaPicker">Optional. Whether the "The" prefix is ignored by the alpha picker, or not.</param>
    /// <returns>The created command.</returns>
    public UpdateUserSettingsCommand Create(
        bool? isPaginationEnabled = null,
        int? itemsPerPage = null,
        bool? ignoreThePrefixForAlphaPicker = null)
    {
        return new Faker<UpdateUserSettingsCommand>()
            .CustomInstantiator(f => new UpdateUserSettingsCommand(default, default, default))
            .RuleFor(x => x.IsPaginationEnabled, f => isPaginationEnabled ?? f.Random.Bool())
            .RuleFor(x => x.ItemsPerPage, f => itemsPerPage ?? f.Random.Int(1, 100))
            .RuleFor(x => x.IgnoreThePrefixForAlphaPicker, f => ignoreThePrefixForAlphaPicker ?? f.Random.Bool())
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
