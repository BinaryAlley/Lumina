#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Paths.Queries.GetPathSeparator;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.GetPathSeparator;

/// <summary>
/// Contains unit tests for the <see cref="GetPathSeparatorQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathSeparatorQueryHandlerTests
{
    private readonly IPathService _mockPathService;
    private readonly GetPathSeparatorQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorQueryHandlerTests"/> class.
    /// </summary>
    public GetPathSeparatorQueryHandlerTests()
    {
        _mockPathService = Substitute.For<IPathService>();
        _sut = new GetPathSeparatorQueryHandler(_mockPathService);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnPathSeparatorResponse()
    {
        // Arrange
        GetPathSeparatorQuery query = new();
        char expectedSeparator = System.IO.Path.DirectorySeparatorChar;
        _mockPathService.PathSeparator.Returns(expectedSeparator);

        // Act
        PathSeparatorResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Separator.Should().Be(expectedSeparator.ToString());

        _mockPathService.Received(1);
    }

    [Theory]
    [InlineData('/')]
    [InlineData('\\')]
    public async Task Handle_WithDifferentSeparators_ShouldReturnCorrectSeparator(char separator)
    {
        // Arrange
        GetPathSeparatorQuery query = new();
        _mockPathService.PathSeparator.Returns(separator);

        // Act
        PathSeparatorResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Separator.Should().Be(separator.ToString());
        _mockPathService.Received(1);
    }

    [Fact]
    public async Task Handle_WhenCalledMultipleTimes_ShouldReturnSameResult()
    {
        // Arrange
        GetPathSeparatorQuery query = new();
        char expectedSeparator = System.IO.Path.DirectorySeparatorChar;
        _mockPathService.PathSeparator.Returns(expectedSeparator);

        // Act
        PathSeparatorResponse result1 = await _sut.Handle(query, CancellationToken.None);
        PathSeparatorResponse result2 = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result1.Should().BeEquivalentTo(result2);
        _mockPathService.Received(2);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        GetPathSeparatorQuery query = new();
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () => await _sut.Handle(query, cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
