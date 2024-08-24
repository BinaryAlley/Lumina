#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Application.Common.Enums;

/// <summary>
/// Represents the format of a book.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookFormat
{
    Hardcover,
    Paperback,
    MassMarketPaperback,
    TradePaperback,
    eBook,
    Audiobook,
    LargePrint,
    BoardBook,
    SpiralBound,    
    LibraryBinding,
    LeatherBound,
    PopupBook
}