namespace Lumina.Presentation.Web.Common.Enums.MediaConsumption;

/// <summary>
/// Enumeration for the states of a media consumption session.
/// </summary>
public enum ConsumptionStatus
{
    /// <summary>
    /// The consumption session started and has not ended yet.
    /// </summary>
    InProgress,

    /// <summary>
    /// The consumption session reached its explicit completion or the completion threshold of its library.
    /// </summary>
    Completed,

    /// <summary>
    /// The consumption session ended before it completed.
    /// </summary>
    Stopped
}
