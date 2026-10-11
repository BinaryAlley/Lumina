# Lumina Manual

## Contents

**Getting started**

1. **Introduction**
2. [Installation](installation/index.md)

**Using Lumina**

3. [Using Lumina](using-lumina/index.md)

**Administration**

4. [Managing Libraries](managing-libraries/index.md)
5. [Plugins](plugins/index.md)
6. [Themes](themes/index.md)

Your files have waited long enough. Lumina is a media server that gives a whole collection one place to live, whether that collection is books, music, or the many other kinds of media that quietly pile up on a hard drive. It runs on your own hardware, answers to you and no one else, and never asks for a subscription before it will show you the files you already own.

> **Pre-alpha, not production ready.** Lumina is under heavy development, and breaking changes happen all the time. Configuration, APIs, data formats and behavior can shift between versions, and nothing guarantees backward compatibility or a way back to an earlier version. Use Lumina to explore and to shape it, not to run a service you depend on.

## What Lumina is

Under the hood, Lumina is two applications that talk to each other:

- The **API server** is the part that knows things. It keeps track of your libraries, metadata, users and settings, and hands them out as a JSON REST API. It is also where the heavy lifting happens, like scanning folders and running background jobs.
- The **web client** is the part you look at. It talks to the API for you and paints the interface from the templates of whichever theme is switched on.

The two run separately and speak to each other over the network, so they can sit on the same machine or on machines on opposite sides of the planet. The Docker Compose stack brings them up together with a single command when that suits you. When you are ready to flip the lights on, the [installation guide](installation/index.md) walks through it.

## What Lumina can do today

Lumina is growing, and this is what works right now:

- **Media libraries that scan themselves.** Point Lumina at a folder or a drive, tell it what kind of media lives there, and it gets to work discovering and indexing what it finds. Scans run in the background, report their progress as they go, and can be started, cancelled or resumed.
- **Written content.** Add a written content library and Lumina scans it, cataloguing the books and e-books it contains and dressing them up with metadata for browsing in the client. Open one and it renders in the browser through a reader plugin; the EPUB and PDF readers are bundled and can be enabled per library.
- **Music.** Add a music library and Lumina indexes the artists, albums and tracks it contains, reading metadata from the tags inside the audio files. If you want more, enable a metadata provider and Lumina fills in the gaps from online sources, and artwork can come from your folders or from remote sources.
- **Metadata and artwork from plugins.** Plugins do the knowing. They supply the metadata and the artwork of a library, and you decide, library by library, which ones are switched on and which one wins when several of them recognize the same item.
- **A file system browser.** Reach the drives, folders and files of the server machine from anywhere you can open the client, as if you were sitting right in front of it. Poke around, walk a path to its root or to its parent, and let Lumina generate thumbnails for what it finds.
- **Accounts, roles, permissions and policies.** Create accounts, sign in with a password and optional two factor authentication, and hand out access through roles, fine grained permissions and policies that decide who may reach which library or content. Administrators manage all of it from the administration area.
- **Theming.** The client is a themed shell, and a theme is a portable ZIP of HTML templates and assets that anyone can create. A theme can restyle the entire application and rearrange its navigation without a single line of code from the core, because some of us still find flat interfaces sterile and boring.
- **Plugins.** Upload an assembly or an archive, restart the server, and Lumina picks it up. Anyone can write a plugin to extend what Lumina understands, and a plugin can expose its own settings, which show up in the client.
- **Scheduled jobs.** Recurring work like library scans is driven by a scheduler with a live dashboard in the client. Watch the jobs run on a real time graph, read their history, and start, stop, fire or remove them on demand.
- **A local interface in many languages.** The client ships in ten languages and greets each visitor in theirs.
- **Observability.** Lumina logs and traces extensively, and everything it records lives on your machine. Nothing is sent anywhere: no analytics, no usage reporting, no telemetry, no exception reports, nothing leaves your server unless you deliberately wire it to a collector you run yourself.

Video libraries, podcasts and photos are sketched out in the design and on the way, but they are not part of what works today.

## How Lumina is put together

