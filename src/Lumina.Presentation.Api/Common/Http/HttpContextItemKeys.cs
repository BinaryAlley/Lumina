#region ========================================================================= USING =====================================================================================
namespace Lumina.Presentation.Api.Common.Http;
#endregion

/// <summary>
/// Constants shared by HttpContext across a HTTP request (avoid magic strings).
/// </summary>
public static class HttpContextItemKeys
{
    public const string ERRORS = "errors";
}