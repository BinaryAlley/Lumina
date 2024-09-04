namespace Lumina.Presentation.Web.Common.Models.UI;

/// <summary>
/// Represents an item in a combobox.
/// </summary>
/// <typeparam name="TValue">The type used for the <see cref="Value"/> property of the item.</typeparam>
public class ComboboxItem<TValue>
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the text of the item, that is visible to the user.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the value of the item, that is used by the application.
    /// </summary>
    public TValue? Value { get; set; }
    #endregion
}