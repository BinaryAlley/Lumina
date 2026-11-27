#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Nemera.Application.Common.Infrastructure.Time;

/// <summary>
/// Interface for time related concerns.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// The current UTC time.
    /// </summary>
    DateTime UtcNow { get; }
}
