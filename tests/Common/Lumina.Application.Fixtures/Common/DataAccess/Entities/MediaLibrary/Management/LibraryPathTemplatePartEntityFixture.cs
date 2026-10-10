#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryPathTemplatePartEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplatePartEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryPathTemplatePartEntity"/>.
    /// </summary>
    /// <param name="position">Optional. The zero based position of the part in the path template.</param>
    /// <param name="kind">Optional. The kind of the path part.</param>
    /// <param name="representation">Optional. The literal text or the value mask of the path part.</param>
    /// <param name="isOptional">Whether the path part can be absent from the path.</param>
    /// <returns>The created <see cref="LibraryPathTemplatePartEntity"/>.</returns>
    public LibraryPathTemplatePartEntity Create(
        int? position = null,
        LibraryPathPartKind? kind = null,
        string? representation = null,
        bool isOptional = false)
    {
        return new Faker<LibraryPathTemplatePartEntity>()
            .CustomInstantiator(f => new LibraryPathTemplatePartEntity
            {
                Position = position ?? 0,
                Kind = default,
                Representation = default!,
                IsOptional = isOptional
            })
            .RuleFor(part => part.Kind, f => kind ?? f.PickRandom<LibraryPathPartKind>())
            .RuleFor(part => part.Representation, f => representation ?? "{0}")
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryPathTemplatePartEntity"/> instances with sequential positions.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryPathTemplatePartEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(index => Create(position: index))];
    }
}
