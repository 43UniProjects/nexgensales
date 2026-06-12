using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfColors = QuestPDF.Helpers.Colors;

namespace NexGenSales.Core
{
    /// <summary>
    /// Assembles a multi-section, paginated A4 PDF report from chart images using QuestPDF.
    /// Accepts a list of (title, image) pairs and renders each as a titled section.
    /// </summary>
    public class ReportBuilder
    {
        // QuestPDF requires a license declaration before any document is generated.
        static ReportBuilder()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Generates a structured PDF report document containing the provided chart images.
        /// </summary>
        /// <param name="reportType">The specific analytical context (e.g., "Sales", "Expenses") to dynamically update the document title.</param>
        /// <param name="charts">A collection of captured chart images with their corresponding section titles.</param>
        /// <param name="filePath">The designated output path for the finalized PDF.</param>
        public void GenerateReport(string reportType, List<(string Title, byte[] Image)> charts, string filePath)
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
                        // Dynamically inject the report type into the document title
                        header.Item()
                              .Text($"NexGenSales — {reportType} Analysis Report")
                              .SemiBold().FontSize(20).FontColor(PdfColors.Blue.Darken2);

                        header.Item().PaddingTop(4)
                              .LineHorizontal(2).LineColor(PdfColors.Blue.Lighten3);
                    });

                    // ── Body — one section per chart ────────────────────────────
                    page.Content().Column(col =>
                    {
                        for (int i = 0; i < charts.Count; i++)
                        {
                            var (title, image) = charts[i];

                            col.Item().PaddingTop(1, Unit.Centimetre)
                               .Text(title).Bold().FontSize(14)
                               .FontColor(PdfColors.Grey.Darken2);

                            col.Item().PaddingTop(0.3f, Unit.Centimetre)
                               .AlignCenter()
                               .Image(image).FitWidth();

                            col.Item().PaddingTop(0.5f, Unit.Centimetre)
                               .LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);

                            // Force a page break after every 2 charts (except the last one)
                            if ((i + 1) % 2 == 0 && i < charts.Count - 1)
                            {
                                col.Item().PageBreak();
                            }
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
