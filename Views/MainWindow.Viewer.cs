using PdfiumViewer;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace PdfStudio
{
    public partial class MainWindow
    {
        private void OpenPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new WinOpenFileDialog { Filter = "PDF files (*.pdf)|*.pdf", Title = "Open PDF" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    LoadIntoViewer(dlg.FileName);
                    StatusText.Text = $"Opened: {dlg.FileName}";
                }
                catch (Exception ex)
                {
                    WpfMessageBox.Show(this, ex.ToString(), "Load error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CreateNewPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), $"PDFStudio_{Guid.NewGuid():N}.pdf");
                CreateSamplePdf(tempPath);
                LoadIntoViewer(tempPath);
                StatusText.Text = "Created new PDF and loaded it.";
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(this, ex.Message, "Create PDF error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (_parsedDoc != null)
            {
                // Export edited version
                var dlgOut = new WinSaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", Title = "Export Edited PDF" };
                if (dlgOut.ShowDialog() == true)
                {
                    try
                    {
                        ExportEditedPdf(dlgOut.FileName, _parsedDoc);
                        StatusText.Text = $"Exported edited PDF: {dlgOut.FileName}";
                    }
                    catch (Exception ex)
                    {
                        WpfMessageBox.Show(this, ex.ToString(), "Export error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return;
            }

            if (_pdfiumDoc == null) return;

            var dlg = new WinSaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", Title = "Save PDF As" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    if (!string.IsNullOrEmpty(_currentPath) && File.Exists(_currentPath))
                        File.Copy(_currentPath, dlg.FileName, overwrite: true);
                    else if (_pdfBytesCache != null)
                        File.WriteAllBytes(dlg.FileName, _pdfBytesCache);
                    else
                    {
                        WpfMessageBox.Show(this, "No original PDF bytes to save.", "Save As",
                                           MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    StatusText.Text = $"Saved: {dlg.FileName}";
                }
                catch (Exception ex)
                {
                    WpfMessageBox.Show(this, ex.Message, "Save As error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_pdfiumDoc == null) return;
            CaptureEditsIfEditing();

            var p = Math.Max(0, _renderer.Page - 1);
            _renderer.Page = p;
            PageBox.Text = (p + 1).ToString();

            RefreshEditSurfaceIfActive();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_pdfiumDoc == null) return;
            CaptureEditsIfEditing();

            var p = Math.Min(_pdfiumDoc.PageCount - 1, _renderer.Page + 1);
            _renderer.Page = p;
            PageBox.Text = (p + 1).ToString();

            RefreshEditSurfaceIfActive();
        }

        private void PageBox_KeyDown(object sender, WpfKeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && _pdfiumDoc != null &&
                int.TryParse(PageBox.Text, out var oneBased))
            {
                CaptureEditsIfEditing();

                var zero = Math.Clamp(oneBased - 1, 0, _pdfiumDoc.PageCount - 1);
                _renderer.Page = zero;
                PageBox.Text = (zero + 1).ToString();

                RefreshEditSurfaceIfActive();
            }
        }

        private void ZoomBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pdfiumDoc == null || ZoomBox.SelectedItem == null) return;

            var text = (ZoomBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (text != null && text.EndsWith("%") && int.TryParse(text.TrimEnd('%'), out var percent))
            {
                if (CenterHost.Content is EditPageView)
                    StatusText.Text = "Edit mode zoom not implemented; using page bitmap scale.";
                else
                    _renderer.Zoom = percent / 100.0;
            }
        }

        private void LoadIntoViewer(string path)
        {
            DisposeCurrent();

            _currentPath = path;
            _pdfBytesCache = File.ReadAllBytes(path);

            _pdfiumDoc = PdfiumDocument.Load(new MemoryStream(_pdfBytesCache, writable: false));
            _renderer.Load(_pdfiumDoc);

            EnableViewerUi(true);
            PageBox.Text = "1";
            PageCountText.Text = $"/ {_pdfiumDoc.PageCount}";
            _renderer.Page = 0;

            try { _renderer.ZoomMode = PdfViewerZoomMode.FitWidth; } catch { }

            // Parse synchronously (v1)
            _parsedDoc = PdfExtractor.Parse(_currentPath);
        }

        private void EnableViewerUi(bool on)
        {
            try { SaveAsButton.IsEnabled = on; } catch { }
            try { PrevBtn.IsEnabled = on; } catch { }
            try { NextBtn.IsEnabled = on; } catch { }
            try { PageBox.IsEnabled = on; } catch { }
            try { ZoomBox.IsEnabled = on; } catch { }
        }

        private void DisposeCurrent()
        {
            CaptureEditsIfEditing();

            try { _pdfiumDoc?.Dispose(); } catch { }
            _pdfiumDoc = null;

            _currentPath = null;
            _pdfBytesCache = null;

            _parsedDoc = null;
            _editView = null;

            _viewerHost = new WFI.WindowsFormsHost { Background = System.Windows.Media.Brushes.White, Child = _renderer };
            CenterHost.Content = _viewerHost;

            EnableViewerUi(false);
            try { PageBox.Text = ""; } catch { }
            try { PageCountText.Text = "/ 0"; } catch { }
        }

        protected override void OnClosed(EventArgs e)
        {
            DisposeCurrent();
            base.OnClosed(e);
        }

        // Bitmap helper (Image → BitmapSource)
        private static System.Windows.Media.Imaging.BitmapSource CreateBitmapSourceAndFree(System.Drawing.Image img)
        {
            using var bmp = new System.Drawing.Bitmap(img);
            IntPtr hBmp = bmp.GetHbitmap();
            try
            {
                var src = Imaging.CreateBitmapSourceFromHBitmap(
                    hBmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                src.Freeze();
                return src;
            }
            finally
            {
                DeleteObject(hBmp);
                img.Dispose();
            }
        }

        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);
    }
}
