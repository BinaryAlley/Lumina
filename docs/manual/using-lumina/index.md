# Using Lumina

## Contents

**Getting started**

1. [Introduction](../index.md)
2. [Installation](../installation/index.md)

**Using Lumina**

3. **Using Lumina**

**Administration**

4. [Managing Libraries](../managing-libraries/index.md)
5. [Plugins](../plugins/index.md)
6. [Themes](../themes/index.md)

This is the guide to actually using Lumina from the web client, once a server is running and at least one library exists. It covers signing in, finding your way around, browsing and reading what you have, editing an item, and the settings that shape the experience.

## Getting around

The header menu rearranges itself depending on who you are. Everyone sees a site entry that leads home, an account section, a library section once signed in, a language picker, a tools section, and a help section. Administrators also get an administrator section for roles, permissions, plugins, themes and scheduled jobs.

The home page is your catalogue: a grid of the libraries you are allowed to see, each shown as a card with its cover when it has one. Selecting a card opens that library's browsing page. Some library types do not have a browsing page yet, and their cards appear as plain panels rather than links.

## Your account

Accounts are local to your Lumina server. From the account section you can:

- **Register** a new account, when registration is available on the server.
- **Log in** with your username and password. If your account uses two factor authentication, you are also asked for the current one time code from your authenticator app.
- **Recover a password** when you have forgotten it, using your username and, when two factor authentication is on, a one time code.
- **Change your password** while signed in, by giving the current one and the new one twice.
- **Open your profile** to see your account details.
- **Log out** to end the session.

What each account can do is decided by its role and permissions, which administrators manage.

## Browsing your books

Opening a book library shows a page with the library title, a search box, a sort toggle, a count of items, a grid of book cards, an alphabetical picker down the side, and pagination at the bottom.

- **Search** filters the books by name; press Enter or use the search button.
- **Sort** toggles between ascending and descending order.
- **The alpha picker** jumps to titles starting with a chosen character. It offers All, then the number and symbol keys, then A to Z. You can also press a letter key to filter, and press Escape to clear the filter.
- **Pagination** splits the list into pages when you have it enabled, with previous and next buttons and a page indicator. When it is off, the whole list is shown and the count is the total.

Each book is a card with its cover and title. Clicking a card opens the book in the reader; a book can only be opened when a reader for its format is enabled for the library. Right-clicking a card, or using its actions button, opens a menu with **Read** and **View details**.

## Reading a book

The reader opens the book in the browser and gives you a small toolbar and the book itself:

- **Back to library** returns to the browsing page you came from.
- **Table of contents** toggles a panel of the book's sections; selecting one jumps to it.
- **Previous** and **Next** move between sections, and the position indicator shows where you are.
- **A-** and **A+** change the text size, within a comfortable range.

A book opens only through an enabled reader for its format. The EPUB and PDF readers are bundled and can be enabled for a library. A scanned PDF has no text to extract, so it can only be shown as page images, which is what the `Render PDFs as images` setting turns on.

Book libraries track consumption progress by default. Reopening a book continues from the last section of a tracked reading session. A short reading, such as opening a book, turning a few pages and closing it, may not reach the minimum needed to be tracked, so it can leave no saved position and the book opens at the beginning. Reaching the library's completion threshold, 100 percent for a book library by default, completes the reading session and removes the saved position. The threshold is a per-library setting, described in [Managing Libraries](../managing-libraries/index.md).

## Browsing your music

Opening a music library shows a page with the library title, a search box, a sort toggle, a count of items, a grid of artist cards, an alphabetical picker down the side, and pagination at the bottom.

- **Search** filters the artists by name; press Enter or use the search button.
- **Sort** toggles between ascending and descending order.
- **The alpha picker** jumps to names starting with a chosen character. It offers All, then the number and symbol keys, then A to Z. You can also press a letter key to filter, and press Escape to clear the filter.
- **Pagination** splits the list into pages when you have it enabled, with previous and next buttons and a page indicator. When it is off, the whole list is shown and the count is the total.

Each artist is a card; selecting it opens the artist's page, and the card's actions menu offers **View details**, **Play**, **Shuffle**, **Add to playlist** and **Delete**. The heart on the card marks the artist as a favorite.

The artist page is also where you edit the artist. It lists the artist's albums as cards, each leading to its own album page, followed by the artist's general information, contributors, ratings, genres, tags, aliases, areas and identifiers. The album page holds the album's information and its tracks, and the track page holds the track's details. The album page also shows the artwork Lumina stored for the album as a strip of thumbnails beside the main image, and selecting a thumbnail displays that image in place of the cover. Deleting an artist, album or track removes it from Lumina's catalogue; your files are not touched.

