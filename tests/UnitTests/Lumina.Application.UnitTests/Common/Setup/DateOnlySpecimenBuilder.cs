﻿#region ========================================================================= USING =====================================================================================
using AutoFixture.Kernel;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Setup;

/// <summary>
/// Represents a specimen builder for the <see cref="DateOnly"/> type.
/// </summary>
[ExcludeFromCodeCoverage]
public class DateOnlySpecimenBuilder : ISpecimenBuilder
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Adds <see cref="DateOnly"/> support to the AutoFixture library.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="context">The context within which the specimen is created.</param>
    /// <returns>A specimen based on the request.</returns>
    public object Create(object request, ISpecimenContext context)
    {
        return request is Type type && type == typeof(DateOnly) ? DateOnly.FromDateTime(DateTime.Now) : (object)new NoSpecimen();
    }
    #endregion
}