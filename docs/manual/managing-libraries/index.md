# Managing Libraries

## Contents

**Getting started**

1. [Introduction](../index.md)
2. [Installation](../installation/index.md)

**Using Lumina**

3. [Using Lumina](../using-lumina/index.md)

**Administration**

4. **Managing Libraries**
5. [Plugins](../plugins/index.md)
6. [Themes](../themes/index.md)

A media library is how you tell Lumina where a part of your collection lives and what kind of media it holds. Lumina does not wander your disk on its own: you create a library, point it at one or more folders, and only then does it read those folders and build a catalogue of what is inside them. This page covers everything you can do with a library: creating it, teaching it how to read your folders, scanning it, editing it, and removing it.

## What a library is

A library is a name, a media type, and one or more content locations. It belongs to the user who created it, but administrators, and anyone holding the permission to create libraries, can manage any library, including the ones they did not create.

The library keeps a catalogue of the items it indexed, together with their metadata and artwork. Your actual files stay exactly where they are and are only read, except for the one opt-in setting that writes metadata back (see below).

## Supported library types and file formats

Lumina can scan two kinds of libraries today:

- **Book** libraries, for books and e-books. During discovery Lumina recognizes these file extensions: `.pdf`, `.epub`, `.mobi`, `.azw`, `.azw3`, `.cbz`, `.cbr`, `.djvu`, `.fb2`, `.lit`, `.prc`, `.txt`, `.doc`, `.docx` and `.rtf`. Of those, the ones a user can open and read in the browser depend on the enabled reader plugins. The EPUB and PDF readers are bundled with Lumina, but readers are disabled by default, so a freshly created book library cannot open any book until you enable one for it. The rest are still indexed and listed in the catalogue, but cannot be opened for reading until a reader for their format is enabled.
- **Music** libraries, for artists, albums and tracks. During discovery Lumina recognizes these file extensions: `.mp3`, `.flac`, `.m4a`, `.aac`, `.ogg`, `.opus`, `.wav`, `.wma`, `.aiff`, `.aif`, `.ape`, `.wv`, `.dsf`, `.dff` and `.mka`.

The other types in Lumina's model, such as movies, TV shows, comics, manga, photos, podcasts and audiobooks, are designed but not implemented yet. There is no scanner for them, so they cannot be scanned.

A scan includes hidden files and folders.

## Creating a library

Open the Libraries management page, choose to add a library, and fill in the form. Every field is described below, with the value it takes when you do not change it.

| Field | Default | What it does |
| --- | --- | --- |
| Title | none | The name of the library, for example `Books` or `Music`. Required, up to 255 characters. |
| Type | none | The media type. Choose `Book` or `Music` today. Required. |
| Content locations | none | The folders that make up the library. Required, and you can add more than one. Pick them with the built-in file system browser, which reaches folders on the server from any browser. Each path can be up to 260 characters. |
| Cover image | none | Optional. A picture that represents the library. Must be a real image file; it is copied into Lumina's own storage and renamed, so moving or deleting the original later does not affect it. |
| Enabled | on | Whether the library is active. A disabled library is never shown and never touched. |
| Locked | off | Whether the library is frozen. A locked library is still shown, but is never changed or updated, scans included. |
| Download metadata from the web | on | Whether Lumina may look up the metadata of the items online. This only has an effect when a metadata provider is enabled for the library (see [Plugins](../plugins/index.md)). |
| Save metadata in media directories | off | Whether the metadata Lumina fetches is also written next to your media files, so other tools can read it. This is the one option that writes into your media folders. |
| Skip unchanged directories during scan | off | An opt-in speedup that skips folders whose own last-modified time has not changed since the last scan. Editing the contents of an existing file in place, without adding, removing or renaming anything in that folder, leaves the folder's time untouched, so that edit can be missed while it is on. |
| Track consumption progress | the type default | Whether the latest playback or reading position of the library's items is remembered, so a user can resume where they left off. |
| Completion threshold | the type default | The percentage of an item that must be consumed for a stopped session to be recorded as completed. Empty means completion is recorded only when it is reported explicitly. |
| Path template | the type's ideal structure | Tells Lumina how your folders are arranged on disk. Unless you customize it, it holds the default template of the library type, which is exactly the structure Lumina expects. Optional, and explained in full below. |

