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

## Browsing your music

Opening a music library shows a page with the library title, a search box, a sort toggle, a count of items, a grid of artist cards, an alphabetical picker down the side, and pagination at the bottom.

- **Search** filters the artists by name; press Enter or use the search button.
- **Sort** toggles between ascending and descending order.
- **The alpha picker** jumps to names starting with a chosen character. It offers All, then the number and symbol keys, then A to Z. You can also press a letter key to filter, and press Escape to clear the filter.
- **Pagination** splits the list into pages when you have it enabled, with previous and next buttons and a page indicator. When it is off, the whole list is shown and the count is the total.

Each artist is a card; selecting it opens the artist's page, and the card's actions menu offers **View details** and **Delete**.

The artist page is also where you edit the artist. It lists the artist's albums as cards, each leading to its own album page, followed by the artist's general information, contributors, ratings, genres, tags, aliases, areas and identifiers. The album page holds the album's information and its tracks, and the track page holds the track's details. The album page also shows the artwork Lumina stored for the album as a strip of thumbnails beside the main image, and selecting a thumbnail displays that image in place of the cover. Deleting an artist, album or track removes it from Lumina's catalogue; your files are not touched.

## Editing what is catalogued

Both book and music pages double as editors for the items that were scanned in. A book's details page groups its fields into general and release information, language and original language, genres, tags, identifiers, ISBNs, contributors and ratings, and lets you replace its cover. An artist's page lists the artist's albums first, then groups its fields into general information, contributors, ratings, genres, tags, aliases, areas and identifiers, and lets you replace the artist's cover. An album page groups its fields into general information, contributors, ratings, genres, tags, release types, release information, language and original language, and identifiers, and lets you replace the album's cover. A track page groups its fields into general information, contributors, ratings, genres, tags, moods, release information, language and original language, work, ISRCs, identifiers and audio properties. Saving sends the changes back to the server, and a confirmation or an error is shown in the corner of the screen.

Changes you make here live in Lumina's catalogue.

## The file system browser

Wherever Lumina needs a folder or a file from the server, it opens the file system browser: a modal that lists the drives, a tree of folders, a pane of the items in the current folder, and an address bar you can walk with. It offers icon view modes, generates thumbnails for images, and can show or hide hidden files. You will meet it when you pick the content locations of a library, and anywhere else the interface asks you to point at a path on the server.

## Settings and language

The tools section holds your settings and the language picker.

Your **settings** decide how lists and books behave for you. Enable pagination and the number of items per page control how browsing is split. The alpha picker option decides whether titles beginning with "The" are filed under the next word. Theme caching decides whether the theme is fetched fresh each time. Rendering PDFs as images and preserving book styles shape how books are shown, and the metadata and artwork aggregation options decide whether missing fields are completed from your other providers. A final group of options shapes playback: whether the player keeps downloading while it is paused, whether it continues automatically at the end of an album or artist, whether consecutive tracks play without a gap, how ReplayGain normalizes loudness, and the quality cap used when the server has to normalize a track. The ones that touch libraries are described in more detail in [Managing Libraries](../managing-libraries/index.md).

The **language** picker switches the interface between the languages Lumina ships with, and remembers your choice.
