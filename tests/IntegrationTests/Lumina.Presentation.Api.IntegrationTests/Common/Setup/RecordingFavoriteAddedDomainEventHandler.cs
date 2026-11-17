#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Domain event handler that records the publication of a <see cref="FavoriteAddedDomainEvent"/>, together with the state visible at publication time.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class RecordingFavoriteAddedDomainEventHandler : IDomainEventHandler<FavoriteAddedDomainEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DomainEventPublicationProbe _publicationProbe;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordingFavoriteAddedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="scopeFactory">Injected factory used to create a database scope independent from the request scope.</param>
    /// <param name="publicationProbe">Injected probe that records the published event.</param>
    public RecordingFavoriteAddedDomainEventHandler(IServiceScopeFactory scopeFactory, DomainEventPublicationProbe publicationProbe)
    {
        _scopeFactory = scopeFactory;
        _publicationProbe = publicationProbe;
    }

    /// <summary>
    /// Records the published <paramref name="domainEvent"/>, checking whether its favorite was already committed.
    /// </summary>
    /// <param name="domainEvent">The domain event being published.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async ValueTask HandleAsync(FavoriteAddedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // A separate scope and context prove that the row is visible to other connections, which is only true after the request transaction was committed.
        using IServiceScope scope = _scopeFactory.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        bool wasFavoriteVisible = await dbContext.Favorites
            .AsNoTracking()
            .AnyAsync(favorite => favorite.Id == domainEvent.FavoriteId.Value, cancellationToken).ConfigureAwait(false);

        _publicationProbe.RecordPublication(wasFavoriteVisible);
    }
}
