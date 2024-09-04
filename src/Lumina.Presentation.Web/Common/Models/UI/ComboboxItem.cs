namespace Lumina.Presentation.Web.Common.Models.UI;

/// <summary>
/// Represents an item in a combobox.
/// </summary>
public class ComboboxItem
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the text of the item, that is visible to the user.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the value of the item, that is used by the application.
    /// </summary>
    public string? Value { get; set; }
    #endregion
}