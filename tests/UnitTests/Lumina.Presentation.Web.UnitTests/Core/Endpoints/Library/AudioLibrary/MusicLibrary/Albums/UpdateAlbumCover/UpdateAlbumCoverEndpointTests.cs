#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbumCover;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbumCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateAlbumCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumCoverEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly UpdateAlbumCoverEndpoint _sut;
    private readonly UpdateMusicCoverDtoFixture _updateMusicCoverDtoFixture = new();
    private static readonly Guid s_libraryId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAlbumCoverEndpointTests"/> class.
    /// </summary>
    public UpdateAlbumCoverEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<UpdateAlbumCoverEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithFile_ShouldUploadCoverToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        _mockApiHttpClient.PutMultipartAsync<UpdateMusicCoverDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateMusicCoverDtoFixture.Create(coverPath: "/media/albums/cover.jpg"));
        ConfigureRequest(artistId, albumId, [1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateMusicCoverDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(s_libraryId.ToString(), artistId, albumId)),
            Arg.Any<Stream>(),
            Arg.Is<string>(fileName => fileName == "cover.jpg"),
            Arg.Is<string>(fieldName => fieldName == "cover"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreNotParseable_ShouldUploadCoverWithEmptyIds()
    {
        // Arrange
        _mockApiHttpClient.PutMultipartAsync<UpdateMusicCoverDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateMusicCoverDtoFixture.Create(coverPath: "/media/albums/cover.jpg"));
        ConfigureRequest("not-a-guid", "not-a-guid", [1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateMusicCoverDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(s_libraryId.ToString(), Guid.Empty, Guid.Empty)),
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsNotParseable_ShouldUploadCoverWithEmptyLibraryId()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        _mockApiHttpClient.PutMultipartAsync<UpdateMusicCoverDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateMusicCoverDtoFixture.Create(coverPath: "/media/albums/cover.jpg"));
        ConfigureRequest(artistId, albumId, [1, 2, 3], "cover.jpg");
        // The query value is normalized to a Guid before it is substituted into the upstream URL, so a crafted value can never escape its segment.
        _sut.HttpContext.Request.QueryString = new QueryString("?libraryId=not-a-library-guid");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateMusicCoverDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(Guid.Empty.ToString(), artistId, albumId)),
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsMissing_ShouldUploadCoverWithEmptyLibraryId()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        _mockApiHttpClient.PutMultipartAsync<UpdateMusicCoverDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateMusicCoverDtoFixture.Create(coverPath: "/media/albums/cover.jpg"));
        ConfigureRequest(artistId, albumId, [1, 2, 3], "cover.jpg");
        _sut.HttpContext.Request.QueryString = new QueryString(string.Empty);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateMusicCoverDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(Guid.Empty.ToString(), artistId, albumId)),
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsCoverPath_ShouldReturnSuccessJsonWithPath()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        const string EXPECTED_PATH = "/media/albums/cover.jpg";
        _mockApiHttpClient.PutMultipartAsync<UpdateMusicCoverDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateMusicCoverDtoFixture.Create(coverPath: EXPECTED_PATH));
        ConfigureRequest(artistId, albumId, [1, 2, 3], "cover.jpg");

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(EXPECTED_PATH, jsonDocument.RootElement.GetProperty("data").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFormHasNoFile_ShouldForwardRequestWithoutFileToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        const string EXPECTED_PATH = "/media/albums/cover.jpg";
        _mockApiHttpClient.PutMultipartAsync<UpdateMusicCoverDto>(Arg.Any<string>(), Arg.Any<Stream?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateMusicCoverDtoFixture.Create(coverPath: EXPECTED_PATH));
        ConfigureRequestWithoutFiles(artistId, albumId);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateMusicCoverDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(s_libraryId.ToString(), artistId, albumId)),
            Arg.Is<Stream?>(stream => stream == null),
            Arg.Is<string?>(fileName => fileName == null),
            Arg.Is<string>(fieldName => fieldName == "cover"),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(EXPECTED_PATH, jsonDocument.RootElement.GetProperty("data").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestIsNotMultipart_ShouldForwardRequestWithoutFileToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        const string EXPECTED_PATH = "/media/albums/cover.jpg";
        _mockApiHttpClient.PutMultipartAsync<UpdateMusicCoverDto>(Arg.Any<string>(), Arg.Any<Stream?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateMusicCoverDtoFixture.Create(coverPath: EXPECTED_PATH));
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={s_libraryId}");
        _sut.HttpContext.Request.ContentType = "application/json";

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateMusicCoverDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(s_libraryId.ToString(), artistId, albumId)),
            Arg.Is<Stream?>(stream => stream == null),
            Arg.Is<string?>(fileName => fileName == null),
            Arg.Is<string>(fieldName => fieldName == "cover"),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(EXPECTED_PATH, jsonDocument.RootElement.GetProperty("data").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFormIsMalformed_ShouldForwardRequestWithoutFileToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        const string EXPECTED_PATH = "/media/albums/cover.jpg";
        _mockApiHttpClient.PutMultipartAsync<UpdateMusicCoverDto>(Arg.Any<string>(), Arg.Any<Stream?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateMusicCoverDtoFixture.Create(coverPath: EXPECTED_PATH));
        ConfigureMalformedRequest(artistId, albumId);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateMusicCoverDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(s_libraryId.ToString(), artistId, albumId)),
            Arg.Is<Stream?>(stream => stream == null),
            Arg.Is<string?>(fileName => fileName == null),
            Arg.Is<string>(fieldName => fieldName == "cover"),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(EXPECTED_PATH, jsonDocument.RootElement.GetProperty("data").GetString());
    }

    /// <summary>
    /// Configures a multipart form request carrying a single file.
    /// </summary>
    /// <param name="artistId">The route Id of the artist.</param>
    /// <param name="albumId">The route Id of the album whose cover is uploaded.</param>
    /// <param name="content">The bytes of the uploaded file.</param>
    /// <param name="fileName">The name of the uploaded file.</param>
    private void ConfigureRequest(Guid artistId, Guid albumId, byte[] content, string fileName)
    {
        ConfigureRequest(artistId.ToString(), albumId.ToString(), content, fileName);
    }

    /// <summary>
    /// Configures a multipart form request carrying a single file.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="albumId">The raw route value of the album Id.</param>
    /// <param name="content">The bytes of the uploaded file.</param>
    /// <param name="fileName">The name of the uploaded file.</param>
    private void ConfigureRequest(string artistId, string albumId, byte[] content, string fileName)
    {
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId;
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={s_libraryId}");
        MemoryStream coverStream = new(content);
        IFormFile formFile = new FormFile(coverStream, 0, content.Length, "cover", fileName);
        FormFileCollection files = [formFile];
        IFormCollection form = new FormCollection([], files);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }

    /// <summary>
    /// Configures a multipart form request without any file.
    /// </summary>
    /// <param name="artistId">The route Id of the artist.</param>
    /// <param name="albumId">The route Id of the album whose cover is uploaded.</param>
    private void ConfigureRequestWithoutFiles(Guid artistId, Guid albumId)
    {
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={s_libraryId}");
        IFormCollection form = new FormCollection([]);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }

    /// <summary>
    /// Configures a multipart form request whose body cannot be parsed.
    /// </summary>
    /// <param name="artistId">The route Id of the artist.</param>
    /// <param name="albumId">The route Id of the album whose cover is uploaded.</param>
    private void ConfigureMalformedRequest(Guid artistId, Guid albumId)
    {
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={s_libraryId}");
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        IFormCollection malformedForm = Substitute.For<IFormCollection>();
        malformedForm.Files.Returns(_ => throw new InvalidDataException("Malformed multipart body."));
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(malformedForm));
    }

    /// <summary>
    /// Builds the upstream endpoint the cover upload is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The raw Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <param name="albumId">The normalized Id of the album.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(string libraryId, Guid artistId, Guid albumId)
    {
        return ApiRoutes.Music.UPDATE_ALBUM_COVER
            .Replace("{libraryId}", libraryId)
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString());
    }
}
