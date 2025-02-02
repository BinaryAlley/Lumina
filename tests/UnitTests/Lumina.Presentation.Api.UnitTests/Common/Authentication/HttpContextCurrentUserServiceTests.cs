#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Common.Authentication;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Common.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="HttpContextCurrentUserService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class HttpContextCurrentUserServiceTests
{
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly HttpContextCurrentUserService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpContextCurrentUserServiceTests"/> class.
    /// </summary>
    public HttpContextCurrentUserServiceTests()
    {
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _sut = new HttpContextCurrentUserService(_mockHttpContextAccessor);
    }

    [Fact]
    public void UserId_WhenHttpContextIsNull_ShouldReturnNull()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        Guid? result = _sut.UserId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UserId_WhenUserIsNull_ShouldReturnNull()
    {
        // Arrange
        HttpContext mockHttpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.HttpContext.Returns(mockHttpContext);

        // Act
        Guid? result = _sut.UserId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UserId_WhenNameIdentifierClaimIsMissing_ShouldReturnNull()
    {
        // Arrange
        HttpContext mockHttpContext = new DefaultHttpContext();
        ClaimsIdentity identity = new(
        [
            new(ClaimTypes.Name, "testuser")
        ]);
        mockHttpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.HttpContext.Returns(mockHttpContext);

        // Act
        Guid? result = _sut.UserId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UserId_WhenNameIdentifierClaimIsInvalidGuid_ShouldReturnNull()
    {
        // Arrange
        HttpContext mockHttpContext = new DefaultHttpContext();
        ClaimsIdentity identity = new(
        [
            new(ClaimTypes.NameIdentifier, "not-a-guid")
        ]);
        mockHttpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.HttpContext.Returns(mockHttpContext);

        // Act
        Guid? result = _sut.UserId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UserId_WhenNameIdentifierClaimIsValidGuid_ShouldReturnGuid()
    {
        // Arrange
        Guid expectedUserId = Guid.NewGuid();
        HttpContext mockHttpContext = new DefaultHttpContext();
        ClaimsIdentity identity = new(
        [
            new(ClaimTypes.NameIdentifier, expectedUserId.ToString())
        ]);
        mockHttpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.HttpContext.Returns(mockHttpContext);

        // Act
        Guid? result = _sut.UserId;

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    public void UserId_WhenMultipleNameIdentifierClaimsExist_ShouldReturnFirstValidGuid()
    {
        // Arrange
        Guid expectedUserId = Guid.NewGuid();
        HttpContext mockHttpContext = new DefaultHttpContext();
        ClaimsIdentity identity = new(
        [
            new(ClaimTypes.NameIdentifier, expectedUserId.ToString()),
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        ]);
        mockHttpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.HttpContext.Returns(mockHttpContext);

        // Act
        Guid? result = _sut.UserId;

        // Assert
        Assert.Equal(expectedUserId, result);
    }
}
