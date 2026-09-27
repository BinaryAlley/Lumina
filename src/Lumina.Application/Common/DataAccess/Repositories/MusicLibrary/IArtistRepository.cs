#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;

/// <summary>
/// Interface for the repository for artists.
/// </summary>
public interface IArtistRepository : IRepository<ArtistEntity>,
                                     IInsertRepositoryAction<ArtistEntity>,
                                     IUpdateRepositoryAction<ArtistEntity>,
                                     IGetByIdRepositoryAction<ArtistEntity, Guid>,
                                     IGetAllRepositoryAction<ArtistEntity>,
                                     IGetAllLiteRepositoryAction<ArtistEntity, ArtistLiteRow>,
                                     IDeleteByIdRepositoryAction<Guid>
{
}
