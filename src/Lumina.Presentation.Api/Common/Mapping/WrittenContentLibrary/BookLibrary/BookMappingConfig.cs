#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Requests.WrittenContentLibrary.BookLibrary;
using Mapster;
#endregion

namespace Lumina.Presentation.Api.Common.Mapping.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Mapping configuration for books between the API request object and the command object.
/// </summary>
public class BookMappingConfig : IRegister
{
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
}