Lumina is written in C# on .NET 10 and keeps its data in SQLite, so the whole thing stays small enough to run happily on modest hardware. The codebase follows Clean Architecture and Domain Driven Design, and it is built from the ground up to be extended by themes and plugins. If you want the deep version of that story, the architecture documents and the domain model in the repository go all the way down.

## Security

You are handing Lumina the keys to your files, so it is built to deserve that trust:

- **Locked down by default.** The API authenticates every request, and only endpoints explicitly marked as public can be reached without signing in. Accounts, roles, fine grained permissions and policies decide who may reach which library or content, and sensitive endpoints are rate limited per client IP.
- **Secrets stay secret.** A production deployment must supply its own signing and encryption keys, and the server refuses to start without them.
- **Themes and plugins are treated as untrusted.** Theme packs are validated, their template and asset paths are normalized and confined to the pack, and their archive size, expanded size and entry count are capped. A plugin archive has its directory components stripped, so it cannot write outside the plugin directory, but it has no size or entry limit. Templates render through a small engine that cannot execute code, and a broken theme falls back to the built-in rendering. Themes can still ship their own JavaScript, so only install code from sources you trust.
- **Nothing phones home.** Lumina itself makes no outbound connections on its own. It reaches the internet only when your configuration tells it to, for example when you enable a metadata provider. Part of what it fetches on your behalf is guarded: artwork downloads reject literal loopback, private, link local and carrier grade NAT addresses, and follow redirects only to public HTTPS hosts. That guard is not a hard boundary, though. It does not resolve host names, so a name that points at an internal address can still get through, and the metadata calls a provider makes go out through the provider's own client, without that guard at all. Some providers add a check of their own: the MusicBrainz provider refuses a base address that is not public unless you turn on its `Allow LAN/Private Base URL` setting, which exists for pointing it at a self hosted instance. Enabling a web provider is trusting it to make requests from your server. Everything the client loads, from stylesheets and scripts to images, fonts and icons, is served from your own server, so using the client pings no third party that could note where you are or what you are looking at. One exception ships switched on, and it is worth knowing about: a book can carry its own styles, and those styles can make the browser load resources from outside servers, which those servers could observe. Lumina keeps a book's styles by default because it looks better, so a book from an untrusted source can leak your location this way. The `Preserve book styles` setting (see [Managing Libraries](managing-libraries/index.md)) turns it off, stripping the styles and closing that door, at the cost of the book's original look.
- **It is tested, a lot.** Do not take any of this on faith. The project is covered by more than 10,000 tests, including a dedicated security suite, because Lumina puts stability and security ahead of moving fast.

## Documentation, diagrams and comments

Lumina takes being readable seriously. Every type and member is documented, and the repository carries far more than this manual: the domain model and the language it speaks, the C4 architecture diagrams, an architecture decision log that records not only what was chosen but why, and a reference for every API endpoint, complete with ready to run request files.

The aim is that anyone, including someone early in their career, can open the code and follow a thread from an endpoint down through its handler into the domain without having to reverse engineer it. The project is also strict with itself: its naming, formatting, layering, testing and documentation conventions are enforced rather than suggested, and a contribution is expected to respect them. That discipline is exactly what keeps the codebase consistent as it grows.

If you feel like helping out, there is no permission to ask for: read the [contributing guide](../CONTRIBUTING.md), pick a branch, and send a pull request.

## Where the project is

Lumina is young and moves quickly. It is pre-alpha software: some rooms are furnished, others still have the plans taped to the wall, and both the code and this documentation change often. When a page here disagrees with the application, trust the application.

## Credits

Lumina stands on the shoulders of others. The visual style is inspired by [Enlightenment](https://www.enlightenment.org/), most icons come from the [Lyra Icon Theme](https://github.com/yeyushengfan258/Lyra-icon-theme/tree/master/src), and several interface icons were adapted from [svgrepo](https://www.svgrepo.com/). Full attribution is kept in the [README](../../README.md).

## License

Lumina is licensed under the GPLv3.0. See the [LICENSE](../../LICENSE.md) file for the full text.
