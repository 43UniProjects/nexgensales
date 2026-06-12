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
        /// Generates a structured PDF report document containing chart images and contextual summaries.
        /// </summary>
        public void GenerateReport(
            string reportType,
            List<(string Title, byte[] Image)> charts,
            string filePath,
            string summaryTitle = null,
            string summaryValue = null,
            List<string> anomalies = null)
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
                              .Text($"NexGenSales — {reportType} Analysis Report")
                              .SemiBold().FontSize(20).FontColor(PdfColors.Blue.Darken2);

                        header.Item().PaddingTop(4).PaddingBottom(10)
                              .LineHorizontal(2).LineColor(PdfColors.Blue.Lighten3);
                    });

                    // ── Body ────────────────────────────────────────────────────
                    page.Content().Column(col =>
                    {
                        // 1. Report Summary Section (e.g., Total Expenses)
                        if (!string.IsNullOrEmpty(summaryTitle) && !string.IsNullOrEmpty(summaryValue))
                        {
                            col.Item().PaddingBottom(1, Unit.Centimetre)
                               .Background(PdfColors.Grey.Lighten4)
                               .Padding(15)
                               .Row(row =>
                               {
                                   row.RelativeItem().Text(summaryTitle).SemiBold().FontSize(14).FontColor(PdfColors.Grey.Darken3);
                                   row.RelativeItem().AlignRight().Text(summaryValue).Bold().FontSize(16).FontColor(PdfColors.Teal.Darken2);
                               });
                        }

                        // 2. Charts Section
                        for (int i = 0; i < charts.Count; i++)
                        {
                            var (title, image) = charts[i];

                            col.Item().PaddingTop(0.5f, Unit.Centimetre)
                               .Text(title).Bold().FontSize(14).FontColor(PdfColors.Grey.Darken2);

                            col.Item().PaddingTop(0.3f, Unit.Centimetre)
                               .AlignCenter().Image(image).FitWidth();

                            col.Item().PaddingTop(0.5f, Unit.Centimetre).PaddingBottom(0.5f, Unit.Centimetre)
                               .LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);

                            if ((i + 1) % 2 == 0 && i < charts.Count - 1)
                            {
                                col.Item().PageBreak();
                            }
                        }

                        // 3. Anomalies/Alerts Section
                        if (anomalies != null)
                        {
                            col.Item().PageBreak();

                            col.Item().PaddingTop(0.5f, Unit.Centimetre)
                               .Text("Detected Cost Anomalies").Bold().FontSize(16).FontColor(PdfColors.Red.Medium);

                            col.Item().PaddingTop(0.2f, Unit.Centimetre).PaddingBottom(0.5f, Unit.Centimetre)
                               .LineHorizontal(1).LineColor(PdfColors.Red.Lighten4);

                            if (anomalies.Count == 0)
                            {
                                col.Item().PaddingTop(1, Unit.Centimetre).AlignCenter()
                                   .Text("All Good!").Bold().FontSize(16).FontColor(PdfColors.Green.Medium);
                                col.Item().AlignCenter()
                                   .Text("No unusual cost spikes detected for this period.").FontSize(12).FontColor(PdfColors.Grey.Medium);
                            }
                            else
                            {
                                foreach (var anomaly in anomalies)
                                {
                                    col.Item().PaddingBottom(5)
                                       .Text($"• {anomaly}").FontSize(11).FontColor(PdfColors.Grey.Darken3);
                                }
                            }
                        }
                    });

                    // ── Footer ──────────────────────────────────────────────────
                    page.Footer().AlignCenter().DefaultTextStyle(s => s.FontSize(9).FontColor(PdfColors.Grey.Medium))
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
