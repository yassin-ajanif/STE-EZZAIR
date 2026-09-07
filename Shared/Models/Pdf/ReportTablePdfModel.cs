namespace GestionCommerciale.Shared.Models.Pdf;

public sealed class ReportTablePdfModel
{
    public required string Title { get; init; }
    public string? PeriodText { get; init; }
    public IReadOnlyList<string> SummaryLines { get; init; } = [];
    public required IReadOnlyList<string> ColumnHeaders { get; init; }
    public required IReadOnlyList<IReadOnlyList<string>> Rows { get; init; }
    /// <summary>Optional per-column right alignment; length should match columns.</summary>
    public IReadOnlyList<bool>? AlignRight { get; init; }
}
