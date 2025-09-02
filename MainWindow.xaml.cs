using PdfiumViewer;
using System.Windows;

namespace PdfStudio
{
    public partial class MainWindow : Window
    {
        // shared fields (used across partial files)
        private readonly PdfRenderer _renderer = new() { Dock = WF.DockStyle.Fill, BackColor = System.Drawing.Color.White };
        private WFI.WindowsFormsHost? _viewerHost;
        private PdfiumDocument? _pdfiumDoc;
        private byte[]? _pdfBytesCache;
        private string? _currentPath;

        private EditPageView? _editView;
        private ParsedDocument? _parsedDoc;

        public MainWindow()
        {
            InitializeComponent();

            _viewerHost = new WFI.WindowsFormsHost { Background = System.Windows.Media.Brushes.White, Child = _renderer };
            CenterHost.Content = _viewerHost;

            try { ZoomBox.SelectedIndex = 2; } catch { }
            EnableViewerUi(false);
        }
    }
}
