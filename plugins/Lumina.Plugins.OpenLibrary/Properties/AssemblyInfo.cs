using System.Runtime.CompilerServices;

// Test-enabling declaration for the Open Library plugin assembly.
// Mirrors the InternalsVisibleTo pattern present in every other testable source project,
// so that the internal Open Library types can be exercised by their unit tests.
[assembly: InternalsVisibleTo("Lumina.Plugins.OpenLibrary.UnitTests")]
[assembly: InternalsVisibleTo("Lumina.Plugins.OpenLibrary.Fixtures")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
