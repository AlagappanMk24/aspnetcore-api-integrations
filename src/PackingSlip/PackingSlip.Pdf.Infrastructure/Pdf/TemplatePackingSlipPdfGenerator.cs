//using System.Globalization;
//using PackingSlip.Pdf.Application.Contracts;
//using PackingSlip.Pdf.Domain.Models;
//using PdfSharpCore.Drawing;
//using PdfSharpCore.Pdf;
//using PdfSharpCore.Pdf.IO;

//namespace PackingSlip.Pdf.Infrastructure.Pdf;

///// <summary>
///// Binds the request payload onto the cleaned commercial PDF template.
///// The original sample values are removed from the template before binding,
///// while the original template artwork, labels and table borders are preserved.
///// </summary>
//public sealed class TemplatePackingSlipPdfGenerator : ITemplatePackingSlipPdfGenerator
//{
//    private const string TemplateResourceName = "PackingSlip.Pdf.Infrastructure.Templates.Packing-Slip.pdf";

//    public byte[] Generate(TemplatePackingSlip model)
//    {
//        if (model.Items.Count > 7)
//            throw new ArgumentException("The supplied commercial template supports a maximum of 7 item rows per page.", nameof(model));

//        using var templateStream = OpenTemplate();
//        using var document = PdfReader.Open(templateStream, PdfDocumentOpenMode.Modify);

//        if (document.PageCount == 0)
//            throw new InvalidOperationException("The commercial packing slip template does not contain a PDF page.");

//        var page = document.Pages[0];
//        using var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

//        var regular = new XFont("Arial", 10, XFontStyle.Regular);

//        // Header contact values. Static labels such as "Phone:" and "Email:" remain in the template.
//        DrawValue(graphics, model.Company.Phone, new XRect(390, 53, 152, 20), regular, XStringFormats.TopRight);
//        DrawValue(graphics, model.Company.Email, new XRect(408, 74, 134, 20), regular, XStringFormats.TopRight);
//        DrawValue(graphics, model.Company.Website, new XRect(414, 96, 128, 20), regular, XStringFormats.TopRight);

//        // Order metadata. Static labels and underline rules remain in the template.
//        DrawValue(graphics, model.InvoiceNumber, new XRect(232, 169, 60, 22), regular, XStringFormats.TopRight);
//        DrawValue(graphics, model.OrderNumber, new XRect(232, 206, 60, 22), regular, XStringFormats.TopRight);
//        DrawValue(graphics, model.OrderDate.ToString("MMMM d,yyyy", CultureInfo.InvariantCulture), new XRect(430, 169, 112, 22), regular, XStringFormats.TopRight);
//        DrawValue(graphics, model.DeliveryMethod, new XRect(445, 206, 97, 22), regular, XStringFormats.TopRight);

//        // Address blocks. The gray title bars are part of the source template.
//        DrawAddress(graphics, model.BillTo, x: 55, y: 302, width: 230, regular);
//        DrawAddress(graphics, model.ShipTo, x: 313, y: 302, width: 228, regular);

//        // The template contains exactly seven item rows.
//        const double firstRowY = 438;
//        const double rowHeight = 30.2;
//        for (var i = 0; i < 7; i++)
//        {
//            var item = i < model.Items.Count ? model.Items.ElementAt(i) : null;
//            if (item is null)
//                continue;

//            var rowY = firstRowY + (i * rowHeight);
//            DrawValue(graphics, item.ItemNumber, new XRect(84, rowY, 58, 25), regular, XStringFormats.Center);
//            DrawValue(graphics, item.Description, new XRect(159, rowY, 276, 25), regular, XStringFormats.CenterLeft);
//            DrawValue(graphics, item.Quantity.ToString(CultureInfo.InvariantCulture), new XRect(464, rowY, 54, 25), regular, XStringFormats.Center);
//        }

//        // Footer messages are dynamic; the sample footer text has already been removed from the clean template.
//        if (!string.IsNullOrWhiteSpace(model.FooterContactMessage))
//            DrawValue(graphics, model.FooterContactMessage, new XRect(55, 721, 250, 40), regular, XStringFormats.TopLeft);

//        if (!string.IsNullOrWhiteSpace(model.FooterThankYouMessage))
//            DrawValue(graphics, model.FooterThankYouMessage, new XRect(375, 721, 167, 40), regular, XStringFormats.TopRight);

//        using var output = new MemoryStream();
//        document.Save(output, false);
//        return output.ToArray();
//    }

