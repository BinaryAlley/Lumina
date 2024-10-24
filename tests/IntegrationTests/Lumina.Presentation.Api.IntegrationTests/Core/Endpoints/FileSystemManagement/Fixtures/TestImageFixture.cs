#region ========================================================================= USING =====================================================================================
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Fixtures;

/// <summary>
/// Fixture class for creating and managing a test image for integration testing purposes.
/// </summary>
[ExcludeFromCodeCoverage]
public class TestImageFixture : IDisposable
{
    /// <summary>
    /// Gets the path of the created test image.
    /// </summary>
    public string ImagePath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestImageFixture"/> class.
    /// </summary>
    public TestImageFixture()
    {
        ImagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"TestImage_{Guid.NewGuid()}.jpg");
        CreateTestImage();
    }

    /// <summary>
    /// Creates a test image file.
    /// </summary>
    private void CreateTestImage()
    {
        using (Image image = new Image<Rgba32>(100, 100))
            image.Save(ImagePath, new JpegEncoder());
    }

    /// <summary>
    /// Disposes the fixture, ensuring the test image is deleted.
    /// </summary>
    public void Dispose()
    {
        if (File.Exists(ImagePath))
            File.Delete(ImagePath);
    }
}
