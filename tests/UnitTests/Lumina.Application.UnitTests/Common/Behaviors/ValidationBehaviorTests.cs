#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Lumina.Application.Common.Behaviors;
using Lumina.Application.UnitTests.Common.Behaviors.Fixtures;
using Mediator;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Contains unit tests for the <see cref="ValidationBehavior"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidationBehaviorTests
{
    private readonly IValidator<ValidationBehaviorFixture> _mockValidator;
    private readonly MessageHandlerDelegate<ValidationBehaviorFixture, ErrorOr<ValidationBehaviorTestResponse>> _nextDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehaviorTests"/> class.
    /// </summary>
    public ValidationBehaviorTests()
    {
        _mockValidator = Substitute.For<IValidator<ValidationBehaviorFixture>>();
        _nextDelegate = Substitute.For<MessageHandlerDelegate<ValidationBehaviorFixture, ErrorOr<ValidationBehaviorTestResponse>>>();
    }

    [Fact]
    public async Task Handle_WhenValidatorIsNull_ShouldInvokeNextDelegate()
    {
        // Arrange
        ValidationBehavior<ValidationBehaviorFixture, ErrorOr<ValidationBehaviorTestResponse>> behavior = new();
        ValidationBehaviorFixture request = new();
        ValidationBehaviorTestResponse expectedResponse = new();
        _nextDelegate.Invoke(request, Arg.Any<CancellationToken>()).Returns(new ValueTask<ErrorOr<ValidationBehaviorTestResponse>>(ErrorOrFactory.From(expectedResponse)));

        // Act
        ErrorOr<ValidationBehaviorTestResponse> result = await behavior.Handle(request, CancellationToken.None, _nextDelegate);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedResponse);
        await _nextDelegate.Received(1).Invoke(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValidationSucceeds_ShouldInvokeNextDelegate()
    {
        // Arrange
        ValidationBehavior<ValidationBehaviorFixture, ErrorOr<ValidationBehaviorTestResponse>> behavior = new(_mockValidator);
        ValidationBehaviorFixture request = new();
        ValidationBehaviorTestResponse expectedResponse = new();
        _mockValidator.ValidateAsync(request, Arg.Any<CancellationToken>()).Returns(new ValidationResult());
        _nextDelegate.Invoke(request, Arg.Any<CancellationToken>()).Returns(new ValueTask<ErrorOr<ValidationBehaviorTestResponse>>(ErrorOrFactory.From(expectedResponse)));

        // Act
        ErrorOr<ValidationBehaviorTestResponse> result = await behavior.Handle(request, CancellationToken.None, _nextDelegate);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedResponse);
        await _mockValidator.Received(1).ValidateAsync(request, Arg.Any<CancellationToken>());
        await _nextDelegate.Received(1).Invoke(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldReturnErrors()
    {
        // Arrange
        ValidationBehavior<ValidationBehaviorFixture, ErrorOr<ValidationBehaviorTestResponse>> behavior = new(_mockValidator);
        ValidationBehaviorFixture request = new();
        List<ValidationFailure> validationFailures =
        [
            new ValidationFailure("Property1", "Error message 1"),
            new ValidationFailure("Property2", "Error message 2")
        ];
        _mockValidator.ValidateAsync(request, Arg.Any<CancellationToken>()).Returns(new ValidationResult(validationFailures));

        // Act
        ErrorOr<ValidationBehaviorTestResponse> result = await behavior.Handle(request, CancellationToken.None, _nextDelegate);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.Errors[0].Description.Should().Be("Error message 1");
        result.Errors[1].Type.Should().Be(ErrorType.Validation);
        result.Errors[1].Description.Should().Be("Error message 2");
        await _mockValidator.Received(1).ValidateAsync(request, Arg.Any<CancellationToken>());
        await _nextDelegate.DidNotReceive().Invoke(request, Arg.Any<CancellationToken>());
    }
}
