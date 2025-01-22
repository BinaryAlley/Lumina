#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.UoW;

/// <summary>
/// Interaction boundary with the Data Access Layer.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LuminaDbContext _luminaDbContext;
    private readonly IRepositoryFactory _repositoryFactory;

    /// <summary>
    /// Gets or sets the collection of available repositories.
    /// </summary>
    internal RepositoryDictionary Repositories { get; private set; } = new RepositoryDictionary();

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="repositoryFactory">The factory used to generate repositories.</param>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public UnitOfWork(IRepositoryFactory repositoryFactory, LuminaDbContext luminaDbContext)
    {
        _repositoryFactory = repositoryFactory;
        _luminaDbContext = luminaDbContext;
        AddRepositories();
    }

    /// <summary>
    /// Adds all the repositories from the Data Access Layer so that they can be exposed to the Business Layer.
    /// </summary>
    internal void AddRepositories()
    {
        // get all the concrete implementations of IRepository<>
        IEnumerable<Type> repositoryClassTypes =
            Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => !t.IsInterface && !t.IsAbstract && t.GetInterfaces()
                                                                    .Any(i => i.IsGenericType &&
                                                                              i.GetGenericTypeDefinition() == typeof(IRepository<>)));
        // store all repositories
        foreach (Type repositoryClassType in repositoryClassTypes)
        {
            // get the interface that implements IRepository<> of the currently iterated repository class
            Type repositoryInterfaceType = repositoryClassType.GetInterfaces()
                                                              .Where(i => !i.IsGenericType &&
                                                                           i.GetInterfaces()
                                                                            .Any(a => a.GetGenericTypeDefinition() == typeof(IRepository<>)))
                                                              .First();
            // ask the concrete type for the repository interface type from the repositories factory;
            // because the method for creating a repository is generic and we need to call it with a runtime type, reflection is the only option
            object repositoryClass = typeof(IRepositoryFactory).GetMethod("CreateRepository")!
                                                               .MakeGenericMethod(repositoryInterfaceType)
                                                               .Invoke(_repositoryFactory, null)!;
            Repositories.Add(repositoryClass);
        }
    }

    /// <summary>
    /// Exposes a repository of type <typeparamref name="TRepository"/> to the Business Logic Layer.
    /// </summary>
    /// <typeparam name="TRepository">The type of the exposed repository.</typeparam>
    /// <returns>A repository of type <typeparamref name="TRepository"/>.</returns>
    public TRepository GetRepository<TRepository>()
    {
        // get the repository type based on the type of the provided repository interface
        Type repositoryType = Assembly.GetExecutingAssembly()
                                      .GetTypes()
                                      .Where(type => !type.IsInterface && !type.IsAbstract && typeof(TRepository).IsAssignableFrom(type))
                                      .First();
        return Repositories.Get<TRepository>(repositoryType);
    }

    /// <summary>
    /// Resets the list of repositories.
    /// </summary>
    internal void ResetRepositories()
    {
        Repositories.Clear();
    }

    /// <summary>
    /// Saves the changes made to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _luminaDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a transaction.
    /// </summary>
    public void OpenTransaction()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Commits a transaction.
    /// </summary>
    public void CommitTransaction()
    {
        throw new NotImplementedException();
    }
}
