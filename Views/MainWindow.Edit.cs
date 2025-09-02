using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PdfStudio
{
    public partial class MainWindow
    {
        private bool IsInEditMode => CenterHost.Content is EditPageView;

        private void EditToggle_Checked(object sender, RoutedEventArgs e)  => EnterEditMode();
        private void EditToggle_Unchecked(object sender, RoutedEventArgs e) => LeaveEditMode();

        private void EnterEditMode()
        {
            if (_pdfiumDoc == null) { WpfMessageBox.Show(this, "Open a PDF first."); EditToggle.IsChecked = false; return; }
            if (_parsedDoc == null)
            {
                if (string.IsNullOrEmpty(_currentPath)) { WpfMessageBox.Show(this, "Missing original path."); EditToggle.IsChecked = false; return; }
                _parsedDoc = PdfExtractor.Parse(_currentPath);
            }

            var pageIdx = Math.Clamp(_renderer.Page, 0, _pdfiumDoc.PageCount - 1);
            var parsedPage = _parsedDoc.Pages[Math.Clamp(pageIdx, 0, _parsedDoc.Pages.Count - 1)];

            const int dpi = 144;
            int pxW = (int)Math.Round(parsedPage.WidthPt  * dpi / 72.0);
            int pxH = (int)Math.Round(parsedPage.HeightPt * dpi / 72.0);

            using var img = _pdfiumDoc.Render(pageIdx, pxW, pxH, dpi, dpi, true);
            var bmpSource = CreateBitmapSourceAndFree(img);

            _editView = new EditPageView();
            _editView.Load(parsedPage, bmpSource); // background hidden by default
            CenterHost.Content = _editView;
        }

        private void LeaveEditMode()
        {
            CaptureEditsIfEditing();

            _viewerHost = new WFI.WindowsFormsHost { Background = Brushes.White, Child = _renderer };
            CenterHost.Content = _viewerHost;
        }

        private void CaptureEditsIfEditing()
        {
            if (_editView != null && _parsedDoc != null && IsInEditMode)
            {
                var edited = _editView.ApplyEdits();
                var idx = Math.Clamp(_renderer.Page, 0, _parsedDoc.Pages.Count - 1);
                var pages = _parsedDoc.Pages.ToList();
                pages[idx] = edited;
                _parsedDoc = new ParsedDocument(pages);
            }
        }

        // When paging while editing, rebuild the overlay for current page
        private void RefreshEditSurfaceIfActive()
        {
            if (_pdfiumDoc == null || _parsedDoc == null || !IsInEditMode) return;

            var pageIdx = Math.Clamp(_renderer.Page, 0, _pdfiumDoc.PageCount - 1);
            var parsedPage = _parsedDoc.Pages[Math.Clamp(pageIdx, 0, _parsedDoc.Pages.Count - 1)];

            const int dpi = 144;
            int pxW = (int)Math.Round(parsedPage.WidthPt  * dpi / 72.0);
            int pxH = (int)Math.Round(parsedPage.HeightPt * dpi / 72.0);

            using var img = _pdfiumDoc.Render(pageIdx, pxW, pxH, dpi, dpi, true);
            var bmpSource = CreateBitmapSourceAndFree(img);

            _editView = new EditPageView();
            _editView.Load(parsedPage, bmpSource);
            CenterHost.Content = _editView;
        }
    }
}
