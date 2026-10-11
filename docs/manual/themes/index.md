# Themes

## Contents

**Getting started**

1. [Introduction](../index.md)
2. [Installation](../installation/index.md)

**Using Lumina**

3. [Using Lumina](../using-lumina/index.md)

**Administration**

4. [Managing Libraries](../managing-libraries/index.md)
5. [Plugins](../plugins/index.md)
6. **Themes**

The web client is a themed shell. Every page it shows is drawn from the templates of the active theme, so a theme decides how Lumina looks, how it is laid out, and how its navigation is arranged. A theme is a portable ZIP archive of HTML templates and static assets, and swapping one in can change the entire feel of the application without a change to its code, because some of us still find flat interfaces sterile and boring. :)

A theme changes how Lumina looks and how its pages are structured. It cannot add features: it shapes what is already there.

## What a theme is

A theme pack is a ZIP archive that holds three things, and nothing else:

- a `theme.json` manifest at the root, describing the theme;
- a `templates/` directory with the HTML the client renders;
- an `assets/` directory with everything static, such as stylesheets, images, fonts and scripts.

The API server stores installed themes and serves their templates and assets. The web client fetches those pieces and renders them with a small, non-executable template engine, so a theme can interpolate values, repeat over collections and test conditions, but cannot run arbitrary code in the templates themselves.

## Bundled themes

Lumina ships with two themes, both installed automatically at startup:

- **`lumina-default`**, the theme Lumina falls back to when no other theme is selected.
- **`editorial-paper`**, a clean, readable theme that is also the reference to start from when you write your own.

A bundled theme that goes missing or becomes corrupted is restored automatically from its shipped archive the next time the server starts, unless you deliberately deleted it. This means a bundled theme always comes back on its own.

## Selecting the current theme

Administrators choose the active theme from the administration Themes page. The page lists every installed theme with a preview thumbnail taken from the theme's own manifest, and marks where each one came from, whether it is bundled with Lumina or installed by a user. Selecting a theme makes it the one every page is rendered with.

## Installing a theme

To install a theme, upload its ZIP archive from the Themes page. The archive is checked before anything is stored: the manifest must be present and valid, the pack may only contain the manifest, templates and assets, and it has to stay within the size limits below. An invalid pack is rejected whole, leaving nothing behind.

Installing a theme whose id already exists replaces the files of that theme. A freshly installed theme is not active until you select it, so you can install first and switch later.

## Managing installed themes

Beyond selecting and installing, the Themes page lets an administrator download any theme back out as a ZIP, delete a theme, and restore a bundled one. Deleting follows a few rules that keep Lumina from ever ending up themeless:

- Only administrators can delete a theme.
- The last remaining bundled theme cannot be deleted.
- When the active theme is deleted, another available theme is activated automatically, preferring the configured default.
- Bundled themes are deleted softly: their files are removed but their shipped archive is kept, so they can be restored. Themes you installed yourself are removed entirely, because Lumina does not keep a copy of them.

## Theme data caching

Each user has a setting, `Enable theme caching`, on by default, that controls whether the theme data served to them is cached. Turning it off makes the client fetch theme templates and assets fresh every time, which is useful while you are developing a theme; leaving it on is faster for everyday use.

## Creating a theme

Anyone can write a theme, and it is the intended way to change Lumina's look. A theme is a directory with `theme.json` at its root, a `templates/` directory and an `assets/` directory, compressed into a ZIP with the manifest at the root of the archive, not nested in an extra folder. The bundled `editorial-paper` theme follows exactly this shape and is the best starting point.

### The manifest

The manifest is a small JSON object:

| Field | Required | Rule |
| --- | --- | --- |
| `schemaVersion` | yes | Must be `1`. |
| `id` | yes | Lowercase kebab-case, up to 64 characters, used to reference the theme, for example `my-dark-theme`. |
| `name` | yes | Display name, up to 80 characters. |
| `description` | yes | What the theme is, up to 300 characters. |
| `author` | yes | Who made it, up to 80 characters. |
| `version` | yes | A semantic version, for example `1.0.0`. |
| `preview` | no | Path to a preview image under `assets/`, shown as the theme thumbnail. |
| `templates` | no | An optional map from template key to a file under `templates/`. |

### Templates

Templates use a small, Mustache-style syntax: `{{name}}` prints a value, escaped; `{{{name}}}` or `{{&name}}` prints it raw; `{{#name}}...{{/name}}` repeats or shows a section; `{{^name}}...{{/name}}` shows a section when a value is absent. A theme provides a layout that wraps every page, a navigation menu, an optional 404 page, page templates for the pages it wants to restyle, and optionally templates for the file system browser component.

A page is matched to a template by a key that mirrors the page's path, so the books page looks for `templates/library/written-content-library/book-library/books/index.html`, or a template at a parent scope when the exact one is missing. When a theme has no template for a page, the application renders that page itself with its own built-in view, so a theme never has to cover everything.

The engine enforces hard limits so a single theme cannot exhaust the server: at most 32 levels of section nesting, expressions no longer than 120 characters, and at most 4 MB of rendered output per page.

### Assets

Everything under `assets/` is served to the browser. A stylesheet referenced from the layout, the images it uses, fonts and scripts all live there. A theme can also ship an optional `assets/file-icons.json` that maps file extensions to icon files for the file system browser. Because scripts are allowed, a theme carries its own behavior too, and it is on you to ship the JavaScript its chrome needs.

### Limits

A theme pack must fit inside these limits, or it is rejected on install:

| Limit | Value |
| --- | --- |
| Archive size | 8 MB |
| Total expanded size | 24 MB |
| Single file size | 6 MB |
| Number of entries | 250 |

The full template and component reference, including the exact fields each page and component exposes, lives in the [theming guide](../../technical/theming/Theming.md) for authors who need the details.

## Security

A theme is treated as untrusted input, but it is not powerless:

- Values printed with `{{name}}` are HTML-escaped, so a theme cannot inject markup through a model value, and the template engine cannot execute code.
- Paths are normalized and confined, so a theme cannot reach outside its own pack, and an oversized or over-populated archive is refused.
- A theme can, however, contain JavaScript, and that JavaScript runs in the browser of every user who views it. A malicious theme is a real risk. Only install themes from sources you trust, or read their code first. The themes bundled with Lumina are written and reviewed by the project and are safe to use.
