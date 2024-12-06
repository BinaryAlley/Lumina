#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Over18;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Infrastructure.Core.Authentication;
using Lumina.Infrastructure.Core.Authorization;
using Lumina.Infrastructure.Core.Authorization.Policies.Common.Factory;
using Lumina.Infrastructure.Core.Authorization.Policies.Over18;
using Lumina.Infrastructure.Core.Security;
using Lumina.Infrastructure.Core.Time;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
#endregion

namespace Lumina.Infrastructure.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Infrastructure layer.
/// </summary>
[ExcludeFromCodeCoverage]
public static class InfrastructureLayerServices
{
    /// <summary>
    /// Extension method for adding the Infrastructure layer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services)
    {
        // scan the current assembly for validators and add them to the DI container
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

        services.AddSingleton<IHashService, HashService>();
        services.AddSingleton<ICryptographyService, CryptographyService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IQRCodeGenerator, QRCodeGenerator>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<ITotpTokenGenerator, TotpTokenGenerator>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // authorization
        services.AddScoped<IOver18Policy, Over18Policy>();
        services.AddScoped<IAuthorizationPolicyFactory, AuthorizationPolicyFactory>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        return services;
    }
}