The music pages also play what they show. The artist page has a **Play all** control that plays the artist's releases in the order the page presents them, one release after another. The actions menu of a music library card on the home page, of an artist card, and of an album card offers **Play** and **Shuffle**. Play starts the library at its first artist and continues through the remaining artists in artist page order, starts the artist's discography in the order the artist page presents it, or starts the release in disc and track order. Shuffle starts the library, the artist's discography or the release with a list of up to 100 tracks sampled uniformly at random from every track of that scope, and continues in random order without repeating a track that is already queued. The album page has a **Play album** control that plays the release in disc and track order, and every track row has its own play control that plays just that track. The track page has a **Play track** control for the track it shows. Every track row on the album page also has an actions menu: **Add to playlist** opens the playlist picker for that track, **Add to play queue** appends it to the end of the player queue or starts it when the queue is empty, and **Play all from here** replaces the queue with that track and the rest of the album and starts playing. Starting playback opens the player bar at the bottom of the window. It keeps playing while you move between pages, and the bar shows the track name, with the album name underneath when the track belongs to a named album, the progress of the current track, a segmented VU meter that follows the level of the track, and the volume, together with previous, play or pause, stop, next and repeat controls. When you expand it, it also shows the cover art, the current track name, the release type, the release name and year, the artist name, and a carousel of the album's stored artwork, next to the queue, where every entry carries its album cover, including when only one track is queued. Selecting an artwork in the carousel shows it as the main cover. The expanded queue moves the current track to the top whenever playback advances, so it stays visible even in a long list. The current track's favorite is the heart on the bar between the repeat and mute controls, and every entry of the expanded queue carries its own heart. The queue's own playlist picker appends the whole queue, in order, to one of your playlists. The double chevron at the right points up to expand the bar and down to collapse it. Stop is the square control next to play or pause; in the condensed bar it stops playback and slides the bar out of view.

Music libraries do not remember positions by default. When progress tracking is turned on for a music library, starting a track resumes it from the last reported position, and stopping it at or above the library's completion threshold, when one is set, completes the session instead of saving a position.

## Playlists

The **Playlists** entry under Audio opens your playlists. A playlist survives signing out and back in, but the player queue itself does not. A full page reload still starts with an empty queue and a hidden player.

From the playlists page you can open a playlist or select **Create** to open the dedicated page that creates a new playlist with a name and an optional description. Selecting an existing playlist opens its own page, which shows its tracks in order and lets you:

- **Add a track** by choosing a music library, then an artist, an album and a track, and selecting **Add**. A track can appear more than once in the same playlist.
- **Reorder the tracks** by dragging a row by its handle, or with the up and down controls on each row. The order is saved immediately.
- **Remove a track** from the playlist. The track itself, and the file behind it, are not touched.
- **Play a single track** with its play control, or **Play playlist** to hand the whole ordered list to the player in one go. Each row also has a heart that marks the track as a favorite and a plus control that adds it to another playlist. While something is playing, the expanded player's **Add to playlist** button appends its current queue to one of your playlists.
- **Add a whole library, artist or album** from the card menu of its page: choose **Add to playlist**, pick a playlist, and the tracks of that scope are appended in catalogue order. The same dialog lets you create a playlist when you have none yet.
- **Edit the name and description**, and **delete the playlist** when you no longer need it. Deleting a playlist never deletes any track.

The playlist reflects the catalogue at the moment you open it. If a track is deleted from a library after it was added to a playlist, its row stays in the playlist as an unavailable track so you can remove it deliberately. Long playlists are shown in pages; select **Load more** to show the next page.

## Favorites

A favorite is a mark you place on an artist, album, track or book so you can find it again quickly. Favorites belong to your account and are not shared with other users. Marking an item never changes the item or its files. Videos and photos are not available as favorites yet.

Select the heart on an item's card, on its row on an album page or in a playlist, on the player bar for the current track, or on any queue entry of the player, to add or remove the mark. A blue heart means the item is not a favorite; a red heart means it is.

The **Favorites** page in the library section lists everything you marked, most recent first, with a count that follows the filter.

- **Filter by kind** with the **Show** picker.
- **Open an item** from its card.
- **Remove a favorite** with the heart on its card.
- **Load more** when the list is long, or **Retry** if it does not load.

A favorite whose item was deleted, or whose library you can no longer access, stays in the list as an unavailable card so you can still remove it.

## Statistics

The **Statistics** entry under Library opens your consumption statistics. It covers every media kind the server can consume, with music and book sessions appearing side by side and more kinds joining as their catalogues arrive. The statistics are built from the sessions your player and reader report, belong to your account, and are not shared with other regular users; administrators can view any user's statistics.