//    private static Stream OpenTemplate()
//    {
//        var assembly = typeof(TemplatePackingSlipPdfGenerator).Assembly;
//        return assembly.GetManifestResourceStream(TemplateResourceName)
//            ?? throw new InvalidOperationException($"Embedded commercial packing slip template not found: {TemplateResourceName}");
//    }

//    private static void DrawAddress(XGraphics graphics, TemplateAddress address, double x, double y, double width, XFont font)
//    {
//        DrawValue(graphics, address.Name, new XRect(x, y, width, 18), font, XStringFormats.TopLeft);
//        DrawValue(graphics, address.AddressLine1, new XRect(x, y + 18, width, 18), font, XStringFormats.TopLeft);
//        DrawValue(graphics, address.CityStateZip, new XRect(x, y + 36, width, 18), font, XStringFormats.TopLeft);
//        DrawValue(graphics, address.EmailAndPhone ?? string.Empty, new XRect(x, y + 54, width, 18), font, XStringFormats.TopLeft);
//    }

//    private static void DrawValue(XGraphics graphics, string value, XRect area, XFont font, XStringFormat format)
//    {
//        if (string.IsNullOrWhiteSpace(value))
//            return;

//        graphics.DrawString(value, font, XBrushes.Black, area, format);
//    }
//}

using System.Globalization;
using PackingSlip.Pdf.Application.Contracts;
using PackingSlip.Pdf.Domain.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf.IO;

namespace PackingSlip.Pdf.Infrastructure.Pdf;

/// <summary>
/// Binds the request payload onto the cleaned commercial PDF template with absolute alignment safeguards.
/// </summary>
public sealed class TemplatePackingSlipPdfGenerator : ITemplatePackingSlipPdfGenerator
{
    public byte[] Generate(TemplatePackingSlip model)
    {
        if (model.Items.Count > 7)
            throw new ArgumentException("The supplied commercial template supports a maximum of 7 item rows per page.", nameof(model));

        using var templateStream = OpenTemplate();
        using var document = PdfReader.Open(templateStream, PdfDocumentOpenMode.Modify);

        if (document.PageCount == 0)
            throw new InvalidOperationException("The commercial packing slip template does not contain a PDF page.");

        var page = document.Pages[0];
        using var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

        var fontRegular = new XFont("Arial", 9.5, XFontStyle.Regular);
        var fontSmall = new XFont("Arial", 8.5, XFontStyle.Regular);

        // -----------------------------------------------------------------
        // 1. HEADER CONTACT VALUES (Anchored Right Page Margin = 540)
        // -----------------------------------------------------------------
        DrawValue(graphics, model.Company.Phone, new XRect(380, 56, 160, 15), fontRegular, XStringFormats.TopRight);
        DrawValue(graphics, model.Company.Email, new XRect(380, 77, 160, 15), fontRegular, XStringFormats.TopRight);
        DrawValue(graphics, model.Company.Website, new XRect(380, 98, 160, 15), fontRegular, XStringFormats.TopRight);

        // -----------------------------------------------------------------
        // 2. ORDER METADATA (Vertically centered & right-aligned to underline rules)
        // -----------------------------------------------------------------
        // Left Column Underline Rule: X = 55 to 285 (Width = 230)
        DrawValue(graphics, model.InvoiceNumber, new XRect(55, 170, 230, 20), fontRegular, XStringFormats.CenterRight);
        DrawValue(graphics, model.OrderNumber, new XRect(55, 207, 230, 20), fontRegular, XStringFormats.CenterRight);

        // Right Column Underline Rule: X = 313 to 541 (Width = 228)
        var formattedDate = model.OrderDate.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);
        DrawValue(graphics, formattedDate, new XRect(313, 170, 228, 20), fontRegular, XStringFormats.CenterRight);
        DrawValue(graphics, model.DeliveryMethod, new XRect(313, 207, 228, 20), fontRegular, XStringFormats.CenterRight);

        // -----------------------------------------------------------------
        // 3. ADDRESS BLOCKS
        // -----------------------------------------------------------------
        DrawAddress(graphics, model.BillTo, x: 55, y: 300, width: 230, fontRegular);
        DrawAddress(graphics, model.ShipTo, x: 313, y: 300, width: 228, fontRegular);

        // -----------------------------------------------------------------
        // 4. ITEMS TABLE ROWS (Strict Table Bounds & Center-Vertical Alignment)
        // -----------------------------------------------------------------
        const double firstRowY = 438;
        const double rowHeight = 30.2;

