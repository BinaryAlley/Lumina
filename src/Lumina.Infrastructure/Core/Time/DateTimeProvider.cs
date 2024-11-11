#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Time;
using System;
#endregion

namespace Lumina.Infrastructure.Core.Time;

/// <summary>
/// Service for time related concerns
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// The current UTC time.
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}
