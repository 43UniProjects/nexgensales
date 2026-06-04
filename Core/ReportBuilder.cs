using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfColors = QuestPDF.Helpers.Colors;

namespace NextGenSales.Core
{
    /// <summary>
    /// Assembles a multi-section, paginated A4 PDF report from chart images using QuestPDF.
    /// Accepts a list of (title, image) pairs and renders each as a titled section.
    /// </summary>
    public class ReportBuilder
    {
        public void GenerateReport(List<(string Title, byte[] Image)> charts, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(PdfColors.White);

                    // ── Header ──────────────────────────────────────────────────
                    page.Header().Column(header =>
                    {
                        header.Item()
                              .Text("NexGenSales — Sales Analysis Report")
                              .SemiBold().FontSize(20).FontColor(PdfColors.Blue.Darken2);

                        header.Item().PaddingTop(4)
                              .LineHorizontal(2).LineColor(PdfColors.Blue.Lighten3);
                    });

                    // ── Body — one section per chart ────────────────────────────
                    page.Content().Column(col =>
                    {
                        foreach (var (title, image) in charts)
                        {
                            col.Item().PaddingTop(1, Unit.Centimetre)
                               .Text(title).Bold().FontSize(14)
                               .FontColor(PdfColors.Grey.Darken2);

                            col.Item().PaddingTop(0.3f, Unit.Centimetre)
                               .Image(image);

                            col.Item().PaddingTop(0.5f, Unit.Centimetre)
                               .LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);
                        }
                    });

                    // ── Footer ──────────────────────────────────────────────────
                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(s => s.FontSize(9).FontColor(PdfColors.Grey.Medium))
                        .Text(x =>
                        {
                            x.Span("NexGenSales Analytics  •  Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf(filePath);
        }
    }
}
