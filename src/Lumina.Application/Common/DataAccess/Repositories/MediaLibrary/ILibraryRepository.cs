#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for media libraries.
/// </summary>
public interface ILibraryRepository : IRepository<LibraryEntity>,
                                      IGetByIdRepositoryAction<LibraryEntity, Guid>,
                                      IGetAllRepositoryAction<LibraryEntity>,
                                      IInsertRepositoryAction<LibraryEntity>,
                                      IUpdateRepositoryAction<LibraryEntity>,
                                      IDeleteByIdRepositoryAction<Guid>
{
}
