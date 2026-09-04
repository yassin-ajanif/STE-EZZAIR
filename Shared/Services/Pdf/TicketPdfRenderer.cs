using GestionCommerciale.Shared.Models.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace GestionCommerciale.Shared.Services.Pdf;

/// <summary>Thermal ticket layout (58/80 mm). Independent from A4 <see cref="CommercialDocumentPdfRenderer"/>.</summary>
public static class TicketPdfRenderer
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public static byte[] Render(TicketDocumentPdfModel model)
    {
        if (model.WidthMm is not (58f or 80f))
            throw new ArgumentOutOfRangeException(nameof(model), "Ticket width must be 58 or 80 mm.");

        var dash = model.WidthMm >= 80f ? new string('-', 42) : new string('-', 32);
        // Near-full width: keep a small inset so the logo doesn't touch the paper edges.
        var logoSideInset = model.WidthMm >= 80f ? 2f : 1.5f;
        var logoMaxHeight = model.WidthMm >= 80f ? 42f : 32f;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Height follows content — avoids the empty white strip at the bottom of thermal tickets.
                page.ContinuousSize(model.WidthMm, Unit.Millimetre);
                page.MarginHorizontal(4);
                page.MarginVertical(6);
                page.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Black));

                page.Content().Column(col =>
                {
                    DrawLogo(col, model, logoSideInset, logoMaxHeight);

                    col.Item().PaddingTop(4).AlignCenter().Text(dash).FontSize(7);
                    col.Item().PaddingTop(4).AlignCenter()
                        .Text(model.DocumentKindLabel).Bold().FontSize(11);
                    col.Item().PaddingTop(6).Text($"N° : {model.Numero}").FontSize(8);
                    col.Item().Text($"Date : {model.DateText}").FontSize(8);
                    if (!string.IsNullOrWhiteSpace(model.ExtraInfoLabel) && !string.IsNullOrWhiteSpace(model.ExtraInfoValue))
                        col.Item().Text($"{model.ExtraInfoLabel} : {model.ExtraInfoValue}").FontSize(8);
                    col.Item().Text($"{model.PartyLabel} : {model.PartyName}").FontSize(8);
                    col.Item().PaddingTop(6);

                    DrawLinesTable(col, model);

                    col.Item().PaddingVertical(4).AlignCenter().Text(dash).FontSize(7);
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL").Bold().FontSize(9);
                        r.AutoItem().Text($"{FmtMoney(model.Total)} {model.Devise}").Bold().FontSize(9);
                    });
                    col.Item().PaddingVertical(4).AlignCenter().Text(dash).FontSize(7);
                    col.Item().AlignCenter().Text(model.FooterMessage).FontSize(8);

                    DrawCompanyInfoFooter(col, model);
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static void DrawLinesTable(ColumnDescriptor col, TicketDocumentPdfModel model)
    {
        // ~45% article / rest for numbers (mm) so Prix/QTE/Montant never wrap on 58/80.
        var is80 = model.WidthMm >= 80f;
        var prixW = is80 ? 16f : 13f;
        var qteW = is80 ? 9f : 7f;
        var montantW = is80 ? 17f : 14f;
        var headerSize = is80 ? 7f : 6.5f;
        var cellSize = is80 ? 7f : 6.5f;

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(prixW, Unit.Millimetre);
                columns.ConstantColumn(qteW, Unit.Millimetre);
                columns.ConstantColumn(montantW, Unit.Millimetre);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).AlignLeft().Text("Article").FontSize(headerSize);
                header.Cell().Element(HeaderCell).AlignRight().Text("Prix").FontSize(headerSize);
                header.Cell().Element(HeaderCell).AlignRight().Text("QTE").FontSize(headerSize);
                header.Cell().Element(HeaderCell).AlignRight().Text("Montant").FontSize(headerSize);
            });

            foreach (var line in model.Lines)
            {
                table.Cell().Element(BodyCell).AlignLeft()
                    .Text(line.Designation).FontSize(cellSize);
                table.Cell().Element(BodyCell).AlignRight()
                    .Text(FmtMoney(line.PrixUnitaire)).FontSize(cellSize);
                table.Cell().Element(BodyCell).AlignRight()
                    .Text(FmtQty(line.Quantite)).FontSize(cellSize);
                table.Cell().Element(BodyCell).AlignRight()
                    .Text(FmtMoney(line.Montant)).FontSize(cellSize);
            }
        });

        static IContainer HeaderCell(IContainer c) =>
            c.BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(2).PaddingHorizontal(1);

        static IContainer BodyCell(IContainer c) =>
            c.BorderBottom(0.5f).BorderColor(Colors.Black).PaddingVertical(3).PaddingHorizontal(1);
    }

    private static void DrawLogo(ColumnDescriptor col, TicketDocumentPdfModel model, float sideInset, float maxHeight)
    {
        if (model.LogoBytes is { Length: > 0 })
        {
            col.Item().PaddingHorizontal(sideInset).Height(maxHeight).AlignCenter()
                .Image(model.LogoBytes)
                .FitArea();
            return;
        }

        // Fallback when no logo file is configured.
        if (!string.IsNullOrWhiteSpace(model.CompanyName))
        {
            col.Item().AlignCenter()
                .Text(model.CompanyName.Trim().ToUpperInvariant())
                .Bold()
                .FontSize(10);
        }
    }

    private static void DrawCompanyInfoFooter(ColumnDescriptor col, TicketDocumentPdfModel model)
    {
        var hasName = !string.IsNullOrWhiteSpace(model.CompanyName) && model.LogoBytes is { Length: > 0 };
        var infoLines = model.CompanyInfoLines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (!hasName && infoLines.Count == 0)
            return;

        col.Item().PaddingTop(6);

        if (hasName)
        {
            col.Item().AlignCenter()
                .Text(model.CompanyName.Trim().ToUpperInvariant())
                .Bold()
                .FontSize(7.5f);
        }

        foreach (var info in infoLines)
        {
            col.Item().AlignCenter()
                .Text(info.Trim())
                .FontSize(6.5f)
                .FontColor(Colors.Grey.Darken2);
        }
    }

    private static string FmtQty(decimal value) => value.ToString("0.###", Fr);
    private static string FmtMoney(decimal value) => value.ToString("N2", Fr);
}
