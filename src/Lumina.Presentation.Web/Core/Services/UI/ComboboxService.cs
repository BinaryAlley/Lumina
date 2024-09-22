#region ========================================================================= USING =====================================================================================
using System;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Services.UI;

/// <summary>
/// Service for managing comboboxes.
/// </summary>
public class ComboboxService
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public event Func<string, Task>? ComboboxOpened;
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