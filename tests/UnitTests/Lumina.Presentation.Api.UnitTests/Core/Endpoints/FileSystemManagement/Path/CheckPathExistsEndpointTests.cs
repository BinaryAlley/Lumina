#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.CheckPathExists;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="CheckPathExistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsEndpointTests
{
    private readonly ISender _mockSender;
    private readonly CheckPathExistsEndpoint _sut;
    private readonly CheckPathExistsRequestFixture _checkPathExistsRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsEndpointTests"/> class.
    /// </summary>
    public CheckPathExistsEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<CheckPathExistsEndpoint>(_mockSender);
        _checkPathExistsRequestFixture = new CheckPathExistsRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithPathExistsResponse()
    {
        // Arrange
        CheckPathExistsRequest request = _checkPathExistsRequestFixture.Create(path: @"C:\Users\TestUser\Documents", includeHiddenElements: true);
        CancellationToken cancellationToken = CancellationToken.None;
        PathExistsResponse expectedResponse = new(true);
        _mockSender.Send(Arg.Any<CheckPathExistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        PathExistsResponse actualResponse = result.Should().BeOfType<Ok<PathExistsResponse>>().Subject.Value!;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendCheckPathExistsQueryToMediator()
    {
        // Arrange
        CheckPathExistsRequest request = _checkPathExistsRequestFixture.Create(path: @"C:\Users\TestUser\Documents", includeHiddenElements: true);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<CheckPathExistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PathExistsResponse(true));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<CheckPathExistsQuery>(q => q.Path == request.Path && q.IncludeHiddenElements == request.IncludeHiddenElements),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CheckPathExistsRequest request = _checkPathExistsRequestFixture.Create(path: @"C:\Users\TestUser\Documents", includeHiddenElements: true);
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<CheckPathExistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<PathExistsResponse>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(new PathExistsResponse(true));
            }, callInfo.Arg<CancellationToken>())));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
