﻿#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Common.Factory;

/// <summary>
/// Concrete implementation for the repositories factory.
/// </summary>
public class RepositoryFactory : IRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public RepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a new repository of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of repository to create.</typeparam>
    /// <returns>A repository of type <typeparamref name="TResult"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested repository has not been registered.</exception>
    public TResult CreateRepository<TResult>() where TResult : notnull
    {
        return _serviceProvider.GetRequiredService<TResult>() ?? throw new ArgumentException();
    }
}