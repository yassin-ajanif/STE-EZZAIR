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
                    col.Item().PaddingVertical(4).AlignCenter().Text(dash).FontSize(7);

                    foreach (var line in model.Lines)
                    {
                        col.Item().PaddingTop(3)
                            .Text(line.Designation).Bold().FontSize(8);
                        col.Item().Row(r =>
                        {
                            r.RelativeItem()
                                .Text($"{FmtQty(line.Quantite)} x {FmtMoney(line.PrixUnitaire)}")
                                .FontSize(7.5f);
                            r.AutoItem()
                                .Text(FmtMoney(line.Montant))
                                .FontSize(8);
                        });
                    }

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
