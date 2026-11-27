#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
#endregion

namespace Nemera.Infrastructure.Fixtures.Core.Authorization;

/// <summary>
/// Example concrete policy for testing generic constraint.
/// </summary>
public interface ITestAuthorizationPolicy : IAuthorizationPolicy { }
