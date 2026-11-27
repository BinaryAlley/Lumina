#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Infrastructure.Fixtures.Core.Authorization;

/// <summary>
/// Test policy implementation for registered policy scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public class TestAuthorizationPolicy : IAuthorizationPolicy
{
    public Task<bool> EvaluateAsync(Guid userId, PolicyContext? context, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
