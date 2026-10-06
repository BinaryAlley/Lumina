# Ubiquitous Language

## General Concepts
- _**Media Library**_: The overall collection of various media types
- _**Metadata**_: Information describing a media item
- _**User**_: A person who interacts with the media library
- _**Playlist**_: A curated list of media items, potentially cross-media-type
- _**Tag**_: A label for categorizing and organizing media items
- _**Collection**_: A user-defined group of any media items
- _**File**_: The actual digital file of a media item
- _**Stream**_: A media item accessed via streaming rather than local storage
- _**Language Track**_: An audio or text track in a specific language
- _**Media Contributor**_: A person, group, or organization that contributed to a media item, together with the role they played

## Video Content
- _**TV Show**_: A series of episodes typically organized into seasons
- _**Season**_: A grouping of episodes within a TV show
- _**Episode**_: An individual installment of a TV show
- _**Movie**_: A standalone film
- _**Film Series**_: A collection of related movies (e.g., The Lord of the Rings trilogy)
- _**Film Franchise**_: A broader collection of related movies (e.g., James Bond films)
- _**Concert Video**_: Recording of a live music performance
- _**Documentary**_: Non-fiction film or series
- _**Tutorial/Instructional Video**_: Educational content
- _**Home Video**_: Personal recordings
- _**Anime**_: Japanese animation, which may have different categorization needs
- _**YouTube Video**_: A video hosted on YouTube, with its own metadata structure
- _**Music Video**_: A video representation of a song

## Audio Content
- _**Track**_: An individual music track, the smallest unit of music in a music library (the entity is named Track, not Song)
- _**Album**_: A release that gathers the tracks of an artist
- _**Artist**_: The creator of music content, the root of the music library aggregate
- _**MusicReleaseType**_: The set of content types a music release can carry, like Album, Single, Ep, Compilation, AudioBook, Interview, Live, Remix, Soundtrack or SpokenWord

## Written Content
- _**Book**_: A standalone written work
- _**E-book**_: A digital version of a book
- _**Book Series**_: A collection of related books
- _**Comic Book**_: A single issue of a comic
- _**Comic Series**_: A collection of related comic books
- _**Magazine**_: A periodical publication
- _**Magazine Issue**_: A single edition of a magazine
- _**Manga**_: Japanese comics with specific formatting and reading direction
- _**Graphic Novel**_: Long-form comic book
- _**Academic Paper**_: Scholarly article
- _**Sheet Music**_: Musical notation for songs

## Visual Content
- _**Photo**_: An individual image
- _**Photo Album**_: A collection of related photos

## Miscellaneous Media
- _**Subtitle File**_: Text file synchronized with video content
- _**Lyrics**_: Text of a song, potentially time-synced
- _**Album Artwork**_: Visual representation of an album
- _**Movie Poster**_: Promotional image for a film
- _**Screenplay**_: Script for a film or TV show

## Entities
- TVShow
- Season
- Episode
- Movie
- FilmSeries
- ConcertVideo
- Documentary
- TutorialVideo
- HomeVideo
- Anime
- YouTubeVideo
- MusicVideo
- Track
- Album
- Artist
- Podcast
- PodcastEpisode
- Book
- BookSeries
- ComicBook
- ComicSeries
- Magazine
- MagazineIssue
- Newspaper
- Manga
- GraphicNovel
- AcademicPaper
- SheetMusic
- Photo
- PhotoAlbum
- User
- Playlist
- Collection
- SubtitleFile
- Lyrics
- AlbumArtwork
- MoviePoster
- Screenplay
- File
- Stream
- LanguageTrack
- MediaContributor

