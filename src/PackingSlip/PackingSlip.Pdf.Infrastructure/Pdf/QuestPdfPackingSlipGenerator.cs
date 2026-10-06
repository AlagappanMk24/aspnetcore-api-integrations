using System.Reflection;
using PackingSlip.Pdf.Application.Contracts;
using PackingSlip.Pdf.Domain.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PackingSlip.Pdf.Infrastructure.Pdf;

public sealed class QuestPdfPackingSlipGenerator : IPackingSlipPdfGenerator
{
    private const float PageMargin = 30f;
    private const float BaseFontSize = 11.5f;
    private const string TextColor = "#171717";
    private const string HeadingColor = "#263140";
    private const string RuleColor = "#D7DCE1";

    public byte[] Generate(Domain.Models.PackingSlip model)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(PageMargin);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style
                    .FontFamily(Fonts.Lato)
                    .FontSize(BaseFontSize)
                    .FontColor(TextColor));

                page.Content().Element(content => ComposePage(content, model));

                // Essential Feature: Page numbering in footer
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposePage(IContainer container, Domain.Models.PackingSlip model)
    {
        container.Column(column =>
        {
            column.Spacing(0);

            // Title Block
            column.Item().Text("Packing slip")
                .FontFamily(Fonts.Lato)
                .Bold()
                .FontSize(28)
                .FontColor(HeadingColor)
                .LineHeight(1.05f);

            // Header Row: Logo & Right-Aligned Order Metadata
            column.Item().PaddingTop(16).Row(row =>
            {
                row.RelativeItem(1).AlignLeft().AlignTop().Element(c => ComposeLogo(c, model));
                row.RelativeItem(1).AlignRight().AlignTop().Element(c => ComposeOrderDetails(c, model));
            });

            column.Item().Height(40);

            // 3-Column Address Row with Equal Widths & Spacing
            column.Item().Row(row =>
            {
                row.Spacing(20);
                row.RelativeItem(1).Element(c => ComposeFrom(c, model.From));
                row.RelativeItem(1).Element(c => ComposeBillTo(c, model.BillTo));
                row.RelativeItem(1).Element(c => ComposeShipTo(c, model.ShipTo));
            });

            column.Item().Height(35);

            // Item Table with Correct Text & Numeric Alignment
            column.Item().Element(c => ComposeItems(c, model.Items));

            column.Item().PaddingTop(27).Element(c => ComposeThankYou(c, model));
        });
    }

    private static void ComposeLogo(IContainer container, Domain.Models.PackingSlip model)
    {
        var logoBytes = TryGetLogo(model.LogoBase64) ?? GetEmbeddedReferenceLogo();

        if (logoBytes is not null)
        {
            container
                 .MaxWidth(180)
                 .MaxHeight(80)
                 .AlignLeft()
                 .AlignTop()
                 .Image(logoBytes)
                 .FitArea();
            return;
        }
    }

    private static void ComposeOrderDetails(IContainer container, Domain.Models.PackingSlip model)
    {
        container.Width(220).Column(column =>
        {
            AddLabelValueRow(column, "Order No.:", model.OrderNumber);
            AddLabelValueRow(column, "Order Date:", model.OrderDate.ToString("MM-dd-yyyy"));
            AddLabelValueRow(column, "Shipping Method:", model.ShippingMethod);
        });
    }

    private static void AddLabelValueRow(ColumnDescriptor column, string label, string value)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem().Text(label).Bold().FontSize(11.5f);
            row.AutoItem().Text(value).FontSize(11.5f);
        });
    }

    private static void ComposeFrom(IContainer container, Address address)
    {
        ComposeAddress(container, "From", address, includeContact: false);
    }

    private static void ComposeBillTo(IContainer container, Address address)
    {
        ComposeAddress(container, "Bill to", address, includeContact: true);
    }

    private static void ComposeShipTo(IContainer container, Address address)
    {
        ComposeAddress(container, "Ship to", address, includeContact: false);
    }

    private static void ComposeAddress(IContainer container, string heading, Address address, bool includeContact)
    {
        container.Column(column =>
        {
            column.Item().Text(heading)
                .Bold()
                .FontSize(13.5f)
                .LineHeight(1.1f);

            column.Item().PaddingTop(9).Text(address.Name).FontSize(11.5f).LineHeight(1.35f);
            column.Item().Text(address.AddressLine1).FontSize(11.5f).LineHeight(1.35f);

            if (!string.IsNullOrWhiteSpace(address.AddressLine2))
                column.Item().Text(address.AddressLine2).FontSize(11.5f).LineHeight(1.35f);

            column.Item().Text($"{address.City}, {address.State}, {address.PostalCode}")
                .FontSize(11.5f).LineHeight(1.35f);
            column.Item().Text(address.Country).FontSize(11.5f).LineHeight(1.35f);

            if (!includeContact)
                return;

            if (!string.IsNullOrWhiteSpace(address.Email))
                AddInlineContact(column, "Email:", address.Email!);

            if (!string.IsNullOrWhiteSpace(address.Phone))
                AddInlineContact(column, "Phone:", address.Phone!);

            if (!string.IsNullOrWhiteSpace(address.Vat))
                AddInlineContact(column, "VAT:", address.Vat!);
        });
    }

    private static void AddInlineContact(ColumnDescriptor column, string label, string value)
    {
        column.Item().Text(text =>
        {
            text.DefaultTextStyle(style => style.FontSize(11.5f).LineHeight(1.28f));
            text.Span(label + " ").Bold();
            text.Span(value);
        });
    }

    private static void ComposeItems(IContainer container, IReadOnlyCollection<PackingSlipItem> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(50);   // S.No
                columns.ConstantColumn(110);  // SKU
                columns.RelativeColumn();     // Product Name
                columns.ConstantColumn(90);   // Quantity
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).AlignCenter().Text("S.No");
                header.Cell().Element(HeaderCell).AlignLeft().Text("SKU");
                header.Cell().Element(HeaderCell).AlignLeft().Text("Product");
                header.Cell().Element(HeaderCell).AlignRight().Text("Quantity");
            });

            var index = 1;
            var totalQuantity = 0;

            foreach (var item in items)
            {
                totalQuantity += item.Quantity;

                table.Cell().Element(BodyCell).AlignCenter().Text(index++.ToString());
                table.Cell().Element(BodyCell).AlignLeft().Text(item.Sku);
                table.Cell().Element(BodyCell).AlignLeft().Text(item.ProductName);
                table.Cell().Element(BodyCell).AlignRight().Text(item.Quantity.ToString("N0"));
            }

            // Summary Row: Total quantity count (Essential for warehouse verification)
            table.Cell().ColumnSpan(3).Element(FooterCell).AlignRight().Text("Total Quantity:").Bold();
            table.Cell().Element(FooterCell).AlignRight().Text(totalQuantity.ToString("N0")).Bold();
        });

        static IContainer HeaderCell(IContainer container) => container
            .BorderBottom(1f)
            .BorderColor(HeadingColor)
            .PaddingLeft(4)
            .PaddingRight(4)
            .PaddingBottom(6)
            .DefaultTextStyle(style => style.Bold().FontSize(11.5f));

        static IContainer BodyCell(IContainer container) => container
            .BorderBottom(0.65f)
            .BorderColor(RuleColor)
            .PaddingLeft(4)
            .PaddingRight(4)
            .PaddingTop(7)
            .PaddingBottom(7)
            .DefaultTextStyle(style => style.FontSize(11.5f));

        static IContainer FooterCell(IContainer container) => container
            .PaddingLeft(4)
            .PaddingRight(4)
            .PaddingTop(8)
            .PaddingBottom(8)
            .DefaultTextStyle(style => style.FontSize(11.5f));
    }

    private static void ComposeThankYou(IContainer container, Domain.Models.PackingSlip model)
    {
        var text = string.IsNullOrWhiteSpace(model.Notes)
            ? "Thank you for your order! We’ve carefully packed your items and hope everything arrives just as expected. If you have any questions or concerns, feel free to reach out—we’re here to help!"
            : model.Notes;

        container.Text(text)
            .FontSize(11.5f)
            .LineHeight(1.45f);
    }

    private static void ComposeFooter(IContainer container)
    {
        container.PaddingTop(10).Row(row =>
        {
            row.RelativeItem().Text($"Printed: {DateTime.Now:MM-dd-yyyy HH:mm}")
                .FontSize(9f)
                .FontColor("#737373");

            row.RelativeItem().AlignRight().Text(text =>
            {
                text.DefaultTextStyle(style => style.FontSize(9f).FontColor("#737373"));
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
        });
    }

    private static byte[]? TryGetLogo(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return null;

        try
        {
            var value = base64;
            var comma = value.IndexOf(',');
            if (comma >= 0)
                value = value[(comma + 1)..];

            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static byte[]? GetEmbeddedReferenceLogo()
    {
        var assembly = typeof(QuestPdfPackingSlipGenerator).Assembly;
        const string resourceName = "PackingSlip.Pdf.Infrastructure.Pdf.Infotech-Logo.png";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return null;

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
