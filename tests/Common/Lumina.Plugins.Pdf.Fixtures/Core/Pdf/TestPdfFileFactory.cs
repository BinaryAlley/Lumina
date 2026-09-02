#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
#endregion

namespace Lumina.Plugins.Pdf.Fixtures.Core.Pdf;

/// <summary>
/// Builds a minimal, structurally valid PDF file used as test input for the PDF parsing and rendering tests. The file is produced into a temporary directory
/// at test runtime, because a PDF is a binary format that needs byte-accurate cross-reference offsets.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TestPdfFileFactory
{
    /// <summary>
    /// Creates a minimal single-page PDF with a text layer containing "Hello PDF Page 1".
    /// </summary>
    /// <param name="pdfPath">The path where the PDF is written.</param>
    public static void CreateMinimalPdf(string pdfPath)
    {
        File.WriteAllBytes(pdfPath, BuildMinimalPdf());
    }

    /// <summary>
    /// Creates a two-page PDF with a document outline (bookmarks): a top-level "Chapter One" bookmark that owns a nested
    /// "Part One" bookmark, a top-level "Chapter Two" bookmark, and a top-level "External Link" bookmark that points at a
    /// URI instead of a page, which PdfPig reports as a non-document bookmark.
    /// </summary>
    /// <param name="pdfPath">The path where the PDF is written.</param>
    public static void CreatePdfWithOutline(string pdfPath)
    {
        List<string> objects =
        [
            "<< /Type /Catalog /Pages 2 0 R /Outlines 8 0 R >>",
            "<< /Type /Pages /Kids [3 0 R 4 0 R] /Count 2 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 5 0 R /Resources << /Font << /F1 7 0 R >> >> >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 6 0 R /Resources << /Font << /F1 7 0 R >> >> >>",
            "<< /Length 62 >>\nstream\nBT /F1 24 Tf 72 720 Td (Hello PDF Page 1) Tj ET\nendstream",
            "<< /Length 62 >>\nstream\nBT /F1 24 Tf 72 720 Td (Hello PDF Page 2) Tj ET\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            "<< /Type /Outlines /First 9 0 R /Last 12 0 R /Count 3 >>",
            "<< /Title (Chapter One) /Parent 8 0 R /Dest [3 0 R /Fit] /First 11 0 R /Last 11 0 R /Count 1 /Next 10 0 R >>",
            "<< /Title (Chapter Two) /Parent 8 0 R /Dest [4 0 R /Fit] /Next 12 0 R >>",
            "<< /Title (Part One) /Parent 9 0 R /Dest [3 0 R /Fit] >>",
            "<< /Title (External Link) /Parent 8 0 R /A << /S /URI /URI (https://example.com) >> >>"
        ];

        List<byte> pdfBytes = [];
        AddAscii(pdfBytes, "%PDF-1.4\n");
        List<int> objectOffsets = [];
        for (int index = 0; index < objects.Count; index++)
        {
            objectOffsets.Add(pdfBytes.Count);
            AddAscii(pdfBytes, $"{index + 1} 0 obj\n");
            AddAscii(pdfBytes, objects[index]);
            AddAscii(pdfBytes, "\nendobj\n");
        }

        int xrefOffset = pdfBytes.Count;
        AddAscii(pdfBytes, $"xref\n0 {objects.Count + 1}\n");
        AddAscii(pdfBytes, "0000000000 65535 f \n");
        foreach (int objectOffset in objectOffsets)
            AddAscii(pdfBytes, $"{objectOffset:0000000000} 00000 n \n");

        AddAscii(pdfBytes, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        File.WriteAllBytes(pdfPath, [.. pdfBytes]);
    }

    /// <summary>
    /// Creates a single-page PDF whose text layer spans multiple lines, including a blank line, so the paragraph builder
    /// skips the empty line.
    /// </summary>
    /// <param name="pdfPath">The path where the PDF is written.</param>
    public static void CreatePdfWithBlankLine(string pdfPath)
    {
        // The PDF literal string embeds a real newline followed by an empty line and then the last line, so the text layer
        // of the page contains a blank line that the paragraph builder skips.
        const string TEXT = "(First line\\n\\nThird line) Tj";
        List<string> objects =
        [
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            "<< /Length 58 >>\nstream\nBT /F1 24 Tf 72 720 Td " + TEXT + " ET\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        ];

        List<byte> pdfBytes = [];
        AddAscii(pdfBytes, "%PDF-1.4\n");
        List<int> objectOffsets = [];
        for (int index = 0; index < objects.Count; index++)
        {
            objectOffsets.Add(pdfBytes.Count);
            AddAscii(pdfBytes, $"{index + 1} 0 obj\n");
            AddAscii(pdfBytes, objects[index]);
            AddAscii(pdfBytes, "\nendobj\n");
        }

        int xrefOffset = pdfBytes.Count;
        AddAscii(pdfBytes, $"xref\n0 {objects.Count + 1}\n");
        AddAscii(pdfBytes, "0000000000 65535 f \n");
        foreach (int objectOffset in objectOffsets)
            AddAscii(pdfBytes, $"{objectOffset:0000000000} 00000 n \n");

        AddAscii(pdfBytes, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        File.WriteAllBytes(pdfPath, [.. pdfBytes]);
    }

    /// <summary>
    /// Creates a single-page PDF whose catalog references a malformed outline dictionary, so reading its bookmarks throws.
    /// </summary>
    /// <param name="pdfPath">The path where the PDF is written.</param>
    public static void CreatePdfWithMalformedOutline(string pdfPath)
    {
        List<string> objects =
        [
            "<< /Type /Catalog /Pages 2 0 R /Outlines 8 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            "<< /Length 57 >>\nstream\nBT /F1 24 Tf 72 720 Td (Hello PDF Page 1) Tj ET\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            // The outline root claims a first child that does not exist.
            "<< /Type /Outlines /First 99 0 R /Last 99 0 R /Count 1 >>"
        ];

        List<byte> pdfBytes = [];
        AddAscii(pdfBytes, "%PDF-1.4\n");
        List<int> objectOffsets = [];
        for (int index = 0; index < objects.Count; index++)
        {
            objectOffsets.Add(pdfBytes.Count);
            AddAscii(pdfBytes, $"{index + 1} 0 obj\n");
            AddAscii(pdfBytes, objects[index]);
            AddAscii(pdfBytes, "\nendobj\n");
        }

        int xrefOffset = pdfBytes.Count;
        AddAscii(pdfBytes, $"xref\n0 {objects.Count + 1}\n");
        AddAscii(pdfBytes, "0000000000 65535 f \n");
        foreach (int objectOffset in objectOffsets)
            AddAscii(pdfBytes, $"{objectOffset:0000000000} 00000 n \n");

        AddAscii(pdfBytes, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        File.WriteAllBytes(pdfPath, [.. pdfBytes]);
    }

    /// <summary>
    /// Creates a PDF with more pages than the parser's page cap, so the page enumeration stops at the cap.
    /// </summary>
    /// <param name="pdfPath">The path where the PDF is written.</param>
    public static void CreatePdfWithMorePagesThanAllowed(string pdfPath)
    {
        const int PAGE_COUNT = 10_001;
        List<string> pageObjects = [];
        for (int index = 0; index < PAGE_COUNT; index++)
        {
            int objectNumber = 3 + index;
            // Every page after the first shares the font, but each page needs its own empty content stream to stay valid.
            pageObjects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] >>");
        }

        string kids = string.Join(' ', Enumerable.Range(0, PAGE_COUNT).Select(index => $"{3 + index} 0 R"));

        List<string> objects =
        [
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{kids}] /Count {PAGE_COUNT} >>",
            .. pageObjects
        ];

        List<byte> pdfBytes = [];
        AddAscii(pdfBytes, "%PDF-1.4\n");
        List<int> objectOffsets = [];
        for (int index = 0; index < objects.Count; index++)
        {
            objectOffsets.Add(pdfBytes.Count);
            AddAscii(pdfBytes, $"{index + 1} 0 obj\n");
            AddAscii(pdfBytes, objects[index]);
            AddAscii(pdfBytes, "\nendobj\n");
        }

        int xrefOffset = pdfBytes.Count;
        AddAscii(pdfBytes, $"xref\n0 {objects.Count + 1}\n");
        AddAscii(pdfBytes, "0000000000 65535 f \n");
        foreach (int objectOffset in objectOffsets)
            AddAscii(pdfBytes, $"{objectOffset:0000000000} 00000 n \n");

        AddAscii(pdfBytes, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        File.WriteAllBytes(pdfPath, [.. pdfBytes]);
    }

    /// <summary>
    /// Builds the bytes of a minimal single-page PDF with an accurate cross-reference table.
    /// </summary>
    /// <returns>The bytes of the PDF.</returns>
    private static byte[] BuildMinimalPdf()
    {
        const string TEXT = "(Hello PDF Page 1)";
        List<string> objects =
        [
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            "<< /Length 57 >>\nstream\nBT /F1 24 Tf 72 720 Td " + TEXT + " Tj ET\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        ];

        List<byte> pdfBytes = [];
        AddAscii(pdfBytes, "%PDF-1.4\n");
        List<int> objectOffsets = [];
        for (int index = 0; index < objects.Count; index++)
        {
            objectOffsets.Add(pdfBytes.Count);
            AddAscii(pdfBytes, $"{index + 1} 0 obj\n");
            AddAscii(pdfBytes, objects[index]);
            AddAscii(pdfBytes, "\nendobj\n");
        }

        int xrefOffset = pdfBytes.Count;
        AddAscii(pdfBytes, $"xref\n0 {objects.Count + 1}\n");
        AddAscii(pdfBytes, "0000000000 65535 f \n");
        foreach (int objectOffset in objectOffsets)
            AddAscii(pdfBytes, $"{objectOffset:0000000000} 00000 n \n");

        AddAscii(pdfBytes, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        return [.. pdfBytes];
    }

    /// <summary>
    /// Appends ASCII text to the byte list.
    /// </summary>
    /// <param name="bytes">The byte list to append to.</param>
    /// <param name="text">The ASCII text to append.</param>
    private static void AddAscii(List<byte> bytes, string text)
    {
        foreach (byte character in Encoding.ASCII.GetBytes(text))
            bytes.Add(character);
    }
}
