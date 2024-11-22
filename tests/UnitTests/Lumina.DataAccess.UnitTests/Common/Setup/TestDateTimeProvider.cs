#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Time;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Setup;

/// <summary>
/// Service for time related concerns
/// </summary>
[ExcludeFromCodeCoverage]
public class TestDateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// The current UTC time.
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}
