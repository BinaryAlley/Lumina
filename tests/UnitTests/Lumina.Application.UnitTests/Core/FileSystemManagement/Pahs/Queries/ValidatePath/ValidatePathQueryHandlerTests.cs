#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.ValidatePath.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.SharedKernel.Common.Errors;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.ValidatePath;

/// <summary>
/// Contains unit tests for the <see cref="ValidatePathQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathQueryHandlerTests
{
    private readonly IPathService _mockPathService;
    private readonly IValidator<ValidatePathQuery> _mockValidator;
    private readonly ValidatePathQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathQueryHandlerTests"/> class.
    /// </summary>
    public ValidatePathQueryHandlerTests()
    {
        _mockPathService = Substitute.For<IPathService>();
        _mockValidator = Substitute.For<IValidator<ValidatePathQuery>>();
        _mockValidator.Validate(Arg.Any<ValidatePathQuery>())
            .Returns([]);
        _sut = new ValidatePathQueryHandler(_mockPathService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidPath_ShouldReturnTrueResponse()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        _mockPathService.IsValidPath(query.Path!).Returns(true);

        // Act
        ErrorOr<PathValidResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value.IsValid);
        _mockPathService.Received(1).IsValidPath(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithInvalidPath_ShouldReturnFalseResponse()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        _mockPathService.IsValidPath(query.Path!).Returns(false);

        // Act
        ErrorOr<PathValidResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.False(result.Value.IsValid);
        _mockPathService.Received(1).IsValidPath(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithNullPath_ShouldReturnFalseResponse()
    {
        // Arrange
        ValidatePathQuery query = new(null!);
        _mockPathService.IsValidPath(Arg.Any<string>()).Returns(false);

        // Act
        ErrorOr<PathValidResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.False(result.Value.IsValid);
        _mockPathService.Received(1).IsValidPath(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithEmptyPath_ShouldReturnFalseResponse()
    {
        // Arrange
        ValidatePathQuery query = new(string.Empty);
        _mockPathService.IsValidPath(Arg.Any<string>()).Returns(false);

        // Act
        ErrorOr<PathValidResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.False(result.Value.IsValid);
        _mockPathService.Received(1).IsValidPath(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsErrors_ShouldReturnValidationErrorsWithoutCallingPathService()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        _mockValidator.Validate(query)
            .Returns([Errors.FileSystemManagement.PathCannotBeEmpty]);

        // Act
        ErrorOr<PathValidResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.PathCannotBeEmpty, result.FirstError);
        _mockPathService.DidNotReceive().IsValidPath(Arg.Any<string>());
    }
}
