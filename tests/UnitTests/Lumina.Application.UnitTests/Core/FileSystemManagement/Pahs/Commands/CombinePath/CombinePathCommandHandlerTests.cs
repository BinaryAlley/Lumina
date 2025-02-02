#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Commands.CombinePath.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Commands.CombinePath;

/// <summary>
/// Contains unit tests for the <see cref="CombinePathCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IPathService _mockPathService;
    private readonly CombinePathCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathCommandHandlerTests"/> class.
    /// </summary>
    public CombinePathCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPathService = Substitute.For<IPathService>();
        _sut = new CombinePathCommandHandler(_mockPathService);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        CombinePathCommand combinePathCommand = CombinePathCommandFixture.CreateCombinePathCommand();
        string combinedPath = System.IO.Path.Combine(combinePathCommand.OriginalPath!, combinePathCommand.NewPath!);

        _mockPathService.CombinePath(combinePathCommand.OriginalPath!, combinePathCommand.NewPath!)
            .Returns(ErrorOrFactory.From(combinedPath));

        // Act
        ErrorOr<PathSegmentResponse> result = await _sut.Handle(combinePathCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.IsType<PathSegmentResponse>(result.Value);
        Assert.Equal(combinedPath, result.Value.Path);
        _mockPathService.Received(1).CombinePath(combinePathCommand.OriginalPath!, combinePathCommand.NewPath!);
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        CombinePathCommand command = _fixture.Create<CombinePathCommand>();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.CombinePath(command.OriginalPath!, command.NewPath!)
            .Returns(error);

        // Act
        ErrorOr<PathSegmentResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
        _mockPathService.Received(1).CombinePath(command.OriginalPath!, command.NewPath!);
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsEmptyString_ShouldReturnSuccessResultWithEmptyPath()
    {
        // Arrange
        CombinePathCommand command = _fixture.Create<CombinePathCommand>();
        _mockPathService.CombinePath(command.OriginalPath!, command.NewPath!)
            .Returns(ErrorOrFactory.From(string.Empty));

        // Act
        ErrorOr<PathSegmentResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.IsType<PathSegmentResponse>(result.Value);
        Assert.Empty(result.Value.Path);
        _mockPathService.Received(1).CombinePath(command.OriginalPath!, command.NewPath!);
    }
}
