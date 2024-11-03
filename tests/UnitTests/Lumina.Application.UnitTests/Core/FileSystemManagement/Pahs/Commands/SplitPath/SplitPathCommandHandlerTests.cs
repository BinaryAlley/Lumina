#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Commands.SplitPath.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Commands.SplitPath;

/// <summary>
/// Contains unit tests for the <see cref="SplitPathCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IPathService _mockPathService;
    private readonly SplitPathCommandHandler _sut;
    private readonly PathSegmentFixture _pathSegmentFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandHandlerTests"/> class.
    /// </summary>
    public SplitPathCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPathService = Substitute.For<IPathService>();
        _sut = new SplitPathCommandHandler(_mockPathService);
        _pathSegmentFixture = new PathSegmentFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        SplitPathCommand splitPathCommand = SplitPathCommandFixture.CreateSplitPathCommand();

        IEnumerable<PathSegment> pathSegments = _pathSegmentFixture.CreateMany();

        _mockPathService.ParsePath(splitPathCommand.Path!)
            .Returns(ErrorOrFactory.From(pathSegments));

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.Handle(splitPathCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<PathSegmentResponse>>();
        result.Value.Should().HaveCount(pathSegments.Count());

        List<PathSegmentResponse> resultList = result.Value.ToList();
        List<PathSegment> segmentsList = pathSegments.ToList();

        for (int i = 0; i < resultList.Count; i++)
            resultList[i].Path.Should().Be(segmentsList[i].Name);
        _mockPathService.Received(1).ParsePath(splitPathCommand.Path!);
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        SplitPathCommand command = _fixture.Create<SplitPathCommand>();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.ParsePath(command.Path!)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockPathService.Received(1).ParsePath(command.Path!);
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        SplitPathCommand command = _fixture.Create<SplitPathCommand>();
        ErrorOr<IEnumerable<PathSegment>> emptyList = ErrorOrFactory.From(Enumerable.Empty<PathSegment>());
        _mockPathService.ParsePath(command.Path!)
            .Returns(emptyList);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockPathService.Received(1).ParsePath(command.Path!);
    }
}
