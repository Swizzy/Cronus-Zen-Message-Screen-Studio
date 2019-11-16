using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Image = System.Drawing.Image;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    ///     Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow
    {
        private static PreviewWindow _window;
        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
        private readonly List<PixelControl> _pixels;
        public PreviewWindow(List<PixelControl> pixels)
        {
            _pixels = pixels;
            InitializeComponent();
            Closing += (sender, args) => _window = null;
            _window = this;
            _backgroundWorker.DoWork += BackgroundWorkerOnDoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerOnRunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_window == null)
                return;
            try
            {
                _window.ImagePreview.Source = BitmapToImageSource(e.Result as Image);
            }
            catch
            {
                // Ignore the error
            }
            _backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            ExportProcessor export = new ExportProcessor(_pixels);
            e.Result = export.GenerateImage();
        }

        private static BitmapImage BitmapToImageSource(Image image)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage toReturn = new BitmapImage();
                toReturn.BeginInit();
                toReturn.StreamSource = memory;
                toReturn.CacheOption = BitmapCacheOption.OnLoad;
                toReturn.EndInit();
                return toReturn;
            }
        }

        public static void ShowWindow(List<PixelControl> pixels)
        {
            if (_window != null)
            {
                _window.Topmost = true;
                _window.Show();
                _window.Activate();
                _window.Focus();
                _window.Topmost = false;
            }
            else
            {
                new PreviewWindow(pixels).Show();
            }
        }

        public static void CloseWindow() => _window?.Close();

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var aspect = MinWidth / (MinHeight - TitlebarHeight);
            if (sizeInfo.HeightChanged) Width = (sizeInfo.NewSize.Height - TitlebarHeight) * aspect;
            else Height = (sizeInfo.NewSize.Width / aspect) + TitlebarHeight;
        }
    }
}
