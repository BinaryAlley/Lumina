#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authorization.Fixtures;

/// <summary>
/// Example concrete policy for testing generic constraint.
/// </summary>
public interface ITestAuthorizationPolicy : IAuthorizationPolicy { }
