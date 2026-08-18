#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="TaskUtilities"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TaskUtilitiesTests
{
    [Fact]
    public async Task FireAndForgetSafeAsync_WhenTaskCompletesSuccessfully_ShouldInvokeOnComplete()
    {
        // Arrange
        TaskCompletionSource<bool> completionSource = new();
        bool onCompleteInvoked = false;

        // Act
        Task.CompletedTask.FireAndForgetSafeAsync(() =>
        {
            onCompleteInvoked = true;
            completionSource.SetResult(true);
        });

        // Assert
        await completionSource.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(onCompleteInvoked);
    }

    [Fact]
    public async Task FireAndForgetSafeAsync_WhenTaskFaults_ShouldSwallowExceptionAndInvokeOnComplete()
    {
        // Arrange
        TaskCompletionSource<bool> completionSource = new();
        bool onCompleteInvoked = false;
        Task faultedTask = Task.FromException(new InvalidOperationException("Expected failure"));

        // Act
        faultedTask.FireAndForgetSafeAsync(() =>
        {
            onCompleteInvoked = true;
            completionSource.SetResult(true);
        });

        // Assert
        await completionSource.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(onCompleteInvoked);
    }

    [Fact]
    public async Task FireAndForgetSafeAsync_WhenTaskIsCanceled_ShouldSwallowExceptionAndInvokeOnComplete()
    {
        // Arrange
        TaskCompletionSource<bool> completionSource = new();
        bool onCompleteInvoked = false;
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        Task canceledTask = Task.FromCanceled(cancellationTokenSource.Token);

        // Act
        canceledTask.FireAndForgetSafeAsync(() =>
        {
            onCompleteInvoked = true;
            completionSource.SetResult(true);
        });

        // Assert
        await completionSource.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(onCompleteInvoked);
    }
}
