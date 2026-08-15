#region ========================================================================= USING =====================================================================================
using Bogus;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Presentation.Api.Fixtures.Core.Endpoints.FileSystemManagement;

/// <summary>
/// Fixture class for creating and managing a file system structure for integration testing purposes.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemStructureFixture
{
    private static readonly bool s_hiddenAttributeIsSupported = DetectHiddenAttributeSupport();
    private readonly string _rootPath;
    private readonly Faker _faker = new();

    /// <summary>
    /// Gets a value indicating whether the fixture can create hidden elements on the current platform and file system.
    /// </summary>
    internal static bool HiddenAttributeIsSupported => s_hiddenAttributeIsSupported;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemStructureFixture"/> class.
    /// </summary>
    public FileSystemStructureFixture()
    {
        _rootPath = Path.Combine(AppContext.BaseDirectory, "TestFileSystemStructure_" + Guid.NewGuid());
    }


    /// <summary>
    /// Creates a file system structure with three nested directories, each containing two text files.
    /// Makes the second nested directory and the first file in each directory hidden.
    /// </summary>
    /// <returns>The path to the root directory of the created file system structure.</returns>
    public string CreateFileSystemStructure()
    {
        // create the root directory
        Directory.CreateDirectory(_rootPath);
        string currentPath = _rootPath;
        for (int i = 1; i <= 3; i++)
        {
            // create a nested subdirectory
            currentPath = Path.Combine(currentPath, $"NestedDirectory_{i}");
            Directory.CreateDirectory(currentPath);

            // create two text files in the current directory
            for (int j = 1; j <= 2; j++)
            {
                string filePath = Path.Combine(currentPath, $"TestFile_{j}.txt");
                File.WriteAllText(filePath, _faker.Lorem.Paragraph());
                // make the first file in each directory hidden
                if (j == 1)
                    SetHidden(filePath);
            }
            // make the second nested directory hidden
            if (i == 2)
                currentPath = SetHidden(currentPath);
        }
        return currentPath;
    }

    /// <summary>
    /// Detects whether the fixture can create hidden elements on the current platform and file system.
    /// </summary>
    /// <returns><see langword="true"/> when hidden elements can be created, otherwise <see langword="false"/>.</returns>
    private static bool DetectHiddenAttributeSupport()
    {
        // on Unix-like systems the fixture hides elements with the dot-prefix convention, which the file system always supports
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            return true;
        // on Windows the fixture relies on the hidden file attribute, which some file systems (for example network drives) do not persist
        string probePath = Path.Combine(AppContext.BaseDirectory, "lumina_hidden_attribute_probe_" + Guid.NewGuid());
        try
        {
            File.WriteAllText(probePath, string.Empty);
            File.SetAttributes(probePath, File.GetAttributes(probePath) | FileAttributes.Hidden);
            return (File.GetAttributes(probePath) & FileAttributes.Hidden) == FileAttributes.Hidden;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (File.Exists(probePath))
            {
                try
                {
                    File.SetAttributes(probePath, FileAttributes.Normal);
                    File.Delete(probePath);
                }
                catch
                {
                }
            }
        }
    }

    /// <summary>
    /// Sets the hidden attribute for a file or directory.
    /// </summary>
    /// <param name="path">The path to the file or directory.</param>
    /// <returns>The path to the hidden file or directory.</returns>
    private static string SetHidden(string path)
    {
        if (OperatingSystem.IsWindows())
            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // On Unix-like systems, rename the file/directory to start with a dot
            string directory = Path.GetDirectoryName(path)!;
            string name = Path.GetFileName(path);
            string newPath = Path.Combine(directory, "." + name);
            if (Directory.Exists(path))
                Directory.Move(path, newPath);
            else if (File.Exists(path))
                File.Move(path, newPath);
            path = newPath;
        }
        return path;
    }

    /// <summary>
    /// Deletes the file system structure created for testing purposes.
    /// </summary>
    public void CleanupFileSystemStructure()
    {
        if (Directory.Exists(_rootPath))
            Directory.Delete(_rootPath, recursive: true);
    }
}
