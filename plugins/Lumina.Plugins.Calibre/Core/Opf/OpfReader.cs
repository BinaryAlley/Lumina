#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Calibre.Common.Models.DTO.Opf;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
#endregion

namespace Lumina.Plugins.Calibre.Core.Opf;

/// <summary>
/// Reads the metadata of a book from a Calibre OPF file.
/// </summary>
internal static class OpfReader
{
    private const long MAX_OPF_FILE_SIZE_BYTES = 5 * 1024 * 1024;
    private static readonly XNamespace s_opfNamespace = "http://www.idpf.org/2007/opf";
    private static readonly XNamespace s_dcNamespace = "http://purl.org/dc/elements/1.1/";

    /// <summary>
    /// Reads the metadata of the book from the OPF file at <paramref name="opfFilePath"/>.
    /// </summary>
    /// <param name="opfFilePath">The file system path of the OPF file to read.</param>
    /// <returns>The metadata read from the OPF file, with the fields that could not be read left empty.</returns>
    public static OpfDocumentDto Read(string opfFilePath)
    {
        OpfDocumentDto document = new();
        FileInfo fileInfo = new(opfFilePath);
        if (!fileInfo.Exists || fileInfo.Length > MAX_OPF_FILE_SIZE_BYTES)
            return document;

        try
        {
            using FileStream fileStream = File.OpenRead(opfFilePath);
            // the OPF file is untrusted XML, so DTD processing and external entity resolution are disabled, and the document size is bounded
            XmlReaderSettings xmlReaderSettings = new()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = MAX_OPF_FILE_SIZE_BYTES,
                MaxCharactersFromEntities = 0
            };
            using XmlReader xmlReader = XmlReader.Create(fileStream, xmlReaderSettings);
            XDocument xDocument = XDocument.Load(xmlReader, LoadOptions.None);
            Parse(xDocument, document);
        }
        catch (XmlException)
        {
            // a malformed OPF file must not prevent the enrichment of the book from proceeding
        }

        return document;
    }

    /// <summary>
    /// Parses the metadata elements of the <paramref name="xDocument"/> into the <paramref name="document"/>.
    /// </summary>
    /// <param name="xDocument">The parsed OPF document.</param>
    /// <param name="document">The model onto which the parsed metadata is written.</param>
    private static void Parse(XDocument xDocument, OpfDocumentDto document)
    {
        XElement? metadata = xDocument.Root?.Element(s_opfNamespace + "metadata");
        if (metadata is null)
            return;

        document.Title = GetElementText(metadata, s_dcNamespace + "title");
        document.Description = GetElementText(metadata, s_dcNamespace + "description");
        document.Publisher = GetElementText(metadata, s_dcNamespace + "publisher");
        document.LanguageCode = GetElementText(metadata, s_dcNamespace + "language");
        document.PublishDate = ParseDate(GetElementText(metadata, s_dcNamespace + "date"));

        foreach (XElement identifier in metadata.Elements(s_dcNamespace + "identifier"))
        {
            string? scheme = identifier.Attribute(s_opfNamespace + "scheme")?.Value;
            if (!string.IsNullOrWhiteSpace(scheme) && !string.IsNullOrWhiteSpace(identifier.Value))
                document.Identifiers.Add(new OpfIdentifierDto(Scheme: scheme.Trim(), Value: identifier.Value.Trim()));
        }

        foreach (XElement subject in metadata.Elements(s_dcNamespace + "subject"))
            if (!string.IsNullOrWhiteSpace(subject.Value))
                document.Subjects.Add(subject.Value.Trim());

        foreach (XElement creator in metadata.Elements(s_dcNamespace + "creator"))
            if (!string.IsNullOrWhiteSpace(creator.Value))
                document.Creators.Add(new OpfCreatorDto(Name: creator.Value.Trim(), Role: creator.Attribute(s_opfNamespace + "role")?.Value?.Trim()));

        foreach (XElement contributor in metadata.Elements(s_dcNamespace + "contributor"))
            if (!string.IsNullOrWhiteSpace(contributor.Value))
                document.Contributors.Add(new OpfContributorDto(Name: contributor.Value.Trim(), Role: contributor.Attribute(s_opfNamespace + "role")?.Value?.Trim()));

        foreach (XElement meta in metadata.Elements(s_opfNamespace + "meta"))
        {
            string? name = meta.Attribute("name")?.Value;
            string? content = meta.Attribute("content")?.Value;
            if (name is null || content is null)
                continue;

            if (string.Equals(name, "calibre:series", StringComparison.OrdinalIgnoreCase))
                document.Series = content.Trim();
            else if (string.Equals(name, "calibre:series_index", StringComparison.OrdinalIgnoreCase))
                document.SeriesIndex = ParseDouble(content);
            else if (string.Equals(name, "calibre:rating", StringComparison.OrdinalIgnoreCase))
                document.Rating = ParseInt(content);
        }

        XElement? coverReference = xDocument.Root
            ?.Element(s_opfNamespace + "guide")
            ?.Elements(s_opfNamespace + "reference")
            .FirstOrDefault(reference => string.Equals(reference.Attribute("type")?.Value, "cover", StringComparison.OrdinalIgnoreCase));
        document.CoverHref = coverReference?.Attribute("href")?.Value;
    }

    /// <summary>
    /// Gets the trimmed text of the child element with the specified <paramref name="name"/> of the <paramref name="parent"/> element.
    /// </summary>
    /// <param name="parent">The parent element.</param>
    /// <param name="name">The name of the child element.</param>
    /// <returns>The trimmed text, or <see langword="null"/> when the element is missing or empty.</returns>
    private static string? GetElementText(XElement parent, XName name)
    {
        string? value = parent.Element(name)?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Parses a date string into a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="value">The date string to parse.</param>
    /// <returns>The parsed date, or <see langword="null"/> when the string is empty or cannot be parsed.</returns>
    private static DateTimeOffset? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return DateTimeOffset.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTimeOffset result) ? result : null;
    }

    /// <summary>
    /// Parses a double string.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed value, or <see langword="null"/> when the string is empty or cannot be parsed.</returns>
    private static double? ParseDouble(string value)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result) ? result : null;
    }

    /// <summary>
    /// Parses an integer string.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed value, or <see langword="null"/> when the string is empty or cannot be parsed.</returns>
    private static int? ParseInt(string value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : null;
    }
}