A library that is disabled or locked is never scanned, so those two settings double as a pause switch, and `Locked` also freezes edits.

## Path templates

To make sense of your files, Lumina needs to know how they are arranged. A path template describes that arrangement as an ordered list of parts, and Lumina uses it to pull values such as an author and a title, or an artist, a release and a track number, straight out of the folder and file names.

A template is optional. Unless you customize it, Lumina uses the default template of the library type, which describes the structure it expects. You only need to touch it when your folders do not follow that structure.

### The parts a template is made of

| Part kind | Applies to | Captures |
| --- | --- | --- |
| Literal | both | Fixed text that appears on disk, for example ` - ` or `Disk `. |
| Separator | both | The path separator between two folders. It renders as `\` on Windows and `/` elsewhere, and you never type it yourself. |
| Author | books | The name of the author. |
| Title | books | The title of the book. |
| Series | books | The name of the series the book belongs to. |
| SeriesNumber | books | The number of the book within its series. |
| BookId | books | The identifier Calibre assigns to a book on disk. |
| Artist | music | The name of the artist the release belongs to. |
| ReleaseType | music | The type of the release, such as Album, Single or Compilation. |
| ReleaseYear | music | The year the release was published. |
| ReleaseName | music | The title of the release. |
| TrackNumber | music | The number of the track on its disc. |
| TrackName | music | The title of the track. |
| DiscNumber | music | The number of the disc the track belongs to. |
| Extension | both | The file name extension, without the leading dot. |

The values a typed part captures follow their type: a `Year` captures exactly four digits, an `Integer` captures one or more digits, and a text value captures anything that is not a path separator. A part also carries a representation, which is the mask that says how the value sits in the name. A plain `{0}` marks where the value goes, and numeric parts can fix a digit width, for example `{0:00}` for exactly two digits. When you add a part, its representation is pre-filled with the type's default, so `TrackNumber` starts as `{0:00}` and `ReleaseYear` starts as `{0}`.

Parts can also be optional. An optional part may be missing from a path without breaking the match, which is how a template copes with something like a disc folder that only some releases have. Optional parts that sit next to each other are treated as one block: the whole run is either present or absent, never half of it.

### The defaults

The default template for a book library describes the Calibre layout, an author folder holding a `Title (id)` folder with a `Title - Author.ext` file inside:

```text
{Author}/{Title} ({BookId})/{Title} - {Author}.{Extension}
```

The default template for a music library describes an artist folder, then a release type folder, then a `Year - Release` folder, with an optional `Disk NN` folder, and numbered tracks named after their title:

```text
{Artist}/{ReleaseType}/{ReleaseYear} - {ReleaseName}/[Disk {DiscNumber}/]{TrackNumber} - {TrackName}.{Extension}
```

The bracketed part is the optional run, present only for multi disc releases.

### Matching rules

A file's path is read relative to the content location it was found under, and only counts as a match when the whole relative path fits the template. When it does, Lumina reads the values it needs; when it does not, the item is still indexed, Lumina simply cannot read those values from the path. For books, an unmatched item falls back to a title derived from its file name. For music, the values embedded in the file's tags take priority over the ones read from the path anyway, so the path template mostly fills in what the tags are missing.

A template is checked before it is saved. A literal part must not be empty, a typed part must contain exactly one value placeholder, and every part kind must be one the library type actually supports. An empty template is always valid and means the type's default.

### Changing the template later

The template is read on every scan, so changing it changes how your folders are interpreted from the next scan onward. If you change it after items have already been catalogued, run a scan and check the results, because the titles and numbers Lumina derives from paths can change, and an item whose path no longer matches will be catalogued with fallback values instead.

## Scanning a library

Creating a library only records where the media lives. To actually read it, you scan it. You can scan a single library, or all of them at once, from the Libraries page.

Lumina is designed to be efficient and performant. The first scan of a library reads and indexes everything it finds, so it is the slowest one. Every scan after that compares what is on disk against the previous scan and only processes what is new or changed, so it is normally much faster. How long any scan takes also depends on what is enabled for the library: metadata and artwork providers that fetch data from the web add network time on top of reading the files, and they do it only for the items that need it.

Scans run on the server in the background, so once one starts you can close the browser and come back later.

While a scan is running, its progress is shown live: how far along it is overall, how many items it has processed, and the operation it is currently on. A scan is queued before it starts, then runs, and ends up finished, cancelled or failed. You can cancel a running scan at any time, for one library or for all of them.

A disabled or locked library is refused a scan. Scans can also be driven by the scheduler, so libraries refresh themselves on a timetable without you lifting a finger.

## Editing a library

Editing a library reopens the same form, and changes take effect on the next scan. Beyond the fields above, two things change the wider picture:

- Changing the **type** of an existing library also reconciles the metadata, artwork and reader provider settings of that library, so they match the plugins that support the new type.
- Changing the **content locations** takes effect on the next scan. Items found under a location you removed are dropped from the catalogue, along with the metadata and artwork Lumina had stored for them, and items under a location you added are picked up.

## Progress tracking and completion thresholds

Two settings on a library decide whether Lumina remembers where you stopped and when a session is complete. They apply to every user of that library.

- **Track consumption progress** decides whether the latest playback or reading position of an item is remembered and offered as the resume position. When it is off, Lumina stores no resume point for the library's items.
- **Completion threshold** is the percentage of an item at which a stopped session is recorded as completed instead of stopped. It is set only while tracking is on, and it can be left empty.

When a session ends, the settings decide its status:

- **Tracking off**: the session is recorded as stopped, its position is not saved, and the completion threshold is not checked.
- **Tracking on, no threshold**: the session is recorded as stopped and its position is saved, so it can be resumed. It is completed only when completion is reported explicitly.
- **Tracking on with a threshold**: a stop at or above the threshold is recorded as completed and removes the resume point. A stop below the threshold is recorded as stopped and keeps the resume point.

The defaults by library type are:

| Library category | Track progress | Completion threshold |
| --- | --- | --- |
| Music audio (Music, Remix, SoundEffect, CoverSongAudio, LiveRecordingAudio, InterviewAudio) | no | none |
| Photo | no | none |
| Written content (Book, EBook, ComicBook, Magazine, Newspaper, Manga, GraphicNovel, AcademicPaper, SheetMusic) | yes | 100 |
| Video (TvShow, Movie, Documentary, Anime, ConcertVideo, TutorialVideo, HomeVideo, YouTubeVideo, MusicVideo, LiveRecordingVideo, InterviewVideo, CoverSongVideo) | yes | 90 |
| Audiobook and podcast (Audiobook, PodcastAudio, PodcastVideo) | yes | 95 |
| Other, non-media or unknown | no | none |

For now, only Book and Music libraries produce a working catalogue, so the written content and music defaults are the ones you will meet. Other library types can still be created, and their rows are the defaults they will use when their catalogues arrive. A value you set on a library always wins after the library is created.

When you add a library without changing these fields, Lumina applies the default of its type. When you turn tracking on for a library whose type has no default threshold, the threshold stays empty and completion is recorded only when it is reported explicitly. An empty threshold is not zero: zero would complete every stopped session as soon as it stops.

The threshold must be a whole number from 0 to 100, and the form refuses any other value. Changing it affects sessions that stop afterwards; a session that was already recorded keeps the status it was recorded with. Turning tracking off clears the threshold. The setting does not change what a library can play or read, only whether a resume point is kept and how a stopped session is recorded.

## Deleting a library

Deleting a library removes it from Lumina: its configuration, its scan history, the items in its catalogue, their metadata and the artwork Lumina stored for them, and its provider settings. The cover image Lumina copied into its own storage is removed as well. Your original files are left untouched.

Deletion is not reversible from Lumina. If you want the library back afterwards, you recreate it and scan again. Because of that, and because this is pre-alpha software, treat deletion as final and make sure of what you are deleting.

## Where your files live

Content locations are folders on the machine that runs the API server, and the file system browser is how you reach them from wherever you happen to be. Folders under the media root that Lumina manages itself (its database, the cover images it copies, the artwork and thumbnails it generates, and the plugin and theme storage) live in Lumina's own storage rather than in your media folders, so your folders stay yours.

## Settings that affect your libraries

These are per user settings, each user changes their own from the settings page in the client, and they shape how libraries are browsed and how their items are read.

| Setting | Default | What it does |
| --- | --- | --- |
| Enable pagination | on | Whether lists of library items are split into pages. |
| Items per page | 48 | How many items a page holds when pagination is on. |
| Ignore "The" prefix for the alpha picker | off | Whether titles that begin with "The" are filed under the following word in the alphabetical picker, so that "The Hobbit" sorts under H rather than under T. |
| Aggregate metadata when missing | off | Whether a metadata field that a provider could not fill is completed from the other enabled metadata providers of that item, instead of staying empty. |
| Aggregate artwork when missing | off | The same idea for artwork: when the preferred provider has nothing, try the others. |
| Render PDFs as images | off | Whether PDFs are shown as page images, which keeps their original layout but makes their text non selectable and non searchable. Off uses the text layer instead. |
| Preserve book styles | on | Whether a book keeps its own fonts, colors and alignment when it is rendered. A book's styles can load resources from external servers, which those servers could observe; turning this off strips the styles and removes that possibility, at the cost of the book's original look. |

## Things to watch out for

- **`Save metadata in media directories` writes into your media folders.** It is off by default; turn it on only if you want metadata files to accompany your media.
- **`Skip unchanged directories during scan` can miss in-place edits.** It trusts each folder's own last-modified time, so editing the contents of an existing file without adding, removing or renaming anything in its folder leaves the folder unchanged and it gets skipped. Adding, removing or renaming a file does change the folder, and is still seen.
- **`Locked` blocks everything.** A locked library is not scanned and not updated, including by the scheduler.
- **`Disabled` hides the library.** A disabled library is neither shown on the dashboard nor touched.
- **Deleting a library removes Lumina's catalogue for it.** The files remain, but the metadata and artwork Lumina held for that library do not. Consumption history and resume points are kept as records of what was consumed. The history shows the items as unavailable until you delete it. **Remove from continue watching** removes every resume point of the selected user, not only the ones from the deleted library.
- **Removing a folder from a library, or deleting a file, removes its catalogued items on the next scan.** The catalogue tracks what is on disk, so what disappears from disk also disappears from Lumina, along with the metadata and artwork Lumina stored for it.
- **Changing the path template can change how items are read.** Re-scan and check the results after a change.
- **Tracking off means no resume points.** A library that does not track consumption progress never offers a saved position, even if a client reports one.
- **An empty completion threshold is not zero.** Empty means completion is recorded only when it is reported explicitly, while zero completes a stopped session immediately.
- **Deleting history and removing resume points are separate.** Clearing a user's history removes all of that user's recorded sessions and keeps their resume points, while **Remove from continue watching** removes all of that user's resume points and keeps their history. Neither action is limited by the library, media kind or date filters of the statistics page.
- **For now, only Book and Music libraries produce a working catalogue.** Other types can be created, but they do not scan into a usable catalogue yet.
- **Not every book format is readable.** Recognized formats are indexed, but only those with an enabled reader plugin can be opened in the browser.