## Aggregates
- VideoLibrary (root: VideoLibrary, entities: TVShow, Season, Episode, Movie, FilmSeries, ConcertVideo, Documentary, TutorialVideo, HomeVideo, Anime, YouTubeVideo, MusicVideo)
- MusicLibrary (root: Artist, entities: Album, Track)
- PodcastLibrary (root: PodcastLibrary, entities: Podcast, PodcastEpisode)
- WrittenContentLibrary (root: WrittenContentLibrary, entities: Book, EBook, BookSeries, ComicBook, ComicSeries, Magazine, MagazineIssue, Newspaper, Manga, GraphicNovel, AcademicPaper, SheetMusic, Screenplay)
- PhotoLibrary (root: PhotoLibrary, entities: Photo, PhotoAlbum, AlbumArtwork, MoviePoster)
- UserProfile (root: User, entities: Playlist, Collection)
- SupplementaryContentLibrary (root: SupplementaryContentLibrary, entities: SubtitleFile, Lyrics, LanguageTrack)
- FileSystemManagement (root: File, entities: Stream)

- WrittenContentLibrary
  - BookLibrary (root: BookLibrary)
    - Entities: Book, BookSeries
    - Value Objects: BookMetadata, SeriesInfo
  
  - ComicLibrary (root: ComicLibrary)
    - Entities: ComicBook, ComicSeries, Manga, GraphicNovel
    - Value Objects: ComicMetadata, SeriesInfo
  
  - PeriodicalLibrary (root: PeriodicalLibrary)
    - Entities: Magazine, MagazineIssue, Newspaper
    - Value Objects: PeriodicalMetadata, IssueInfo
  
  - AcademicLibrary (root: AcademicLibrary)
    - Entities: AcademicPaper
    - Value Objects: AcademicMetadata, CitationInfo
  
  - MiscWrittenContentLibrary (root: MiscWrittenContentLibrary)
    - Entities: SheetMusic, Screenplay
    - Value Objects: SheetMusicMetadata, ScreenplayMetadata

## Value Objects
- BaseMetadata
- VideoMetadata
- MovieMetadata
- TVShowMetadata
- DocumentaryMetadata
- TutorialMetadata
- AnimeMetadata
- AudioMetadata
- PodcastMetadata
- AudiobookMetadata
- LiveRecordingMetadata
- InterviewMetadata
- RemixInfo
- BookMetadata
- ComicMetadata
- MagazineMetadata
- MangaMetadata
- AcademicMetadata
- PhotoMetadata
- SheetMusicInfo
- SubtitleInfo
- LyricsInfo
- ArtworkInfo
- FileInfo
- StreamInfo
- StreamingQuality
- UserPreferences
- Rating (abstract)
- AudioRating
- Genre
- Mood
- ReleaseInfo
- LanguageInfo
- AlbumMetadata
- MusicMediaContributor
- MusicBrainzId
- MusicArea
- MusicArtistAlias
- MusicWork
- Isrc
- Barcode

## Domain Services
- Music Library Type Scanner
- Music File System Discovery Job
- Music Metadata Extraction Job
- TranscodingService
- StreamingService
- AudiobookPlaybackService
- LiveContentManager
- RemixDetectionService
- AnimeCategorizationService
- AcademicContentIndexer
- LyricsMatchingService
- ArtworkManagementService
- SearchService
- RecommendationEngine
- FileManagementService
- LanguageTrackManager
- CrossMediaRelationshipManager
- ContentCurationService
- UserActivityTrackingService
- ContentSynchronizationService

## Domain Events

- MediaItemAdded
- MediaItemRemoved
- PlaylistCreated
- PlaylistUpdated
- UserPreferencesChanged
- MetadataUpdated
- FileTranscoded
- StreamStarted
- StreamEnded

## Bounded Contexts

- MediaManagement
  - Aggregates: VideoLibrary, MusicLibrary, PodcastLibrary, WrittenContentLibrary, PhotoLibrary
  - Services: Music Library Type Scanner, Music File System Discovery Job, Music Metadata Extraction Job
- UserExperience
  - Aggregates: UserProfile
  - Services: RecommendationEngine, ContentCurationService, UserActivityTrackingService
- ContentDelivery
  - Aggregates: FileSystemManagement
  - Services: StreamingService, ContentSynchronizationService
- MetadataManagement
  - Aggregates: SupplementaryContentLibrary
  - Services: LyricsMatchingService, ArtworkManagementService

## Repositories
- VideoRepository
- AudioRepository
- PodcastRepository
- WrittenContentRepository
- PhotoRepository
- UserRepository
- PlaylistRepository
- CollectionRepository
- FileRepository
- StreamRepository
