namespace PdfStudio
{
    public record ParsedDocument(System.Collections.Generic.List<ParsedPage> Pages);
    public record ParsedPage(int PageNumber, double WidthPt, double HeightPt,
                             System.Collections.Generic.List<TextSpan> Texts);
    public record TextSpan(int Index, string Text, double XPt, double YPt,
                           double WidthPt, double HeightPt, double FontSizePt, string? FontName);
}
