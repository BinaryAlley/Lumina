#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion

namespace Lumina.Infrastructure.Common.Utilities;

/// <summary>
/// Exception extension methods.
/// </summary>
/// <remarks>
/// Based on https://stackoverflow.com/a/62283895.
/// </remarks>  
public static class ExceptionUtilities
{
    /// <summary>
    /// Concatenates all inner exception(s) messages of <paramref name="ex"/>.
    /// </summary>
    /// <param name="ex">The exception for which to get the inner exception(s) message.</param>
    /// <returns>A string representing the concatenated messages of all inner expcetion(s) of <paramref name="ex"/>.</returns>
    public static string AggregateMessages(this Exception ex)
    {
        return ex.GetInnerExceptions().Aggregate(new StringBuilder(), (stringBuilder, exception) => stringBuilder.AppendLine(exception.Message + " -> "), stringBuilder => stringBuilder.ToString());
    }

    /// <summary>
    /// Recursively gets the inner exception(s) of <paramref name="exception"/>, at <paramref name="maximumDepth"/> depth.
    /// </summary>
    /// <param name="exception">The exception for which to get the inner exception(s).</param>
    /// <param name="maximumDepth">The depth of the recursivity of getting the inner exception(s) of <paramref name="exception"/>.</param>
    /// <returns>A list of exceptions representing the list of inner exception(s) of <paramref name="exception"/>.</returns>
    public static IEnumerable<Exception> GetInnerExceptions(this Exception exception, int maximumDepth = 5)
    {
        // check if there are any more inner exceptions to return
        if (exception == null || maximumDepth <= 0)
            yield break;
        // yield the current level exception itself
        yield return exception;
        // if the exception is an AggregateException, treat it differently and get its all exceptions
        if (exception is AggregateException aggregateException)
            foreach (Exception? innerException in aggregateException.InnerExceptions.SelectMany(innerException => innerException.GetInnerExceptions(maximumDepth - 1)))
                yield return innerException;
        // if it's a normal exception, recursively get its list of inner exceptions and yield them
        foreach (Exception innerException in exception.InnerException?.GetInnerExceptions(maximumDepth - 1) ?? [])
            yield return innerException;
    }

    /// <summary>
    /// Concatenates the relevant properties of <paramref name="ex"/>.
    /// </summary>
    /// <param name="ex">The exception for which to get the relevant properties.</param>
    /// <returns>A string representing the concatenated relevant properties of <paramref name="ex"/>.</returns>
    public static string AggregateMessage(this Exception ex)
    {
        return $"Message: {ex.Message}; CallStack: {ex.StackTrace}";
    }
}