A session counts towards the statistics when it completes, or when it reaches the counting rule of its kind. A music track counts after 30 seconds, and a book counts after more than 5 consecutive pages in the reader. A stopped session below the rule is stored but not counted, while the total consumed time still includes the time you spent. Completion is separate from counting: a session can count without the library considering the item complete.

The page shows:

- **Total consumed time**, **counted sessions**, **completed sessions** and the **completion rate** of the selected user.
- **By media kind**, the same totals broken down per kind.
- **Genres** and **tags** as bars, grouped across media kinds and shown lowercase. A genre or tag shared by a book and a track appears once.
- **Recently consumed** and **most consumed**, with the kind of each item, its title, creator, collection, cover, last consumption time, counted sessions, completed sessions and consumed time.
- **Filters** for the library, the media kind and a date range, applied to every section.

Each history row has **Delete from history**, which removes every recorded session of that item. The page also has **Clear history**, which removes every recorded session of the selected user, and **Remove from continue watching**, which removes every saved resume point of the selected user. Each action asks for confirmation, and the page reloads after it. Deleting history does not remove resume points, and removing resume points does not delete history. The resume points themselves are the saved positions described under reading and listening; the statistics page does not list them, it only clears them.

An administrator sees a user picker with **Me**, every user, and **All users**. Selecting another user shows that user's statistics. Selecting **All users** shows the combined totals and a per-user table, because the history lists and the genre and tag breakdown are only meaningful per user. A regular user sees no picker and only their own statistics. Administrators can read and delete the history of any user; regular users can only read and delete their own.

An item that was deleted, or whose library you can no longer access, keeps its place in the counts but is shown as unavailable, without display details. Select **Retry** when a section does not load.

## Editing what is catalogued

Both book and music pages double as editors for the items that were scanned in. A book's details page groups its fields into general and release information, language and original language, genres, tags, identifiers, ISBNs, contributors and ratings, and lets you replace its cover. An artist's page lists the artist's albums first, then groups its fields into general information, contributors, ratings, genres, tags, aliases, areas and identifiers, and lets you replace the artist's cover. An album page groups its fields into general information, contributors, ratings, genres, tags, release types, release information, language and original language, and identifiers, and lets you replace the album's cover. A track page groups its fields into general information, contributors, ratings, genres, tags, moods, release information, language and original language, work, ISRCs, identifiers and audio properties. Saving applies the changes to Lumina's catalogue.

## The file system browser

Wherever Lumina needs a folder or a file from the server, it opens the file system browser: a modal that lists the drives, a tree of folders, a pane of the items in the current folder, and an address bar you can walk with. It offers icon view modes, generates thumbnails for images, and can show or hide hidden files. You will meet it when you pick the content locations of a library, and anywhere else the interface asks you to point at a path on the server.

## Settings and language

The tools section holds your settings and the language picker.

Your **settings** decide how lists and books behave for you. Enable pagination and the number of items per page control how browsing is split. The alpha picker option decides whether titles beginning with "The" are filed under the next word. Theme caching decides whether the theme is fetched fresh each time. Rendering PDFs as images and preserving book styles shape how books are shown, and the metadata and artwork aggregation options decide whether missing fields are completed from your other providers. A final group of options shapes playback: whether the player keeps downloading while it is paused (for a track the server has to normalize, this keeps its bounded one-segment lookahead filled rather than downloading the whole file), whether it continues automatically at the end of an album or artist, whether consecutive tracks play without a gap, how ReplayGain normalizes loudness, and the quality cap used when the server has to normalize a track. That cap applies only when the server re-encodes a track into a lossy format; a remux only repackages the stored audio, and a lossless delivery is not reduced. The ones that touch libraries are described in more detail in [Managing Libraries](../managing-libraries/index.md).

The **VU meter** group shapes the meter of the player bar. Orientation lays its channels out as columns that grow upwards or rows that grow to the right. Segments per channel sets how finely each channel is divided, and Segment gap sets the space between segments and channels. The medium and high thresholds decide the level at which a segment changes color; the high threshold cannot be lower than the medium one. Show peak keeps a mark at the loudest recent level of each channel. Peak hold is how long that mark stays in place before it falls, in milliseconds, and Level falloff and Peak falloff are how quickly the bars and the peak marks drop once the sound gets quieter, in level units per second. Larger falloff values drop faster. The meter follows the level of the track itself, so muting the player or moving the volume slider does not change the height of the bars, and saving these settings applies them to the player without reloading the page.

The **language** picker switches the interface between the languages Lumina ships with, and remembers your choice.
