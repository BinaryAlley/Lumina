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
- _**Song**_: An individual music track
- _**Album**_: A collection of songs
- _**Artist**_: The creator of music content
- _**Audiobook**_: A narrated version of a book
- _**Live Recording**_: Audio or video of a live performance
- _**Interview**_: Audio or video recording of a conversation
- _**Cover Song**_: A rendition of a song performed by an artist other than the original
- _**Remix**_: A modified version of an original song
- _**Sound Effect**_: A short audio clip used for specific purposes
- _**Soundtrack**_: A collection of music used in a film or TV show
- _**Podcast**_: A series of audio episodes
- _**Podcast Episode**_: An individual installment of a podcast
- _**Radio Show**_: An audio program broadcast on the radio

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
- Song
- Album
- Artist
- Audiobook
- LiveRecording
- Interview
- CoverSong
- Remix
- SoundEffect
- Podcast
- PodcastEpisode
- Book
- BookSeries
- ComicBook
- ComicSeries
- Magazine
- MagazineIssue
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

## Aggregates
- VideoLibrary (root: VideoLibrary, entities: TVShow, Season, Episode, Movie, FilmSeries, ConcertVideo, Documentary, TutorialVideo, HomeVideo, Anime, YouTubeVideo, MusicVideo)
- AudioLibrary (root: AudioLibrary, entities: Song, Album, Artist, Audiobook, LiveRecording, Interview, CoverSong, Remix, SoundEffect)
- PodcastLibrary (root: PodcastLibrary, entities: Podcast, PodcastEpisode)
- WrittenContentLibrary (root: WrittenContentLibrary, entities: Book, EBook, BookSeries, ComicBook, ComicSeries, Magazine, MagazineIssue, Manga, GraphicNovel, AcademicPaper, SheetMusic, Screenplay)
- PhotoLibrary (root: PhotoLibrary, entities: Photo, PhotoAlbum, AlbumArtwork, MoviePoster)
- UserProfile (root: User, entities: Playlist, Collection)
- SupplementaryContentLibrary (root: SupplementaryContentLibrary, entities: SubtitleFile, Lyrics, LanguageTrack)
- FileManagement (root: File, entities: Stream)

- WrittenContentLibrary
  - BookLibrary (root: BookLibrary)
    - Entities: Book, BookSeries
    - Value Objects: BookMetadata, SeriesInfo
  
  - ComicLibrary (root: ComicLibrary)
    - Entities: ComicBook, ComicSeries, Manga, GraphicNovel
    - Value Objects: ComicMetadata, SeriesInfo
  
  - PeriodicalLibrary (root: PeriodicalLibrary)
    - Entities: Magazine, MagazineIssue
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
- Rating
- Genre
- ReleaseInfo
- LanguageInfo

## Domain Services
- MediaScanner (with specialized scanners for each media type)
- MetadataFetcher (with different strategies for various media types and sources)
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
  - Aggregates: VideoLibrary, AudioLibrary, PodcastLibrary, WrittenContentLibrary, PhotoLibrary
  - Services: MediaScanner, MetadataFetcher, TranscodingService
- UserExperience
  - Aggregates: UserProfile
  - Services: RecommendationEngine, ContentCurationService, UserActivityTrackingService
- ContentDelivery
  - Aggregates: FileManagement
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