        for (var i = 0; i < model.Items.Count; i++)
        {
            var item = model.Items.ElementAt(i);
            var rowY = firstRowY + (i * rowHeight);

            // Item Number Column (X: 55 to 135)
            DrawValue(graphics, item.ItemNumber, new XRect(55, rowY, 80, rowHeight), fontRegular, XStringFormats.Center);

            // Product Description Column (X: 145 to 425) — Trimmed to fit without spillage
            var boundedDescription = FitTextToWidth(graphics, item.Description, 280, fontRegular);
            DrawValue(graphics, boundedDescription, new XRect(145, rowY, 280, rowHeight), fontRegular, XStringFormats.CenterLeft);

            // Quantity Column (X: 445 to 515)
            DrawValue(graphics, item.Quantity.ToString(CultureInfo.InvariantCulture), new XRect(445, rowY, 70, rowHeight), fontRegular, XStringFormats.CenterRight);
        }

        // -----------------------------------------------------------------
        // 5. FOOTER MESSAGES
        // -----------------------------------------------------------------
        // Contact Message (Centered across full printable width: X = 55 to 540)
        if (!string.IsNullOrWhiteSpace(model.FooterContactMessage))
            DrawWrappedText(graphics, model.FooterContactMessage, new XRect(55, 715, 485, 30), fontSmall);

        // Thank You Message (Centered directly below contact message or at page bottom)
        if (!string.IsNullOrWhiteSpace(model.FooterThankYouMessage))
            DrawValue(graphics, model.FooterThankYouMessage, new XRect(55, 745, 485, 20), fontSmall, XStringFormats.TopCenter);

        using var output = new MemoryStream();
        document.Save(output, false);
        return output.ToArray();
    }

    private static Stream OpenTemplate()
    {
        var assembly = typeof(TemplatePackingSlipPdfGenerator).Assembly;

        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("Packing-Slip.pdf", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
            throw new InvalidOperationException("Embedded Packing-Slip.pdf template could not be located in assembly manifest.");

        return assembly.GetManifestResourceStream(resourceName)!;
    }

    private static void DrawAddress(XGraphics graphics, TemplateAddress address, double x, double y, double width, XFont font)
    {
        var currentY = y;
        const double lineSpacing = 16.0;

        DrawValue(graphics, address.Name, new XRect(x, currentY, width, lineSpacing), font, XStringFormats.TopLeft);
        currentY += lineSpacing;

        DrawValue(graphics, address.AddressLine1, new XRect(x, currentY, width, lineSpacing), font, XStringFormats.TopLeft);
        currentY += lineSpacing;

        DrawValue(graphics, address.CityStateZip, new XRect(x, currentY, width, lineSpacing), font, XStringFormats.TopLeft);
        currentY += lineSpacing;

        if (!string.IsNullOrWhiteSpace(address.EmailAndPhone))
            DrawValue(graphics, address.EmailAndPhone, new XRect(x, currentY, width, lineSpacing), font, XStringFormats.TopLeft);
    }

    private static void DrawValue(XGraphics graphics, string? value, XRect area, XFont font, XStringFormat format)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        graphics.DrawString(value, font, XBrushes.Black, area, format);
    }

    private static void DrawWrappedText(XGraphics graphics, string text, XRect area, XFont font, bool center = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentLine = "";
        var currentY = area.Y;
        var lineSpacing = font.GetHeight() * 1.25;

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            var size = graphics.MeasureString(testLine, font);

            if (size.Width > area.Width && !string.IsNullOrEmpty(currentLine))
            {
                DrawLine(graphics, currentLine, area, currentY, font, center);
                currentLine = word;
                currentY += lineSpacing;

                if (currentY + font.Height > area.Y + area.Height)
                    break;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine) && currentY + font.Height <= area.Y + area.Height)
        {
            DrawLine(graphics, currentLine, area, currentY, font, center);
        }
    }

    private static void DrawLine(XGraphics graphics, string line, XRect area, double y, XFont font, bool center)
    {
        var lineSize = graphics.MeasureString(line, font);
        var x = center ? area.X + (area.Width - lineSize.Width) / 2.0 : area.X;
        graphics.DrawString(line, font, XBrushes.Black, new XPoint(x, y + font.Height));
    }

    private static string FitTextToWidth(XGraphics graphics, string text, double maxWidth, XFont font)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var size = graphics.MeasureString(text, font);
        if (size.Width <= maxWidth)
            return text;

        const string ellipsis = "...";
        var length = text.Length;

        while (length > 0 && graphics.MeasureString(text[..length] + ellipsis, font).Width > maxWidth)
        {
            length--;
        }

        return text[..length] + ellipsis;
    }
}