#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryScanProgressQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryScanProgressQueryValidatorTests
{
    private readonly GetLibraryScanProgressQueryValidator _validator = new();
    private readonly GetLibraryScanProgressQueryFixture _getLibraryScanProgressQueryFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();
        query = query with { LibraryId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenScanIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();
        query = query with { ScanId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.LibraryScanning.LibraryScanIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenScanIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.LibraryScanning.LibraryScanIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetLibraryScanProgressQuery query = _getLibraryScanProgressQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
