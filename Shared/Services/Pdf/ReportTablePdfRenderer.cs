using GestionCommerciale.Shared.Models.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace GestionCommerciale.Shared.Services.Pdf;

public static class ReportTablePdfRenderer
{
    private const string TextPrimary = "#111827";
    private const string TextMuted = "#6B7280";
    private const string TableHeaderBg = "#E5E7EB";
    private const string TableBorder = "#D1D5DB";
    private const string TableRowAlt = "#F9FAFB";
    private const float HeaderLogoWidth = 128f;
    private const float HeaderLogoHeight = 78f;

    public static byte[] Render(
        string societeNom,
        ReportTablePdfModel model,
        byte[]? logoBytes)
    {
        var colCount = Math.Max(1, model.ColumnHeaders.Count);
        var alignRight = model.AlignRight;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.MarginHorizontal(28);
                page.MarginVertical(24);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(TextPrimary));

                page.Header().Column(header =>
                {
                    header.Spacing(6);
                    header.Item().Row(row =>
                    {
                        if (logoBytes is { Length: > 0 })
                            row.ConstantItem(HeaderLogoWidth).Height(HeaderLogoHeight).Image(logoBytes).FitArea();
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            if (!string.IsNullOrWhiteSpace(societeNom))
                                col.Item().Text(societeNom).Bold().FontSize(14);
                            col.Item().Text(model.Title).Bold().FontSize(15);
                            if (!string.IsNullOrWhiteSpace(model.PeriodText))
                                col.Item().Text(model.PeriodText!).FontSize(9).FontColor(TextMuted);
                        });
                    });

                    if (model.SummaryLines.Count > 0)
                    {
                        header.Item().PaddingTop(6).Border(1).BorderColor(TableBorder).Padding(8).Column(sum =>
                        {
                            sum.Spacing(2);
                            foreach (var line in model.SummaryLines)
                                sum.Item().Text(line).FontSize(9);
                        });
                    }
                });

                page.Content().PaddingTop(10).Column(content =>
                {
                    if (model.Rows.Count == 0)
                    {
                        content.Item().Text("—").FontColor(TextMuted);
                        return;
                    }

                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            for (var i = 0; i < colCount; i++)
                                columns.RelativeColumn(1);
                        });

                        table.Header(h =>
                        {
                            for (var i = 0; i < colCount; i++)
                            {
                                var title = i < model.ColumnHeaders.Count ? model.ColumnHeaders[i] : string.Empty;
                                var right = alignRight is not null && i < alignRight.Count && alignRight[i];
                                HeaderCell(h.Cell(), title, right);
                            }
                        });

                        var rowIndex = 0;
                        foreach (var row in model.Rows)
                        {
                            var bg = rowIndex % 2 == 1 ? TableRowAlt : "#FFFFFF";
                            for (var i = 0; i < colCount; i++)
                            {
                                var text = i < row.Count ? row[i] : string.Empty;
                                var right = alignRight is not null && i < alignRight.Count && alignRight[i];
                                BodyCell(table.Cell().Background(bg), text, right);
                            }
                            rowIndex++;
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static void HeaderCell(IContainer cell, string text, bool alignRight)
    {
        var c = cell.Background(TableHeaderBg).Border(0.5f).BorderColor(TableBorder).Padding(4);
        if (alignRight)
            c.AlignRight().Text(text).Bold().FontSize(8);
        else
            c.Text(text).Bold().FontSize(8);
    }

    private static void BodyCell(IContainer cell, string text, bool alignRight)
    {
        var c = cell.Border(0.5f).BorderColor(TableBorder).Padding(4);
        if (alignRight)
            c = c.AlignRight();
        c.Text(text).FontSize(8);
    }
}
