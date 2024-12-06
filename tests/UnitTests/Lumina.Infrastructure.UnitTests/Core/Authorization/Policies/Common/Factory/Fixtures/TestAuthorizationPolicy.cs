#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authorization.Policies.Common.Factory.Fixtures;

/// <summary>
/// Test policy implementation for registered policy scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public class TestAuthorizationPolicy : IAuthorizationPolicy
{
    public Task<bool> EvaluateAsync(Guid userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
