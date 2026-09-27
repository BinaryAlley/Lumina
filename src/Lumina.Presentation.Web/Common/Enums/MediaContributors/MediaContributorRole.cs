namespace Lumina.Presentation.Web.Common.Enums.MediaContributors;

/// <summary>
/// Enumeration for the role of a media contributor in a media item.
/// The role is used as the key of the localized display strings of the UI, and normalizes the free-form role names
/// returned by the metadata providers, so that roles that describe the same kind of contribution, like "Author" and
/// "Writer", are never treated as distinct.
/// </summary>
public enum MediaContributorRole
{
    /// <summary>
    /// The role does not fall into any of the known roles.
    /// </summary>
    Other,

    /// <summary>
    /// The contributor wrote the media item, like a book, a movie, or a piece of music.
    /// </summary>
    Author,

    /// <summary>
    /// The contributor translated the media item into another language.
    /// </summary>
    Translator,

    /// <summary>
    /// The contributor created the illustrations of the media item.
    /// </summary>
    Illustrator,

    /// <summary>
    /// The contributor published the media item.
    /// </summary>
    Publisher,

    /// <summary>
    /// The contributor narrated the media item.
    /// </summary>
    Narrator,

    /// <summary>
    /// The contributor played a role in a filmed media item.
    /// </summary>
    Actor,

    /// <summary>
    /// The contributor directed a filmed media item.
    /// </summary>
    Director,

    /// <summary>
    /// The contributor wrote the screenplay of a filmed media item.
    /// </summary>
    Screenwriter,

    /// <summary>
    /// The contributor wrote the music of the media item.
    /// </summary>
    Composer,

    /// <summary>
    /// The contributor wrote the lyrics of the media item.
    /// </summary>
    Lyricist,

    /// <summary>
    /// The contributor arranged the music of the media item.
    /// </summary>
    Arranger,

    /// <summary>
    /// The contributor conducted the ensemble performing the media item.
    /// </summary>
    Conductor,

    /// <summary>
    /// The contributor oversaw the production of the media item.
    /// </summary>
    Producer,

    /// <summary>
    /// The contributor oversaw the overall production of the media item, rather than a single track or episode.
    /// </summary>
    ExecutiveProducer,

    /// <summary>
    /// The contributor operated the recording equipment during the production of the media item.
    /// </summary>
    Engineer,

    /// <summary>
    /// The contributor mixed the audio of the media item.
    /// </summary>
    Mixer,

    /// <summary>
    /// The contributor created a remix of the media item.
    /// </summary>
    Remixer,

    /// <summary>
    /// The contributor released the media item under a record label.
    /// </summary>
    RecordLabel,

    /// <summary>
    /// The contributor sang the media item.
    /// </summary>
    Vocals,

    /// <summary>
    /// The contributor sang the backing vocals of the media item.
    /// </summary>
    BackingVocals,

    /// <summary>
    /// The contributor played the guitar in the media item.
    /// </summary>
    Guitar,

    /// <summary>
    /// The contributor played the bass guitar in the media item.
    /// </summary>
    BassGuitar,

    /// <summary>
    /// The contributor played the drums in the media item.
    /// </summary>
    Drums,

    /// <summary>
    /// The contributor played percussion in the media item.
    /// </summary>
    Percussion,

    /// <summary>
    /// The contributor played the keyboards in the media item.
    /// </summary>
    Keyboards,

    /// <summary>
    /// The contributor played the piano in the media item.
    /// </summary>
    Piano,

    /// <summary>
    /// The contributor played the synthesizer in the media item.
    /// </summary>
    Synthesizer,

    /// <summary>
    /// The contributor played a string instrument in the media item.
    /// </summary>
    Strings,

    /// <summary>
    /// The contributor played a woodwind instrument in the media item.
    /// </summary>
    Woodwinds,

    /// <summary>
    /// The contributor played a brass instrument in the media item.
    /// </summary>
    Brass,

    /// <summary>
    /// The contributor sang in a choir in the media item.
    /// </summary>
    Choir,

    /// <summary>
    /// The contributor orchestrated the music of the media item.
    /// </summary>
    Orchestrator
}
