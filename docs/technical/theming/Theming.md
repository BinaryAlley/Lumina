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
| `library/written-content-library/book-library/books/index` | The books browsing page. |

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

### The Navigation Template (shared/nav-menu)

The navigation template renders the menu of the active theme. Its model contains the site name and two menu structures, one for the mobile menu and one for the desktop menu bar:

- `siteName` is the site name.
- `mobileSections` and `menubarSections` are lists of sections.
- A section has a `label` and a list of `items`.
- An item has a `label`, an optional `url`, an optional `cssClass` and a list of `children`. Items with children are submenus, items without children are plain links.

The navigation template is also best effort: when its template cannot be rendered, the application menu is used instead.

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
3. Author the templates. Start with `templates/shared/layout.html` and `templates/shared/nav-menu.html`, then add page templates.
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
