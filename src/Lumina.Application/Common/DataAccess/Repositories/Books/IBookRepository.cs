#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;

#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Books;

/// <summary>
/// Interface for the repository for books.
/// </summary>
public interface IBookRepository : IRepository<BookEntity>,
                                   IInsertRepositoryAction<BookEntity>,
                                   IGetAllRepositoryAction<BookEntity>
{
}