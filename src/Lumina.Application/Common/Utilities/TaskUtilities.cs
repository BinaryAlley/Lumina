#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Utilities;

/// <summary>
/// Task extension method to handle exceptions (avoid littering whole project with try...catch'es).
/// </summary>
public static class TaskUtilities
{
    /// <summary>
    /// Extends Task by providing exception handling when a task is invoked on a void method (not awaited).
    /// </summary>
    /// <param name="task">The task to be awaited.</param>
    /// <param name="onComplete">Optional continuation when <paramref name="task"/> finishes.</param>
    public static async void FireAndForgetSafeAsync(this Task task, Action? onComplete = null)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            onComplete?.Invoke();
        }
    }
}
