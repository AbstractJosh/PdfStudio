using PdfSharp.Drawing;
using System;

namespace PdfStudio
{
    public partial class MainWindow
    {
        private static void ExportEditedPdf(string path, ParsedDocument doc)
        {
            AppFonts.Ensure(); // register IFontResolver (Arial)

            var outDoc = new PdfSharpDocument();
            foreach (var page in doc.Pages)
            {
                var p = outDoc.AddPage();
                p.Width  = page.WidthPt;
                p.Height = page.HeightPt;

                using var gfx = XGraphics.FromPdfPage(p);

                foreach (var t in page.Texts)
                {
                    var font = new XFont("Arial", Math.Max(1, t.FontSizePt), XFontStyle.Regular);
                    gfx.DrawString(t.Text, font, XBrushes.Black, new XPoint(t.XPt, t.YPt));
                }
            }
            outDoc.Save(path);
            outDoc.Close();
        }

        private static void CreateSamplePdf(string outputPath)
        {
            AppFonts.Ensure();

            var doc = new PdfSharpDocument();
            var page = doc.AddPage();
            using var gfx = XGraphics.FromPdfPage(page);

            var title = new XFont("Arial", 20, XFontStyle.Bold);
            var body  = new XFont("Arial", 12, XFontStyle.Regular);

            gfx.DrawString("Hello from PDFsharp + .NET 6", title, XBrushes.Black, new XPoint(60, 100));
            gfx.DrawString("You created this PDF and opened it here automatically.", body, XBrushes.Black, new XPoint(60, 140));

            doc.Save(outputPath);
            doc.Close();
        }
    }
}
