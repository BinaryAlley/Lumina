#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands;
using Lumina.Presentation.Api.Common.Contracts.Books;
using Mapster;
#endregion

namespace Lumina.Presentation.Api.Common.Mapping;

/// <summary>
/// Mapping configuration for books.
/// </summary>
public class BookMappingConfig : IRegister
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Registers the mapping configuration.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AddBookRequest, AddBookCommand>()
            .Map(dest => dest.ASIN, src => src.ASIN)
            .Map(dest => dest.ISBNs, src => src.ISBNs)
            .Map(dest => dest.LCCN, src => src.LCCN)
            .Map(dest => dest.OCLCNumber, src => src.OCLCNumber)
            .MapToConstructor(true);
        
    }
    #endregion
}