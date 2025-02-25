#region ========================================================================= USING =====================================================================================
using Bogus;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Fixtures;

/// <summary>
/// Fixture class for creating and managing a file system structure for integration testing purposes.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemStructureFixture
{
    private readonly string _rootPath;
    private readonly Faker _faker = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemStructureFixture"/> class.
    /// </summary>
    public FileSystemStructureFixture()
    {
        _rootPath = System.IO.Path.Combine(AppContext.BaseDirectory, "TestFileSystemStructure_" + Guid.NewGuid());
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
            currentPath = System.IO.Path.Combine(currentPath, $"NestedDirectory_{i}");
            Directory.CreateDirectory(currentPath);

            // create two text files in the current directory
            for (int j = 1; j <= 2; j++)
            {
                string filePath = System.IO.Path.Combine(currentPath, $"TestFile_{j}.txt");
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
    /// Sets the hidden attribute for a file or directory.
    /// </summary>
    /// <param name="path">The path to the file or directory.</param>
    private static string SetHidden(string path)
    {
        if (OperatingSystem.IsWindows())
            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // On Unix-like systems, rename the file/directory to start with a dot
            string directory = System.IO.Path.GetDirectoryName(path)!;
            string name = System.IO.Path.GetFileName(path);
            string newPath = System.IO.Path.Combine(directory, "." + name);
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
