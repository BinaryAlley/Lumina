#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;
using Nemera.Contracts.Requests.MediaLibrary.Management;
#endregion

namespace Nemera.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="GetLibraryPathTemplatePartsRequest"/>.
/// </summary>
public static class GetLibraryPathTemplatePartsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetLibraryPathTemplatePartsQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetLibraryPathTemplatePartsQuery ToQuery(this GetLibraryPathTemplatePartsRequest request)
    {
        return new GetLibraryPathTemplatePartsQuery(
            request.LibraryType
        );
    }
}
