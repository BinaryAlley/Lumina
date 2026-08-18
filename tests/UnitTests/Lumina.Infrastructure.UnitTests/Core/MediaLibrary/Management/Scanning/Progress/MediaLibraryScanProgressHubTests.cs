#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Progress;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Progress;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanProgressHub"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanProgressHubTests
{
    [Fact]
    public async Task SubscribeToScan_WhenCalled_ShouldAddTheConnectionToTheScanGroup()
    {
        // Arrange
        MediaLibraryScanProgressHub sut = new();
        HubCallerContext context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns("connection-1");
        IGroupManager groupManager = Substitute.For<IGroupManager>();
        sut.Context = context;
        sut.Groups = groupManager;
        Guid scanId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        // Act
        await sut.SubscribeToScan(scanId, userId);

        // Assert
        await groupManager.Received(1).AddToGroupAsync("connection-1", $"{scanId}-{userId}", CancellationToken.None);
    }
}
