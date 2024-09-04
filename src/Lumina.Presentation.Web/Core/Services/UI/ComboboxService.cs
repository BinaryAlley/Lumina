namespace Lumina.Presentation.Web.Core.Services.UI;

/// <summary>
/// Service for managing comboboxes.
/// </summary>
public class ComboboxService
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public event Action<string>? ComboboxOpened;
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Notifies that a combobox has been opened.
    /// </summary>
    /// <param name="id">The id of the combobox that opened.</param>
    public void NotifyComboboxOpened(string id)
    {
        ComboboxOpened?.Invoke(id);
    }
    #endregion
}