# Plugins

## Contents

**Getting started**

1. [Introduction](../index.md)
2. [Installation](../installation/index.md)

**Using Lumina**

3. [Using Lumina](../using-lumina/index.md)

**Administration**

4. [Managing Libraries](../managing-libraries/index.md)
5. **Plugins**
6. [Themes](../themes/index.md)

Plugins are how Lumina learns to understand more than its core knows. They supply the metadata and the artwork of your library items, and they open book formats for reading. They are separate units you install, enable, rank and configure, so the core stays lean and the knowing arrives in pieces that you control.

## What a plugin can do

A plugin can provide one or more of three capabilities:

- **Metadata providers** fill in the metadata of a library item, either from the web or from files on disk. Each provider declares which library types it works with and whether it needs to reach the internet.
- **Artwork providers** supply the covers and other images of a library item.
- **Book readers** open a book format and turn it into something the browser can render, with the original layout, styling and structure.

Lumina ships with these plugins:

| Plugin | Provides | For | Needs web | What it does |
| --- | --- | --- | --- | --- |
| MusicBrainz Metadata | metadata | Music | yes | Retrieves artist, album and track metadata from [MusicBrainz](https://musicbrainz.org/). |
| ID3 Metadata | metadata | Music | no | Populates artist, album and track metadata from the tags embedded in the audio files. |
| Local Music Artwork | artwork | Music | no | Reads artist and album artwork from the images stored in the folders of a music library. |
| Cover Art Archive | artwork | Music | yes | Retrieves the cover, back, booklet, medium and other artwork of music albums from [Cover Art Archive](https://coverartarchive.org/). |
| Calibre Metadata | metadata, artwork | Book | no | Reads book metadata and covers from the OPF files of a Calibre library. |
| Open Library Metadata | metadata | Book | yes | Retrieves book and edition metadata from [Open Library](https://openlibrary.org/). |
| EPUB Reader | book reader | Book | no | Opens EPUB format books. |
| PDF Reader | book reader | Book | no | Opens PDF format books. |

The `Needs web` column is worth pausing on: the providers marked `yes` reach out to the internet when a library scan runs, and that can leak out your location and the content of your libraries via inference. They also add increased time to scans, obviously.

## Installing a plugin

Plugins are managed from the administration area, on the Plugins page, which only administrators can open. To install one, upload either a single `.dll` assembly or a `.zip` archive that holds the assembly together with any dependencies it needs.

The uploaded files are placed in the plugin storage directory, `plugins` by default, and are picked up at the **next startup of the API**. In other words, after installing a plugin, restart the API server for it to take effect. Installing is a supervised operation: the archive is unpacked into a flat set of assemblies inside the plugin directory, and any directory component in the upload is stripped, so a crafted archive cannot write outside that directory.

Once installed and restarted, the plugin appears on the Plugins page with its name, author, version and description.

## Load status

Each plugin is listed with one of two states:

- **Loaded**, meaning Lumina found and initialized it.
- **Failed to load**, meaning the assembly was found but could not be initialized, in which case the page also shows the error. A plugin that fails to load contributes nothing.

Lumina reconciles the list at every startup. A plugin that is no longer present in the storage directory is forgotten, together with the per library settings that referenced it.

## Uninstalling a plugin

There is no uninstall button. To remove a plugin, delete its assembly (and any files that came with it) from the plugin storage directory and restart the API. The plugin then disappears from the list, and the settings that enabled it for your libraries are cleaned up.

## Enabling, ranking and configuring

Plugins themselves are installed once, but they are turned on per library. Open a library to edit it and you will find a section for metadata providers and one for artwork providers. A book library has one more, for its book readers:

- **Metadata providers.** Each is switched on or off, and ordered. The providers are tried in that order, the first one that produces a result is used, and, when you enable the `Aggregate metadata when missing` setting, the fields it left empty are filled from the providers that come after it.
- **Artwork providers.** The same idea for images: switched on or off, and ordered, so you decide which source wins when more than one has something.
- **Book readers (book libraries only).** These are the plugins that can open a book. Each is switched on or off, and a book can be opened for reading only through an enabled reader whose format matches it, so an EPUB needs the EPUB reader, a PDF the PDF reader, and so on.

A plugin that supports a library type but is not wanted for a particular library can simply be left off there, without affecting your other libraries.

## Plugin settings

A plugin can declare its own settings, which Lumina renders on the Plugins page next to it. Each setting is one of four kinds: a text field, a number, a true/false switch, or a choice from a fixed list. Values you save are stored by Lumina and handed to the plugin on the next run.

The shipped plugins use these settings for practical things, such as the base address of a service, a contact email for a polite API client, lookup limits, and the minimum interval between requests. These settings are global to the plugin, not per library.

## Writing your own plugin

Anyone can write a plugin, and it is the intended way to teach Lumina about a new source or a new format. A plugin is a .NET assembly targeting the same framework as the server, referencing the `Lumina.Plugins.Contracts` project. To be a plugin it implements `IPlugin`, which describes it and lists its settings, and `IPluginServiceRegistrator`, which wires its services up. To contribute a capability it also implements one or more of `IMetadataProvider`, `IArtworkProvider` and `IBookReader`. Package the built assembly, and any dependencies, into a ZIP and install it like any other plugin.

## Security

Treat plugins with more caution than themes. A theme is HTML, CSS and JavaScript, but a plugin is a .NET assembly that Lumina loads into the API process itself. It runs with the full trust of the server: it can read and write files, open network connections and do anything else the API is allowed to do. There is no sandbox around a plugin once it is loaded.

That means a plugin from an untrusted source effectively owns your server. Only install plugins you trust, and if you cannot vouch for the code, read it first. The bundled plugins are written and reviewed by the project and are safe to use.
