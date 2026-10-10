#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;

/// <summary>
/// Handler for the query to get the path template catalog available for a media library type.
/// </summary>
public class GetLibraryPathTemplatePartsQueryHandler : IQueryHandler<GetLibraryPathTemplatePartsQuery, Result<LibraryPathTemplateCatalogResponse>>
{
    private readonly IReadOnlyList<ILibraryPathPartCatalog> _catalogs;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryPathTemplatePartsQueryHandler"/> class.
    /// </summary>
    /// <param name="catalogs">Injected path part catalogs of the supported media library types.</param>
    public GetLibraryPathTemplatePartsQueryHandler(IEnumerable<ILibraryPathPartCatalog> catalogs)
    {
        _catalogs = [.. catalogs];
    }

    /// <summary>
    /// Handles the query to get the path template catalog available for a media library type.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the path template catalog, or an error message.
    /// </returns>
    public Task<Result<LibraryPathTemplateCatalogResponse>> HandleAsync(GetLibraryPathTemplatePartsQuery query, CancellationToken cancellationToken)
    {
        // the library type travels as a string taken from the route, and is matched against the enum the same way the create and update
        // library commands do, so that a malformed type is reported as an unknown library type instead of silently returning an empty catalog.
        if (!Enum.TryParse(query.LibraryType, ignoreCase: true, out LibraryType libraryType))
            return Task.FromResult<Result<LibraryPathTemplateCatalogResponse>>(DomainErrors.Library.UnknownLibraryType);

        // only the catalog registered for the requested library type applies, so the parts and the default structure offered to the user are
        // type aware (music libraries get artist and release parts, book libraries get author and title parts, and so on).
        ILibraryPathPartCatalog? catalog = _catalogs.FirstOrDefault(candidate => candidate.SupportedLibraryType == libraryType);
        if (catalog is null)
            // a library type without a catalog has no structure to describe, so an empty catalog is returned, which the client uses to hide the editor.
            return Task.FromResult(Result<LibraryPathTemplateCatalogResponse>.Success(new LibraryPathTemplateCatalogResponse([], [])));

        // the selectable parts describe what the user can drop into the template, together with the value type and the default on-disk representation of each part.
        List<LibraryPathPartDefinitionResponse> parts =
        [
            .. catalog.GetPartDefinitions().Select(definition => new LibraryPathPartDefinitionResponse(
                definition.Kind.ToString(),
                definition.ValueType.ToString(),
                definition.DefaultRepresentation,
                definition.IsOptionalByDefault))
        ];
        // the default template parts describe the ideal structure the application expects, pre-filled in the editor so the user only edits what deviates.
        List<LibraryPathTemplatePartResponse> defaultTemplateParts =
        [
            .. catalog.GetDefaultTemplate().Parts.Select(part => new LibraryPathTemplatePartResponse(part.Kind.ToString(), part.Representation, part.IsOptional))
        ];
        return Task.FromResult(Result<LibraryPathTemplateCatalogResponse>.Success(new LibraryPathTemplateCatalogResponse(parts, defaultTemplateParts)));
    }
}
