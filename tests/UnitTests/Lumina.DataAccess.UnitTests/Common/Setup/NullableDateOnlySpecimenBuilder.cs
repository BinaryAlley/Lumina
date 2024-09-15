#region ========================================================================= USING =====================================================================================
using AutoFixture.Kernel;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Setup;

/// <summary>
/// Represents a specimen builder for the <see cref="DateOnly"/> type.
/// </summary>
[ExcludeFromCodeCoverage]
public class NullableDateOnlySpecimenBuilder : ISpecimenBuilder
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Adds <see cref="DateOnly?"/> support to the AutoFixture library.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="context">The context within which the specimen is created.</param>
    /// <returns>A specimen based on the request.</returns>
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(DateOnly?))
            return (DateOnly?)DateOnly.FromDateTime(DateTime.Now);
        return new NoSpecimen();
    }
    #endregion
}