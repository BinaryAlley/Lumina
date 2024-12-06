#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authorization.Policies.Common.Factory.Fixtures;

/// <summary>
/// Test policy interface for unregistered policy scenarios.
/// </summary>
public interface IUnregisteredPolicy : IAuthorizationPolicy { }
