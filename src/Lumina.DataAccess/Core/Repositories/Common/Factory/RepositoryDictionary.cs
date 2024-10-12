#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Common.Factory;

/// <summary>
/// Custom collection for storing repositories.
/// </summary>
public class RepositoryDictionary
{
    private readonly List<KeyValuePair<Type, object>> _container = [];

    /// <summary>
    /// Gets the number of added repositories.
    /// </summary>
    public int Count => _container.Count;

    /// <summary>
    /// Resets the collection of repositories.
    /// </summary>
    public void Clear()
    {
        _container.Clear();
    }

    /// <summary>
    /// Adds a repository of type <typeparamref name="TParam"/> to the collection.
    /// </summary>
    /// <typeparam name="TParam">The type of the repository to add to the collection.</typeparam>
    /// <param name="value">The repository to add.</param>
    public void Add<TParam>(TParam value) where TParam : notnull
    {
        // enforce adding rules
        if (value == null)
            throw new ArgumentException("Value cannot be null!");
        if (_container.Any(e => e.Key == value.GetType()))
            throw new ArgumentException("Duplicate values are not allowed!");
        // check if the value implements an interface that implements IRepository
        bool implementsRepository = value.GetType()
                                         .GetInterfaces()
                                         .Any(e => e.GetInterfaces()
                                                    .Any(i => i.IsGenericType &&
                                                              i.GetGenericTypeDefinition() == typeof(IRepository<>)));
        if (implementsRepository)
            _container.Add(new KeyValuePair<Type, object>(value.GetType(), value));
        else
            throw new ArgumentException("Value must implement IRepository interface!");
    }

    /// <summary>
    /// Gets a repository of type <typeparamref name="TResult"/> from the collection.
    /// </summary>
    /// <typeparam name="TResult">The type of the repository interface to get from the collection.</typeparam>
    /// <param name="key">The concrete type of the repository interface to get.</param>
    /// <returns>A repository implementing <typeparamref name="TResult"/>.</returns>
    public TResult Get<TResult>(Type key)
    {
        return (TResult)_container.FirstOrDefault(e => e.Key == key).Value;
    }
}