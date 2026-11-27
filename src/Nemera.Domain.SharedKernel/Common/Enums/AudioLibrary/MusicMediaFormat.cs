#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the physical or digital medium of a music release.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MusicMediaFormat
{
    /// <summary>
    /// The release was distributed on a 10 inch acetate disc.
    /// </summary>
    Inch10Acetate,

    /// <summary>
    /// The release was distributed on a 10 inch shellac disc.
    /// </summary>
    Inch10Shellac,

    /// <summary>
    /// The release was distributed on a 10 inch vinyl record.
    /// </summary>
    Inch10Vinyl,

    /// <summary>
    /// The release was distributed on a 12 inch acetate disc.
    /// </summary>
    Inch12Acetate,

    /// <summary>
    /// The release was distributed on a 12 inch LaserDisc.
    /// </summary>
    Inch12LaserDisc,

    /// <summary>
    /// The release was distributed on a 12 inch shellac disc.
    /// </summary>
    Inch12Shellac,

    /// <summary>
    /// The release was distributed on a 12 inch vinyl record.
    /// </summary>
    Inch12Vinyl,

    /// <summary>
    /// The release was distributed on a 3 inch vinyl record.
    /// </summary>
    Inch3Vinyl,

    /// <summary>
    /// The release was distributed on a 3.5 inch floppy disk.
    /// </summary>
    Inch3_5FloppyDisk,

    /// <summary>
    /// The release was distributed on a 5.25 inch floppy disk.
    /// </summary>
    Inch5_25FloppyDisk,

    /// <summary>
    /// The release was distributed on a 7 inch acetate disc.
    /// </summary>
    Inch7Acetate,

    /// <summary>
    /// The release was distributed on a 7 inch flexi disc.
    /// </summary>
    Inch7FlexiDisc,

    /// <summary>
    /// The release was distributed on a 7 inch shellac disc.
    /// </summary>
    Inch7Shellac,

    /// <summary>
    /// The release was distributed on a 7 inch vinyl record.
    /// </summary>
    Inch7Vinyl,

    /// <summary>
    /// The release was distributed on an 8 inch LaserDisc.
    /// </summary>
    Inch8LaserDisc,

    /// <summary>
    /// The release was distributed on an 8-track cartridge.
    /// </summary>
    EightTrackCartridge,

    /// <summary>
    /// The release was distributed on an 8 cm compact disc.
    /// </summary>
    Cm8CD,

    /// <summary>
    /// The release was distributed on an 8 cm compact disc with graphics.
    /// </summary>
    Cm8CDPlusG,

    /// <summary>
    /// The release was distributed on an 8 cm recordable compact disc.
    /// </summary>
    Cm8CDMinusR,

    /// <summary>
    /// The release was distributed on an acetate disc.
    /// </summary>
    Acetate,

    /// <summary>
    /// The release was distributed on a Betacam SP videotape.
    /// </summary>
    BetacamSP,

    /// <summary>
    /// The release was distributed on a Betamax videocassette.
    /// </summary>
    Betamax,

    /// <summary>
    /// The release was distributed on a Blu-ray disc.
    /// </summary>
    BluRay,

    /// <summary>
    /// The release was distributed on a recordable Blu-ray disc.
    /// </summary>
    BluRayMinusR,

    /// <summary>
    /// The release was distributed on a Blu-spec compact disc.
    /// </summary>
    BluSpecCD,

    /// <summary>
    /// The release was distributed on a cartridge.
    /// </summary>
    Cartridge,

    /// <summary>
    /// The release was distributed on a compact cassette.
    /// </summary>
    Cassette,

    /// <summary>
    /// The release was distributed on a compact disc.
    /// </summary>
    CD,

    /// <summary>
    /// The release was distributed on a compact disc with graphics.
    /// </summary>
    CDPlusG,

    /// <summary>
    /// The release was distributed on a CD-i (compact disc interactive).
    /// </summary>
    CDMinusi,

    /// <summary>
    /// The release was distributed on a recordable compact disc.
    /// </summary>
    CDMinusR,

    /// <summary>
    /// The release was distributed on a CD Video.
    /// </summary>
    CDV,

    /// <summary>
    /// The release was distributed on a Capacitance Electronic Disc.
    /// </summary>
    CED,

    /// <summary>
    /// The release was distributed on a copy-controlled compact disc.
    /// </summary>
    CopyControlCD,

    /// <summary>
    /// The release was distributed on a Digital Audio Tape.
    /// </summary>
    DAT,

    /// <summary>
    /// The release was distributed on a compact disc used for data storage.
    /// </summary>
    DataCD,

    /// <summary>
    /// The release was distributed on a DVD used for data storage.
    /// </summary>
    DataDVD,

    /// <summary>
    /// The release was distributed on a recordable DVD used for data storage.
    /// </summary>
    DataDVDMinusR,

    /// <summary>
    /// The release was distributed on a DataPlay disc.
    /// </summary>
    DataPlay,

    /// <summary>
    /// The release was distributed on a Digital Compact Cassette.
    /// </summary>
    DCC,

    /// <summary>
    /// The release was distributed as a digital download or stream.
    /// </summary>
    DigitalMedia,

    /// <summary>
    /// The release was distributed on a download card.
    /// </summary>
    DownloadCard,

    /// <summary>
    /// The release was distributed on a DTS-compatible compact disc.
    /// </summary>
    DTSCD,

    /// <summary>
    /// The release was distributed on a DualDisc.
    /// </summary>
    DualDisc,

    /// <summary>
    /// The release was distributed on the CD side of a DualDisc.
    /// </summary>
    DualDiscCDSide,

    /// <summary>
    /// The release was distributed on the DVD side of a DualDisc.
    /// </summary>
    DualDiscDVDSide,

    /// <summary>
    /// The release was distributed on the DVD-audio side of a DualDisc.
    /// </summary>
    DualDiscDVDAudioSide,

    /// <summary>
    /// The release was distributed on the DVD-video side of a DualDisc.
    /// </summary>
    DualDiscDVDVideoSide,

    /// <summary>
    /// The release was distributed on a DVD.
    /// </summary>
    DVD,

    /// <summary>
    /// The release was distributed on a DVD-Audio.
    /// </summary>
    DVDAudio,

    /// <summary>
    /// The release was distributed on a recordable DVD used for video.
    /// </summary>
    DVDMinusRVideo,

    /// <summary>
    /// The release was distributed on a DVD-Video.
    /// </summary>
    DVDVideo,

    /// <summary>
    /// The release was distributed on a DVD+.
    /// </summary>
    DVDPlus,

    /// <summary>
    /// The release was distributed on the CD side of a DVD+.
    /// </summary>
    DVDPlusCDSide,

    /// <summary>
    /// The release was distributed on the DVD-audio side of a DVD+.
    /// </summary>
    DVDPlusDVDAudioSide,

    /// <summary>
    /// The release was distributed on the DVD-video side of a DVD+.
    /// </summary>
    DVDPlusDVDVideoSide,

    /// <summary>
    /// The release was distributed on an Edison Diamond Disc.
    /// </summary>
    EdisonDiamondDisc,

    /// <summary>
    /// The release was distributed on an enhanced compact disc.
    /// </summary>
    EnhancedCD,

    /// <summary>
    /// The release was distributed on a flexi disc.
    /// </summary>
    FlexiDisc,

    /// <summary>
    /// The release was distributed on a floppy disk.
    /// </summary>
    FloppyDisk,

    /// <summary>
    /// The release was distributed on an HD DVD.
    /// </summary>
    HDDVD,

    /// <summary>
    /// The release was distributed on a High Definition Compatible Digital compact disc.
    /// </summary>
    HDCD,

    /// <summary>
    /// The release was distributed on a HiPac.
    /// </summary>
    HiPac,

    /// <summary>
    /// The release was distributed on a High Quality compact disc.
    /// </summary>
    HQCD,

    /// <summary>
    /// The release was distributed on a hybrid Super Audio CD.
    /// </summary>
    HybridSACD,

    /// <summary>
    /// The release was distributed on the CD layer of a hybrid Super Audio CD.
    /// </summary>
    HybridSACDCDLayer,

    /// <summary>
    /// The release was distributed on the SACD layer of a hybrid Super Audio CD.
    /// </summary>
    HybridSACDSACDLayer,

    /// <summary>
    /// The release was distributed on the stereo SACD layer of a hybrid Super Audio CD.
    /// </summary>
    HybridSACDSACDLayer2Channels,

    /// <summary>
    /// The release was distributed on the multichannel SACD layer of a hybrid Super Audio CD.
    /// </summary>
    HybridSACDSACDLayerMultichannel,

    /// <summary>
    /// The release was distributed on a KiT album.
    /// </summary>
    KiTAlbum,

    /// <summary>
    /// The release was distributed on a LaserDisc.
    /// </summary>
    LaserDisc,

    /// <summary>
    /// The release was distributed on a microcassette.
    /// </summary>
    Microcassette,

    /// <summary>
    /// The release was distributed on a microSD card.
    /// </summary>
    MicroSD,

    /// <summary>
    /// The release was distributed on a MiniDisc.
    /// </summary>
    MiniDisc,

    /// <summary>
    /// The release was distributed on a mini DVD.
    /// </summary>
    MiniDVD,

    /// <summary>
    /// The release was distributed on a mini DVD-Audio.
    /// </summary>
    MiniDVDAudio,

    /// <summary>
    /// The release was distributed on a mini DVD-Video.
    /// </summary>
    MiniDVDVideo,

    /// <summary>
    /// The release was distributed on a MiniMax compact disc.
    /// </summary>
    MinimaxCD,

    /// <summary>
    /// The release was distributed on a MiniMax DVD.
    /// </summary>
    MinimaxDVD,

    /// <summary>
    /// The release was distributed on a MiniMax DVD-Audio.
    /// </summary>
    MinimaxDVDAudio,

    /// <summary>
    /// The release was distributed on a MiniMax DVD-Video.
    /// </summary>
    MinimaxDVDVideo,

    /// <summary>
    /// The release was distributed on a mixed-mode compact disc.
    /// </summary>
    MixedModeCD,

    /// <summary>
    /// The medium of the release does not fall into any of the known categories.
    /// </summary>
    Other,

    /// <summary>
    /// The release was distributed on a Pathe disc.
    /// </summary>
    PatheDisc,

    /// <summary>
    /// The release was distributed on a phonograph record.
    /// </summary>
    PhonographRecord,

    /// <summary>
    /// The release was distributed on a piano roll.
    /// </summary>
    PianoRoll,

    /// <summary>
    /// The release was distributed on a Playbutton.
    /// </summary>
    Playbutton,

    /// <summary>
    /// The release was distributed on a PlayTape.
    /// </summary>
    PlayTape,

    /// <summary>
    /// The release was distributed on a reel-to-reel tape.
    /// </summary>
    ReelToReel,

    /// <summary>
    /// The release was distributed on a read-only memory cartridge.
    /// </summary>
    ROMCartridge,

    /// <summary>
    /// The release was distributed on a Super Audio CD.
    /// </summary>
    SACD,

    /// <summary>
    /// The release was distributed on a stereo Super Audio CD.
    /// </summary>
    SACD2Channels,

    /// <summary>
    /// The release was distributed on a multichannel Super Audio CD.
    /// </summary>
    SACDMultichannel,

    /// <summary>
    /// The release was distributed on an SD card.
    /// </summary>
    SDCard,

    /// <summary>
    /// The release was distributed on a shellac disc.
    /// </summary>
    Shellac,

    /// <summary>
    /// The release was distributed on a Super High Material compact disc.
    /// </summary>
    SHMCD,

    /// <summary>
    /// The release was distributed on a Super High Material Super Audio CD.
    /// </summary>
    SHMSACD,

    /// <summary>
    /// The release was distributed on a multichannel Super High Material Super Audio CD.
    /// </summary>
    SHMSACDMultichannel,

    /// <summary>
    /// The release was distributed on a slotMusic card.
    /// </summary>
    SlotMusic,

    /// <summary>
    /// The release was distributed on a Super Video CD.
    /// </summary>
    SVCD,

    /// <summary>
    /// The release was distributed on a Tefifon cartridge.
    /// </summary>
    Tefifon,

    /// <summary>
    /// The release was distributed on a Universal Media Disc.
    /// </summary>
    UMD,

    /// <summary>
    /// The release was distributed on a USB flash drive.
    /// </summary>
    USBFlashDrive,

    /// <summary>
    /// The release was distributed on a Video CD.
    /// </summary>
    VCD,

    /// <summary>
    /// The release was distributed on a Video High Density disc.
    /// </summary>
    VHD,

    /// <summary>
    /// The release was distributed on a VHS videocassette.
    /// </summary>
    VHS,

    /// <summary>
    /// The release was distributed on a vinyl record.
    /// </summary>
    Vinyl,

    /// <summary>
    /// The release was distributed on a VinylDisc.
    /// </summary>
    VinylDisc,

    /// <summary>
    /// The release was distributed on the CD side of a VinylDisc.
    /// </summary>
    VinylDiscCDSide,

    /// <summary>
    /// The release was distributed on the DVD side of a VinylDisc.
    /// </summary>
    VinylDiscDVDSide,

    /// <summary>
    /// The release was distributed on the vinyl side of a VinylDisc.
    /// </summary>
    VinylDiscVinylSide,

    /// <summary>
    /// The release was distributed on a wax cylinder.
    /// </summary>
    WaxCylinder,

    /// <summary>
    /// The release was distributed on a Zip disk.
    /// </summary>
    ZipDisk
}
