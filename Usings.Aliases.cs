// C# 10: global using aliases so every file sees the same names

global using WinOpenFileDialog = Microsoft.Win32.OpenFileDialog;
global using WinSaveFileDialog = Microsoft.Win32.SaveFileDialog;
global using WpfMessageBox    = System.Windows.MessageBox;
global using WpfKeyEventArgs  = System.Windows.Input.KeyEventArgs;

global using WF  = System.Windows.Forms;
global using WFI = System.Windows.Forms.Integration;

global using PdfiumDocument   = PdfiumViewer.PdfDocument;
global using PdfSharpDocument = PdfSharp.Pdf.PdfDocument;

global using WinUserControl   = System.Windows.Controls.UserControl;
global using WinTextBox       = System.Windows.Controls.TextBox;
global using WinImage         = System.Windows.Controls.Image;
global using WinGrid          = System.Windows.Controls.Grid;
global using WinCanvas        = System.Windows.Controls.Canvas;
global using WinScrollViewer  = System.Windows.Controls.ScrollViewer;

global using WinBrushes       = System.Windows.Media.Brushes;
global using WinSolidColorBrush = System.Windows.Media.SolidColorBrush;
global using WinColor         = System.Windows.Media.Color;
global using WinBitmapSource  = System.Windows.Media.Imaging.BitmapSource;
global using WinStretch       = System.Windows.Media.Stretch;
