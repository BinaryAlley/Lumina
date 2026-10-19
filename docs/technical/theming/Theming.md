# Lumina Theming

The web client of Lumina is a themed shell: every page is rendered inside the templates of an active theme. A theme is a self contained ZIP archive with a manifest, HTML templates and static assets. This document explains how themes are structured, how the template engine works, and how to create, install and manage a theme.

- [Lumina Theming](#lumina-theming)
  - [Overview](#overview)
  - [Theme Pack Structure](#theme-pack-structure)
  - [The Manifest (theme.json)](#the-manifest-themejson)
  - [Pack Validation Rules](#pack-validation-rules)
  - [Templates](#templates)
    - [Template Syntax](#template-syntax)
    - [Expressions](#expressions)
    - [Safety Limits](#safety-limits)
    - [Template Resolution](#template-resolution)
    - [The Layout Template (shared/layout)](#the-layout-template-sharedlayout)
    - [The Audio Player Bar](#the-audio-player-bar)
      - [Play Affordances on the Music Pages](#play-affordances-on-the-music-pages)
      - [The audio player sub templates](#the-audio-player-sub-templates)
    - [The Navigation Template (shared/nav-menu)](#the-navigation-template-sharednav-menu)
    - [Page Templates](#page-templates)
    - [The Scripts Section](#the-scripts-section)
  - [Assets](#assets)
  - [Configuration Options](#configuration-options)
  - [Creating a Theme](#creating-a-theme)
  - [Installing a Theme](#installing-a-theme)
  - [Bundled Themes](#bundled-themes)
  - [Selecting the Current Theme](#selecting-the-current-theme)
  - [Managing Themes from the Administration Page](#managing-themes-from-the-administration-page)
  - [Security Notes](#security-notes)

## Overview

The application stores installed themes on the API server and serves their templates and assets. The web client fetches the templates of the active theme and renders them locally with a small, non-executable Mustache-style template engine. Theme templates can interpolate values, iterate collections and test truthiness, but they cannot run code.

Because the templates are HTML, a theme can restyle the whole application, rearrange the layout and navigation, and provide custom pages for the built-in routes. A theme cannot extend the functionality of the application: it only shapes how the existing pages look and are structured.

## Theme Pack Structure

A theme pack is a directory that is shipped as a ZIP archive with `theme.json` at its root:

```
my-theme/
  theme.json
  assets/
    preview.svg
    site.css
  templates/
    index.html
    shared/
      layout.html
      nav-menu.html
      not-found.html
    library/
      written-content-library/
        book-library/
          books/
            index.html
```

Only three locations may exist inside a pack:

- `theme.json`, the manifest, at the root of the archive.
- `templates/`, holding the HTML templates.
- `assets/`, holding any static file served to the browser (stylesheets, images, fonts, scripts).

The bundled reference theme `editorial-paper` follows exactly this layout and is the best starting point when authoring a new theme.

## The Manifest (theme.json)

The manifest is a JSON object describing the theme. The example below is the manifest of the bundled `editorial-paper` theme:

```json
{
  "schemaVersion": 1,
  "id": "editorial-paper",
  "name": "Editorial Paper",
  "description": "A clean, readable proof of concept theme.",
  "author": "Lumina",
  "version": "1.4.0",
  "preview": "assets/preview.svg",
  "templates": {
    "default": "templates/index.html"
  }
}
```

| Field | Required | Rule |
| --- | --- | --- |
| `schemaVersion` | yes | Must be `1`. |
| `id` | yes | A lowercase kebab-case value, up to 64 characters, used to reference the theme. For example `editorial-paper` or `my-dark-theme`. |
| `name` | yes | Display name, up to 80 characters. |
| `description` | yes | Description of the theme, up to 300 characters. |
| `author` | yes | Author of the theme, up to 80 characters. |
| `version` | yes | Semantic version form, for example `1.0.0`. Up to 40 characters. |
| `preview` | no | Path to a preview image, relative to the pack root and under `assets/`. Shown on the administration page. |
| `templates` | no | An optional mapping from template key to a file under `templates/`, with up to 32 entries. Templates can also be provided as mirrored files under `templates/` without a manifest entry. |

All text fields must be non-empty and must not contain control characters. Template keys follow the same lowercase kebab-case rule as the theme id and are at most 40 characters. Template paths are normalized and must point to an existing file under `templates/`.

## Pack Validation Rules

A theme archive is validated on install:

- `theme.json` must exist at the root of the archive and be at most 64 KB.
- Only `theme.json`, files under `templates/` and files under `assets/` are allowed. Anything else makes the archive invalid.
- Paths are normalized and any path that would escape the pack is rejected.
- `theme.json` must contain a valid manifest object that satisfies the rules in the previous section.

Installing a theme is an atomic operation: an invalid pack is rejected entirely and nothing is left behind.

## Templates

Templates are rendered with the built-in template engine. The engine is intentionally small and safe: it parses Mustache-style tags and renders them against a model. It never executes arbitrary code.

### Template Syntax

| Syntax | Meaning |
| --- | --- |
| `{{name}}` | Outputs the value of `name`, HTML-escaped. |
| `{{{name}}}` | Outputs the value of `name` without escaping. |
| `{{&name}}` | Outputs the value of `name` without escaping. |
| `{{!comment}}` | A comment, produces no output. |
| `{{#name}}...{{/name}}` | A section. Renders its children when the value is truthy. When the value is a collection, the children are rendered once per item. |
| `{{^name}}...{{/name}}` | An inverted section. Renders its children when the value is falsy. |

### Expressions

Expressions resolve against the model and support:

- Dotted property paths, for example `{{user.name}}`. Dictionary keys and public properties are matched case-insensitively.
- Parent traversal, for example `{{../title}}`, to reach values outside the current section scope.
- The current value, for example `{{.}}`.
- `{{#count}}` on a collection, where `Count` is exposed for collections.

Values that do not exist resolve to an empty string for variables, or to falsy for sections. Truthiness follows these rules: `null`, `false`, empty strings and empty collections are falsy; everything else is truthy.

### Safety Limits

The template engine enforces hard limits so a single theme cannot exhaust the server:

| Limit | Value |
| --- | --- |
| Maximum section nesting depth | 32 levels |
| Maximum expression length | 120 characters |
| Maximum rendered output | 4 MB per page |

A template that exceeds any of these limits is rejected with a template error and the page falls back to the application layout.

### Template Resolution

Every themed page is identified by a page key that mirrors the path of the corresponding Razor view. When a page is rendered, its template is selected in this order:

1. An explicit mapping in `templates` when the manifest declares the page key.
2. A mirrored template file at `templates/{pageKey}.html`, walking up the scope of the path when the exact mirror does not exist.

When neither an explicit mapping nor a mirrored file exists, the theme has no template for that page, and the application renders its own fallback unstylied Razor view instead.

For example, the books index page has the page key `library/written-content-library/book-library/books/index`. The engine tries, in order: the explicit `templates` entry for that key, `templates/library/written-content-library/book-library/books/index.html`, then the parent scopes such as `templates/library/written-content-library/book-library/books.html`. When none exists, the application renders the books page view itself.

Known page keys:

| Page key | Purpose |
| --- | --- |
| `shared/layout` | The layout that wraps the shell of every page. |
| `shared/nav-menu` | The navigation menu of the theme. |
| `shared/not-found` | The 404 page. |
| `shared/audio-player` | The audio player bar shell. |
| `shared/audio-player/queue-item` | The queue item of the audio player bar. |
| `shared/audio-player/playlist-item` | The playlist item of the audio player bar. |
| `library/written-content-library/book-library/books/index` | The books browsing page. |
| `shared/file-system-browser` | The file system browser component shell. |
| `shared/file-system-browser/tree-node` | The tree view node of the file system browser. |
| `shared/file-system-browser/explorer-item` | The file system entry of the file system browser. |
| `shared/file-system-browser/path-segment` | The address bar path segment of the file system browser. |

### The Layout Template (shared/layout)

Every page is rendered inside the layout template. The layout receives a model with the following fields:

| Field | Content |
| --- | --- |
| `title` | The title of the page. |
| `assetBase` | The base URL of the theme assets, for example `/theme-assets/editorial-paper/assets`. |
| `appHead` | The head links of the application, such as its global stylesheets. |
| `nav` | The fully rendered navigation menu HTML. |
| `content` | The fully rendered content section of the page. |
| `audioPlayer` | The rendered audio player when the user is authenticated, empty otherwise. |
| `appScripts` | The global scripts of the application. |
| `scripts` | The rendered script section of the page. |
| `mainStyle` | An inline style for the main content area, `bottom: 0px;` when the user is not authenticated and empty otherwise. |

The layout template of the bundled theme shows the intended shape:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{{title}} - Lumina</title>
    {{&appHead}}
    <link rel="stylesheet" href="{{assetBase}}/site.css">
</head>
<body>
    <div id="page">
        <header>
            {{&nav}}
        </header>
        <main role="main" style="{{mainStyle}}">
            <article class="h-100">
                {{&content}}
                <div id="modal-background-article"></div>
                <div class="dialog-container"></div>
            </article>
        </main>
        {{#audioPlayer}}
        {{&audioPlayer}}
        {{/audioPlayer}}
    </div>
    <div id="modal-background-menu"></div>
    {{&appScripts}}
    <div data-section="scripts">
        {{&scripts}}
    </div>
</body>
</html>
```

The rendered page content, navigation and scripts are inserted unescaped with `{{&...}}`, because they are trusted application HTML. The layout is best effort: when the theme cannot be loaded or rendered, the application falls back to its built-in layout, so a broken theme never takes the whole site down.

### The Audio Player Bar

Every authenticated page renders the audio player bar through the `audioPlayer` field of the layout model. The bar starts hidden, and the global audio player script shows it when playback starts, expands it and hides it again. The markup comes from the `shared/audio-player` template of the active theme; when the theme does not provide that template, the application renders its own fallback markup, so the player always has a bar to bind to.

The theme owns the position, orientation and size of the bar, and it owns the room reserved for it on the main content element. The application fallback happens to fix the bar to the bottom, but that is only the fallback layout: a theme can make it a vertical bar on a side, a horizontal bar at the bottom, or any other placement, and can expand its queue in any direction. A layout must keep these element hooks, which the audio player drives (the id control is bound by the global site script, which routes it to the audio player):

| Element | Hook | Meaning |
| --- | --- | --- |
| The bar container, with the `audio-player-container` class | `not-shown` class | The bar is hidden. The theme decides how that state is presented and which space it releases. |
| The bar container | `expanded` class | The bar is expanded. This is the layout-neutral state a theme styles for its own layout. |
| The bar container | `inert` attribute | Set while the bar is hidden and removed when it is shown, so a collapsed bar keeps none of its focusable controls in the tab order. |
| The expanded area, with the `audio-player-expanded` class | Layout child of the bar container | The cover art, now playing details and queue of the expanded bar. Its position is theme-owned. The fallback stylesheet places it above the bar in a flex column, but the player does not require that layout. A theme that animates the area should keep it in the DOM while the transition runs. |
| The condensed bar, with the `audio-player-bar` class | Layout child of the bar container | The row of controls. Its position is theme-owned; the fallback stylesheet places it at the bottom edge of the container. |
| The main content element | `audio-player-not-shown` class | The bar is hidden, so the content area releases the room the theme reserved for the bar. |
| The main content element | `audio-player-bar-expanded` class | The bar is expanded. This is the layout-neutral state for the main content element's own adjustment. |
| The expand and collapse control | `id="audio-player-expand-toggle"` | A focusable control, such as a `button`, that expands or collapses the bar. Its label may describe the action in the theme's own layout. |

The `shared/audio-player` template receives the following model:

| Field | Content |
| --- | --- |
| `appBase` | The absolute base URL of the Web application, used to reference the application scripts. |
| `assetBase` | The base URL of the theme assets, used by the theme template to reference its own assets. |
| `queueItemTemplate` | The raw source of the `shared/audio-player/queue-item` sub template, inlined by the application into a plain text script element. |
| `playlistItemTemplate` | The raw source of the `shared/audio-player/playlist-item` sub template, inlined by the application into a plain text script element. |
| `strings` | The localized strings of the bar, keyed by their resource names. A shell template uses `toggleExpanded`, `previous`, `next`, `stop`, `repeat`, `repeatOff`, `repeatAll`, `repeatOne`, `queue` and `albumArtwork` for its controls; the player reads the play, pause, mute, unmute, volume, seek and buffering labels from the global scripts configuration instead, so a themed shell does not need to render them. |

The bundled themes reserve the room for their bottom bar like this. It is an example of a bottom layout, not a required layout:

```css
main {
    bottom: 50px;
    transition: bottom 1s ease, height 1s ease;
}

main.audio-player-not-shown {
    bottom: 0;
}

main.audio-player-bar-expanded {
    bottom: calc(100% - 22px); /* expanded height, less the menu height */
}
```

A theme with a vertical bar instead reserves a side column, a theme with a bar on top reserves top space, and a theme whose expanded bar overlays the content reserves no space at all. The application fallback layout reserves the room itself, because the application stylesheet does not position the content area of a theme. A theme that omits its own space reservation leaves its bar overlapping its content.

The flex column used by the application fallback is also only a default. The player toggles the state classes and never reads the geometry of the bar, so a theme can lay the container out in any direction, place the expanded area to the left, right, above or below the condensed bar, and make the queue scroll vertically or horizontally. The player's only layout-dependent behavior is scrolling the `data-audio-player-queue` element so the current entry aligns to its start on both axes.

A theme that restyles the fallback transition can set `--audio-player-expanded-duration` on `.audio-player-container` to keep its height, fade and padding transitions on the same duration. The bundled themes set it to `1s`, matching their own height transition; the application fallback uses `0.4s`. A theme that uses a different transition can ignore the property.

The markup inside the bar exposes the hook attributes the audio player script binds. A theme that replaces the bar markup must keep all of them; the part of the bar whose hook is missing stays inert. The deprecated `queuechange` event keeps its ordered descriptor array payload for older theme scripts; new code must use `queuestatechange`, whose payload is the compact queue summary.

| Hook attribute | Meaning |
| --- | --- |
| `data-audio-player-play-pause` | The play and pause control. The player adds the `is-playing` class to it while a track plays. |
| `data-audio-player-stop` | The stop control. Clicking it stops playback; a condensed bar then slides out of view, while an expanded bar stays on screen. |
| `data-audio-player-previous` | The previous control. It either returns to the previous queue entry or restarts the current track, following the navigation history. |
| `data-audio-player-next` | The next control. It starts the next queue entry and loads another shuffle or autoplay batch when the queue is at its end. The player sets `disabled` and the `is-disabled` class when there is no next entry. |
| `data-audio-player-repeat` | The repeat toggle. The player writes `data-audio-player-repeat-mode` (`off`, `all` or `one`), toggles `aria-pressed`, the `is-active` class and, for one, the `is-one` class. |
| `data-audio-player-progress` | The progress bar, a `role="slider"` element with `tabindex="0"` that the player scrubs and whose ARIA value attributes it writes. It carries `data-audio-player-progress-state` (`idle`, `ready` or `buffering`) and the `is-buffering` class while it loads. |
| `data-audio-player-progress-played` | The played layer of the bar. The player publishes its start and width as the custom properties `--audio-player-progress-played-start` and `--audio-player-progress-played-width`. |
| `data-audio-player-progress-downloaded` | The downloaded but not played layer. `--audio-player-progress-downloaded-start` and `--audio-player-progress-downloaded-width`. |
| `data-audio-player-progress-undownloaded` | The not yet downloaded layer. `--audio-player-progress-undownloaded-start` and `--audio-player-progress-undownloaded-width`. |
| `data-audio-player-current-time` | The current position, written as `m:ss`. |
| `data-audio-player-total-time` | The duration, written as `m:ss`, or `--:--` while it is unknown. |
| `data-audio-player-track-name` | The track name. |
| `data-audio-player-album-name` | The album name, shown underneath the track name. The player leaves it empty when the descriptor has no album title, including when the title is the stored `Unknown Album` placeholder, so a track without an album shows only its name. |
| `data-audio-player-now-playing-title` | The current track name of the expanded player, under its cover. |
| `data-audio-player-now-playing-release-type` | The release type of the current track's album, such as Album, Live or EP. Multiple types are joined with commas. It is left empty when the album has no release type. |
| `data-audio-player-now-playing-release` | The release name of the current track's album in the expanded player. It is left empty when the album has no displayable title. |
| `data-audio-player-now-playing-year` | The release year of the current track's album in the expanded player. It is left empty when the year is not known. |
| `data-audio-player-now-playing-artist` | The artist name of the current track's album in the expanded player. It is left empty until the album details are available. |
| `data-audio-player-artwork-carousel` | The carousel of the stored artwork of the current track's album. The player appends one `button.artwork-carousel-item` per stored image, carrying `data-artwork-path`, and toggles the `selected` class and `aria-pressed` on the chosen item. The element is hidden with the `hidden` attribute while the album has no stored artwork. |
| `data-audio-player-cover-thumbnail`, `data-audio-player-cover-image` | The cover art of the condensed bar and of the expanded bar. Choosing an album artwork in the carousel replaces both images until the next track. |
| `data-audio-player-volume` | The volume slider, a `role="slider"` element with `tabindex="0"` that the player scrubs and whose ARIA value attributes it writes. The fill is published as `--audio-player-volume-start` and `--audio-player-volume-width`. |
| `data-audio-player-volume-level` | The volume fill. It carries no geometry of its own: the stylesheet drives it from the custom properties above. |
| `data-audio-player-mute` | The mute toggle. The player toggles the `is-muted` class and the `aria-pressed` state on it. |
| `data-audio-player-queue` | The queue list of the expanded bar. The player writes `data-audio-player-queue-entry-id` on every rendered item root, whether it comes from the theme sub template or from the built-in markup, and makes the root focusable, so clicking an item or pressing Enter or Space on it starts that entry. The element must stay the scroll container of the list, because the player scrolls the current entry to the start of whichever axis the theme makes scrollable when playback advances or the bar expands. |

Each progress layer also carries `data-audio-player-progress-layer` (`played`, `downloaded` or `undownloaded`) in the markup, so a theme can select a layer by its state as well as by its class. The player writes no inline geometry on any of the bar elements: it publishes the geometry of each state as the custom properties above, which the application stylesheet consumes, so a theme restyles or reorients either bar by consuming the same properties.

#### Play Affordances on the Music Pages

The artist page, the album page and the track page render play triggers, and the card actions menus render play and shuffle triggers, all handled by the global audio player script through event delegation. The music library cards on the home page, the artist cards and the album cards offer both actions, so the whole library, an artist or a release can be started in order or shuffled. A theme template that replaces one of those pages or menus opts in by rendering the same trigger attributes: the player binds one listener on the document, so a trigger works anywhere in the swapped page content and the page needs no script of its own.

The trigger attributes are:

| Attribute | Meaning |
| --- | --- |
| `data-audio-player-play` | Marks the element as a play trigger. Clicking it replaces the queue with the described context and starts playback. |
| `data-audio-player-play-next` | Marks the element as a play next trigger. While something is current, clicking it inserts the described track or context after the current entry without starting playback and discards the pending autoplay continuation. When nothing is current, the player starts the described context instead, because there is no entry to queue after. |
| `data-audio-player-play-shuffled` | Marks the element as a shuffled play trigger. Clicking it starts the described context and turns shuffle on for the scope in `data-audio-player-shuffle-scope`. |
| `data-audio-player-play-library` | Marks the element as a library play trigger. Clicking it starts the whole library named by `data-library-id` in artist page order, without needing a track, album or artist context. |
| `data-audio-player-play-library-shuffled` | Marks the element as a library shuffle trigger. Clicking it starts a shuffled playback of the whole library named by `data-library-id`, without needing a track, album or artist context. |
| `data-audio-player-shuffle-scope` | The scope of a shuffled play trigger: `release`, `artist` or `library`. A shuffled play trigger without this attribute shuffles the release it started. |

All triggers read the play descriptor from the same `data-*` attributes:

| Attribute | Meaning |
| --- | --- |
| `data-library-id` | The Id of the media library the context belongs to. Required by every trigger. |
| `data-track-id` | The Id of the track. Required by a track trigger. |
| `data-album-id` | The Id of the album. Required by a track trigger and by an album context trigger. |
| `data-artist-id` | The Id of the artist. Required by a track trigger, an album context trigger and an artist context trigger. |
| `data-title` | The display title of the track or context, shown in the bar while the context loads. Optional. |
| `data-album-title` | The display title of the album the track belongs to, shown under the track name in the bar. Optional, and meaningful on a track trigger. An album context already uses its `data-title` as the album title while the context loads, so it does not need this attribute. |
| `data-duration` | The duration of the track in seconds. Optional, and meaningful on a track trigger. |
| `data-cover-art-path` | The artwork path of the cover, relative to the API server base, exactly like the artwork path the album page uses. Optional. |
| `data-cover-art-url` | An absolute cover art URL, or a relative artwork path. Optional, and used in preference to `data-cover-art-path` when both are present. |

The context a trigger describes decides the required parameters and the queue it builds:

| Trigger | Required attributes | Queue |
| --- | --- | --- |
| Track | `data-library-id`, `data-track-id`, `data-album-id`, `data-artist-id` | The single track. |
| Album | `data-library-id`, `data-album-id`, `data-artist-id` | The tracks of the release, in disc and then track number order. |
| Artist | `data-library-id`, `data-artist-id` | Every release of the artist in the order the artist page presents them, each in disc and track number order. |
| Library | `data-library-id` | Every track of the first artist of the library that has tracks, in artist page order, followed by the remaining artists in the same order. |

The album queue is read through `api-get-tracks`, the artist queue through `api-get-albums` and then `api-get-tracks`, and a track trigger uses the descriptor alone. A library trigger reads `api-get-artists` to find the first artist that has tracks and then builds that artist's queue; when playback reaches the end, the continuation walks the remaining artists in library order. The queue script documents these endpoints and their order in full.

An album or artist trigger carries its effective cover into the entries it queues. A `data-cover-art-url` is kept as the resolved cover URL of the context, so a theme that supplies only that attribute does not lose the cover when the queue is built. An album trigger falls back to the context cover for a release without one. An artist trigger falls back to the artist cover, and a library trigger falls back to the cover of the artist the release belongs to, so a release with its own cover always keeps that cover. The `data-cover-art-path` is kept on every entry for artwork matching, even when the trigger supplies a cover URL.

Each page exposes the values its own triggers need. The album page provides `artistId`, `albumId`, `libraryId`, `albumTitle`, `albumCoverPath` and `albumHasTracks` for the album trigger. Its per-track triggers are rendered from the track rows the page loads from `tracksUrl`, which returns the `id`, `title`, `trackNumber`, `discNumber` and `durationInSeconds` of each track; `albumJson` carries the album model the template script starts from, and `trackUrl` is the template of a track page route. The artist page provides `artistId`, `libraryId`, `artistName`, `artistCoverPath` and `artistHasReleases` for the play all trigger, and `albumsUrl` and `albumUrl` for the release list and its cards. The release rows returned by `albumsUrl` carry the `id`, `title` and `coverPath` used by the album card Play and Shuffle actions. The track page provides `artistId`, `albumId`, `trackId`, `libraryId`, `trackTitle`, `trackDurationInSeconds`, `albumTitle` and `albumCoverPath` for its trigger. The artist index page provides `libraryId` and `itemsUrl` for its artist cards, and each row of `itemsUrl` carries the `id`, `name` and `coverPath` used by the artist card Play and Shuffle actions. The home page library cards are application markup, and each music library card uses the library `id` for its Play and Shuffle actions. A theme template that changes these pages uses the same values and endpoints to render equivalent triggers.

The application fallback templates render the triggers on the existing `confirm-button` controls: the page-level controls carry their localized label, and the per-track control carries a text play glyph with an accessible label. The card actions menus of music libraries, artists and albums render Play and Shuffle; the album card menu also keeps Remove beside them. A theme may use any markup and classes; only the trigger and descriptor attributes are required, and a trigger must be a focusable element, such as a `button`. The bundled themes render these triggers on the pages and card menus they provide, so a custom theme that replaces one of those templates must render them again to keep playback and shuffle reachable from that page.

#### The audio player sub templates

Like the file system browser, the audio player is split into a shell template and the sub templates of its dynamic content, all provided as mirrored files under `templates/shared/audio-player/`. The shell template inlines the raw source of each sub template into a plain text script element, so that the browser never parses the theme markup into live elements and requests their resources before the player renders them:

```html
<script type="text/plain" id="audio-player-template-queue-item">{{{queueItemTemplate}}}</script>
<script type="text/plain" id="audio-player-template-playlist-item">{{{playlistItemTemplate}}}</script>
```

The sub templates are rendered client side by the audio player script with the same Mustache syntax as the server side engine. They receive the following fields:

| Template | Fields |
| --- | --- |
| `queue-item` | `entryId` (the identity of this occurrence in the queue), `index` (the one based position in the queue), `trackId`, `albumId`, `artistId`, `libraryId`, `title`, `duration` (the formatted `m:ss` duration), `durationSeconds`, `coverArtUrl` (the resolved cover art URL, which is the music placeholder when the entry has no artwork), `isCurrent`, `isPlayed`, `isLast`, `isContinuation` (the entry was added by the autoplay continuation) and `isShuffled` (the entry was added by a shuffle batch). The item root must keep the `audio-player-queue-item` class, the stylesheet hook of a queue item: the player appends the rendered element as-is, so the class is what lets the theme style it. The player writes `is-current`, `is-played`, `is-continuation` and `is-shuffled` on the root itself, so a theme item does not need to render those state classes, and it writes `data-audio-player-queue-entry-id`, so a theme item does not need to render the identity either. |
| `playlist-item` | `index`, `trackId`, `albumId`, `artistId`, `libraryId`, `title`, `duration`, `durationSeconds`, `coverArtUrl` and `isCurrent`. The item root must keep the `audio-player-playlist-item` class, the stylesheet hook of a playlist item. Saved playlists do not exist yet, so the shell inlines this sub template for theme authors, but nothing renders it yet; the playlist feature renders the entries of a playlist from it. |

A theme can omit either sub template: the player then renders that part with its own built-in markup. When a theme replaces the shell, it must keep the two plain text script element ids above, the element hooks of the bar, and the `audio-player-container`, `not-shown` and `expanded` classes the player toggles.

Every field of the `queue-item` and `playlist-item` models is untrusted: titles and identifiers come from file metadata and the music endpoints. Render these fields with the escaped form, `{{field}}`. The template engine also supports the unescaped form `{{{field}}}`, but these models contain no trusted HTML, so unescaped tags must not be used for them. Escaping does not neutralise whitespace in an unquoted attribute, so every model value placed in an attribute must sit inside quoted attribute marks, for example `data-track-id="{{trackId}}"` and never `data-track-id={{trackId}}`.

### The Navigation Template (shared/nav-menu)

The navigation template renders the menu of the active theme. Its model contains the site name and two menu structures, one for the mobile menu and one for the desktop menu bar:

- `siteName` is the site name.
- `mobileSections` and `menubarSections` are lists of sections.
- A section has a `label` and a list of `items`.
- An item has a `label`, an optional `url`, an optional `cssClass` and a list of `children`. Items with children are submenus, items without children are plain links.

The navigation template is also best effort: when its template cannot be rendered, the application menu is used instead.

### The File System Browser Templates (shared/file-system-browser)

The file system browser is a modal component hosted by the library editing page. Like the navigation menu, it is rendered from theme templates and is best effort: when its component template cannot be loaded, the application renders its own unstyled fallback.

The component is split into a shell template and three sub templates, all provided as mirrored files under `templates/shared/file-system-browser/`. Because the sub template page keys live under the shell page key, the engine would resolve a missing sub template to the shell template by walking up the path scopes; the application detects this fallback and treats it as a missing sub template, so a theme can omit any sub template and keep the built-in rendering of that part.

The shell template receives a model with the following fields:

| Field | Content |
| --- | --- |
| `appBase` | The absolute base URL of the Web application, used to reference the application scripts, for example `{{appBase}}js/file-system-browser-dialog.js`. |
| `assetBase` | The base URL of the theme assets. |
| `iconBaseUrl` | The base URL of the file type icons of the theme, derived from `assetBase`. |
| `fileIconsUrl` | The URL of the file icons mapping of the theme, derived from `assetBase`. |
| `treeNodeTemplate` | The raw source of the `shared/file-system-browser/tree-node` template, inlined by the application into a plain text script element. |
| `explorerItemTemplate` | The raw source of the `shared/file-system-browser/explorer-item` template, inlined by the application into a plain text script element. |
| `pathSegmentTemplate` | The raw source of the `shared/file-system-browser/path-segment` template, inlined by the application into a plain text script element. |
| `strings` | The localized strings of the component, keyed by their resource names. |

The shell template owns the whole dialog markup: the header with the view mode, navigation and address bar controls, the tree view and explorer panes and the action bar. It must keep the DOM contract the application script binds to (the element ids such as `file-system-browser-dialog`, `navigator-*`, `file-system-browser-treeview`, `file-system-browser-file-system-explorer` and the two selection rectangles), and it must include the application component scripts with their `data-component-script` attributes:

```html
<script type="text/plain" id="fsb-template-tree-node">{{{treeNodeTemplate}}}</script>
<script type="text/plain" id="fsb-template-explorer-item">{{{explorerItemTemplate}}}</script>
<script type="text/plain" id="fsb-template-path-segment">{{{pathSegmentTemplate}}}</script>
<script defer src="{{appBase}}js/theme-client-template-engine.js" data-component-script="file-system-browser"></script>
<script defer src="{{appBase}}js/file-system-browser-dialog.js" data-component-script="file-system-browser"></script>
```

The sub template sources are inlined into plain text script elements, whose content the browser never parses into elements, so the theme markup cannot be loaded or requested before the application script renders it. The application script reads the raw source from the script element text and renders it for each dynamic entry.

The application appends the initialization script of the component after the rendered shell, so the theme does not author it.

The sub templates are rendered client side by the application script for each dynamic entry, using the same Mustache syntax as the server side engine. They receive the following fields:

| Template | Fields |
| --- | --- |
| `tree-node` | `path`, `name`, `itemType` (`Root`, `Directory` or `File`), `isRoot`, `isDirectory`, `isFile`, `isExpanded` and `icon`. The node must keep the `.tree-node` root with its `data-path`, the `.node-content`, `.expand-button` (or `.spacer` for files), `.node-icon` and `.node-name` elements, and the `.child-nodes` container. |
| `explorer-item` | `path`, `name`, `itemType` (`Directory` or `File`), `isDirectory`, `isFile`, `cssClass` (the active view mode, such as `large-icons`) and `iconUrl`. The item must keep the `.e` root with its `data-path` and `data-type`, and an `<img>` element, which the script replaces with thumbnails for image files. |
| `path-segment` | `id`, `path` (the segment name), `dataPath` (the full path up to and including the segment), `toggleId` and `dropdownId`. The segment must keep the combobox structure with its `.navigator-combobox` (`data-path`), `.navigator-toggle-checkbox`, `.navigator-selected-text`, `.navigator-arrow` and `.navigator-dropdown` elements. |

#### The file icons mapping (assets/file-icons.json)

The icons of the file system entries are owned by the theme. The theme ships its icon files under `assets/images/icons/`, and maps file extensions to icon paths in an optional `assets/file-icons.json`:

```json
{
    "default": "file.svg",
    "directory": "directory.svg",
    "pdf": "application-pdf.svg",
    "zip": "application-x-7z-ace.svg"
}
```

The application fetches this mapping when the component initializes and resolves icon URLs against `iconBaseUrl`. Extensions without an entry fall back to `default`, directories use `directory`. When the theme does not ship the mapping or the icons, the component renders without icons rather than failing.

### Page Templates

The remaining templates render individual pages. Each page exposes its own data, so the available fields depend on the page. In addition to page specific values, pages expose a `strings` dictionary with the localized strings of the page, keyed by their resource names, so templates can pull any label without hardcoding text.

Page templates are rendered into a content section that the layout wraps. A page template can also declare a reserved top-level `scripts` section, whose content is rendered separately and passed to the layout, mirroring the `Scripts` section of a Razor view:

```html
{{#title}}
<h1 class="page-title">{{title}}</h1>
{{/title}}

{{^title}}
<p>No title available.</p>
{{/title}}

{{#sections}}
<div class="section">
    <h2>{{label}}</h2>
</div>
{{/sections}}

{{#scripts}}
<script>
    window.pageConfiguration = {{&configurationJson}};
</script>
{{/scripts}}
```

### The Scripts Section

JavaScript is always allowed in themes: script elements in templates are served and rendered as-is, and script files under `assets/` are served normally. A theme relies on scripts for the client side navigation of the application, because pages are swapped in place by the global AJAX navigator, so themes are expected to ship their own JavaScript. Because JavaScript is enabled, *themes can execute malicious code*, so, be aware about the source of your installed themes! Themes bundled with the application are reviewed and safe.

## Assets

Static files under `assets/` are served to the browser. The asset base URL is available to the layout and page templates as `assetBase`, so a stylesheet is referenced as `{{assetBase}}/site.css`. Assets are served only from within the pack, and paths that would escape the pack are rejected.

A theme owns its behavior as much as its look: the theme script, for example `{{assetBase}}/theme.js`, carries the behavior of the theme chrome, such as the desktop menu bar.

A theme ships the images its stylesheets use as assets too, referenced with paths relative to the stylesheet. For example a stylesheet at `assets/combobox.css` references `images/ui/metal-plate.png`, which is served from `assets/images/ui/metal-plate.png`.

The `preview` field of the manifest points at an asset that is shown as the theme thumbnail on the administration page.

### The top offset contract

The application translates page coordinates into the content coordinate space of some widgets (for example the selection rectangle of the file system browser) by subtracting the height of the fixed top bar of the theme. A theme that renders a fixed top bar declares that height on the root element as the `--theme-top-offset` custom property, for example in its theme script:

```js
document.documentElement.style.setProperty('--theme-top-offset', window.getComputedStyle(menubar).height);
```

When the property is absent it is treated as `0`, which is correct for a theme without a fixed top bar.

## Configuration Options

The theme engine is configured through the `ThemeEngine` section of the API configuration (`appsettings.shared.json`):

| Option | Default | Meaning |
| --- | --- | --- |
| `StoragePath` | `themes` | Directory where installed theme packs are stored, relative to the API base directory when not rooted. |
| `BundledThemesPath` | `themes/bundled` | Directory where the theme archives shipped with the application are located. |
| `DefaultThemeId` | `lumina-default` | Identifier of the theme selected when no valid current theme is persisted. |
| `MaxArchiveBytes` | `8388608` | Maximum size of an uploaded theme archive, in bytes (8 MB). |
| `MaxExpandedBytes` | `25165824` | Maximum total size of an extracted theme pack, in bytes (24 MB). |
| `MaxSingleFileBytes` | `6291456` | Maximum size of a single file within a theme pack, in bytes (6 MB). |
| `MaxEntries` | `250` | Maximum number of entries in a theme pack archive. |

## Creating a Theme

1. Create a directory for the theme with `theme.json`, a `templates/` directory and an `assets/` directory, following the layout of `editorial-paper`.
2. Write the manifest following the field rules in [The Manifest (theme.json)](#the-manifest-themejson).
3. Author the templates. Start with `templates/shared/layout.html` and `templates/shared/nav-menu.html`, then add page templates and the optional `templates/shared/file-system-browser*.html` component templates.
4. Add the static files under `assets/`, for example a stylesheet referenced from the layout.
5. Keep the pack within the size limits: the archive at most 8 MB, the extracted pack at most 24 MB, no single file larger than 6 MB and at most 250 entries.
6. Compress the directory into a ZIP archive with `theme.json` at the root of the archive, not nested inside an extra folder.

## Installing a Theme

A theme is installed by uploading its ZIP archive:

- From the administration themes page, using the upload control.
- Directly against the API with a `multipart/form-data` POST to `POST /api/v1/themes`, sending the archive in a form field named `archive`.

The archive is validated against the pack rules and stored on the server under `themes/{id}`. Installing a theme with an id that already exists replaces the files of the existing theme. A newly installed theme is not active until it is selected as the current theme.

Validation failures are returned as problem details, for example when the archive is unreadable, the manifest is invalid, or a template references a missing file.

## Bundled Themes

The archives shipped with the application live in the API under `Core/Themes/Bundled` and are installed automatically at startup by the theme detection job. `editorial-paper` is the bundled reference theme.

A bundled theme whose files go missing or are corrupted is restored automatically from its shipped archive on the next startup, unless the user explicitly deleted the theme from the Administrator - Themes page. This means bundled themes always come back unless the user removes them. The last available theme cannot be deleted.

## Selecting the Current Theme

The current theme is the one whose templates are used for page rendering. At startup, when no theme is current, the application activates one automatically, preferring the configured default theme id. The active theme is exposed at `GET /api/v1/themes/current` and can be changed from the administration page, which calls `PUT /api/v1/themes/current`.

## Managing Themes from the Administration Page

The administration themes page lists every installed theme and provides:

- A preview thumbnail for each theme, taken from the `preview` field of the manifest.
- The source of the theme, either bundled with the application or provided by a user.
- The ability to set the current theme, download a theme as a ZIP archive and install a new theme.
- The ability to delete a theme, subject to these rules:
  - Only administrators can delete themes.
  - The last remaining bundled theme cannot be deleted, so the application never ends up without a theme.
  - When the active theme is deleted, another available theme is activated automatically, preferring the configured default.
  - Bundled themes are soft deleted (the theme files are deleted, but the theme zip archive is kept), so they can be restored; user themes are removed entirely, because their files are not shipped with the application.

## Security Notes

Theme packs are treated as untrusted input:

- Values rendered with `{{name}}` are HTML-encoded, so a theme cannot inject markup through a model value.
- Themes can contain javascript, which means, they can run malicious code. Only install themes from sources you trust, or review the code of the theme files.
- Paths are normalized and validated so that templates and assets cannot escape the theme pack or the storage directory.
- The template engine has no access to the application and cannot execute code, only render values against the model.
- Size and count limits prevent an archive from exhausting server memory or disk.
