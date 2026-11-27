namespace Nemera.Presentation.Web.Common.Http;

/// <summary>
/// Marks an endpoint whose malformed JSON request bodies are answered with a client error ProblemDetails instead of the default JSON error envelope.
/// </summary>
public sealed class JsonBindingFailureClientErrorMetadata
{
}
