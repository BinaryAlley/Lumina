#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.TestHelpers;

/// <summary>
/// Helper for executing <see cref="IResult"/> instances and reading the JSON response body they produce.
/// </summary>
[ExcludeFromCodeCoverage]
public static class JsonResultTestHelper
{
    /// <summary>
    /// Executes the specified <paramref name="result"/> against a fresh HTTP context and reads the response body.
    /// </summary>
    /// <param name="result">The result to execute.</param>
    /// <param name="httpContext">Optional HTTP context to execute the result against; a fresh context is created when not provided.</param>
    /// <returns>The response body produced by the result.</returns>
    public static async Task<string> GetResponseBodyAsync(IResult result, HttpContext? httpContext = null)
    {
        DefaultHttpContext context = httpContext as DefaultHttpContext ?? TestHttpContextFactory.Create();
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        return await new StreamReader(context.Response.Body).ReadToEndAsync();
    }
}
