using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly List<PixelControl> _pixelControls = new List<PixelControl>();
        private PixelControl _lastHighlight;
        private bool _windowLoaded;
        private double previousViewBoxWidth = 0;
        private double previousViewBoxHeight = 0;
        private string _savePath;
        private bool _saved = true;

        public MainWindow()
        {
            InitializeComponent();
            Title = $"Cronus Zen Message Screen Studio v{App.Version}";

            // Setup settings in the view
            Settings.LoadSettings();
            PenWidth.Value = Settings.CurrentSettings.PenWidth;
            PenHeight.Value = Settings.CurrentSettings.PenHeight;
            HighlightColumnAndRowBox.IsChecked = Settings.CurrentSettings.HighlightColumnAndRow;
            HighlightFullColumnAndRowBox.IsChecked = Settings.CurrentSettings.HighlightFullColumnAndRow;
            Width = Math.Max(MinWidth, Settings.CurrentSettings.WindowWidth);
            Height = Math.Max(MinHeight, Settings.CurrentSettings.WindowHeight);
            List<SelectionData<Settings.PenShapes>> penShapeList = Settings.MakePenShapeSelectionList();
            PenShapeBox.ItemsSource = penShapeList;
            PenShapeBox.SelectedItem = penShapeList.FirstOrDefault(o => o.Value == Settings.CurrentSettings.PenShape);

            // Setup the pixel controls and line/column numbers
            for (int x = 0; x < 129; x++)
            {
                TextBlock label = new TextBlock
                                  {
                                      TextAlignment = TextAlignment.Center,
                                      Text = x == 0 ? "" : (x - 1).ToString(),
                                      Margin = new Thickness(0,0,5,0)
                                  };
                Canvas.Children.Add(label);
            }
            for (int y = 0; y < 64; y++)
            {
                TextBlock label = new TextBlock
                                  {
                                      Text = y.ToString(),
                                      TextAlignment = TextAlignment.Right,
                                      Margin = new Thickness(5, 0, 5, 0)
                };
                Canvas.Children.Add(label);
                for (int x = 0; x < 128; x++)
                {
                    PixelControl control = new PixelControl(HighlightRowAndColumn, x, y);
                    _pixelControls.Add(control);
                    Canvas.Children.Add(control);
                }
            }

            Closing += (sender, args) =>
                       {
                           PreviewWindow.CloseWindow();
                           DevicePreviewWindow.CloseWindow();
                       };
        }

        private void HighlightRowAndColumn(PixelControl control)
        {
            if (control == null)
            {
                return;
            }
            _lastHighlight = control;
            bool setWhite = false;
            bool setBlack = false;

            if (control.Highlighted && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                setWhite = true;
            }
            else if (control.Highlighted && Mouse.RightButton == MouseButtonState.Pressed)
            {
                setBlack = true;
            }
            int x = control.X;
            int y = control.Y;
            CursorPosition.Text = control.Highlighted ? $"X: {x} Y: {y}" : "";
            Settings.PenShapes shape = Settings.CurrentSettings.PenShape;
            if (Settings.CurrentSettings.HighlightColumnAndRow || Settings.CurrentSettings.HighlightFullColumnAndRow || Settings.CurrentSettings.PenWidth > 1 || Settings.CurrentSettings.PenHeight > 1)
            {

                foreach (PixelControl pixelControl in _pixelControls)
                {
                    bool isWithinDrawing = false;
                    switch (shape)
                    {
                        case Settings.PenShapes.Square:
                            isWithinDrawing = pixelControl.IsWithinSquare(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        case Settings.PenShapes.Ellipse:
                            isWithinDrawing = pixelControl.IsWithinEllipse(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        case Settings.PenShapes.Cross:
                            isWithinDrawing = pixelControl.IsWithinCross(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        case Settings.PenShapes.TriangleUp:
                            isWithinDrawing = pixelControl.IsWithinTriangleUp(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        case Settings.PenShapes.TriangleDown:
                            isWithinDrawing = pixelControl.IsWithinTriangleDown(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        case Settings.PenShapes.TriangleLeft:
                            isWithinDrawing = pixelControl.IsWithinTriangleLeft(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        case Settings.PenShapes.TriangleRight:
                            isWithinDrawing = pixelControl.IsWithinTriangleRight(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        case Settings.PenShapes.Diamond:
                            isWithinDrawing  = pixelControl.IsWithinTriangleUp(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            isWithinDrawing |= pixelControl.IsWithinTriangleDown(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        case Settings.PenShapes.LineLTR:
                            isWithinDrawing = pixelControl.IsOnLineLTR(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        case Settings.PenShapes.LineRTL:
                            isWithinDrawing = pixelControl.IsOnLineRTL(x, y, Settings.CurrentSettings.PenWidth, Settings.CurrentSettings.PenHeight);
                            break;
                        default:
                            isWithinDrawing = false;
                            break;
                    }

                    pixelControl.Highlighted = control.Highlighted
                                               &&
                                               (
                                                   Settings.CurrentSettings.HighlightColumnAndRow
                                                   &&
                                                   (
                                                       pixelControl.X <= x && pixelControl.Y == y
                                                       ||
                                                       pixelControl.Y <= y && pixelControl.X == x
                                                   )
                                                   ||
                                                   Settings.CurrentSettings.HighlightFullColumnAndRow == true
                                                   &&
                                                   (
                                                       pixelControl.Y == y
                                                       ||
                                                       pixelControl.X == x
                                                   )
                                                   ||
                                                   isWithinDrawing
                                               );
                    if (setWhite && isWithinDrawing)
                    {
                        pixelControl.Color = true;
                    }
                    else if (setBlack && isWithinDrawing)
                    {
                        pixelControl.Color = false;
                    }
                }
                if (setBlack || setWhite)
                    _saved = false;
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e) => new ExportWindow(_pixelControls) { Owner = this }.ShowDialog();

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var imageWindow = new LoadImageWindow { Owner = this };
            if (imageWindow.ShowDialog() == true)
            {
                var pixels = imageWindow.GetPixels();
                for (int y = 0; y < 64; y++)
                {
                    for (var x = 0; x < 128; x++)
                    {
                        PixelControl pixel = _pixelControls.First(p => p.X == x && p.Y == y);
                        pixel.Color = pixels[x, y];
                    }
                }
                _saved = false;
            }
        }

        private void ImportPackedData_Click(object sender, RoutedEventArgs e)
        {
            var importWindow = new ImportPackedWindow { Owner = this };
            if (importWindow.ShowDialog() == true)
            {
                var pixels = importWindow.GetPixels();
                for (int y = 0; y < 64; y++)
                {
                    for (var x = 0; x < 128; x++)
                    {
                        PixelControl pixel = _pixelControls.First(p => p.X == x && p.Y == y);
                        pixel.Color = pixels[x, y];
                    }
                }
                _saved = false;
            }
        }

        private void DrawText_Click(object sender, RoutedEventArgs e)
        {
            var drawTextWindow = new DrawTextWindow { Owner = this };
            if (drawTextWindow.ShowDialog() == true)
            {
                var pixels = drawTextWindow.GetPixels();
                for (int y = 0; y < 64; y++)
                {
                    for (var x = 0; x < 128; x++)
                    {
                        PixelControl pixel = _pixelControls.First(p => p.X == x && p.Y == y);
                        pixel.Color = pixels[x, y];
                    }
                }

                _saved = false;
            }
        }

        private void ViewBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
                var width = Viewbox.ActualWidth;
                var height = Viewbox.ActualHeight;
                if (e.Delta > 0)
                {
                    width *= 1.02;
                    height *= 1.02;
                }
                else if (e.Delta < 0)
                {
                    width *= 0.98;
                    height *= 0.98;
                }

                Viewbox.Width = width;
                Viewbox.Height = height;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all pixels?", "", MessageBoxButton.YesNoCancel) ==
                MessageBoxResult.Yes)
            {
                foreach (PixelControl pixel in _pixelControls)
                {
                    pixel.Color = false;
                }
            }
            _saved = false;
        }

        private void Invert_Click(object sender, RoutedEventArgs e)
        {
            foreach (PixelControl pixel in _pixelControls)
            {
                pixel.Invert();
            }
            _saved = false;
        }

        private void MainWindow_OnActivated(object sender, EventArgs eventArgs)
        {
            SetShowAllPixelsLabel();
            if (_windowLoaded)
            {
                return;
            }
            ShowAllPixels_Click(this, null);
            _windowLoaded = true;
        }

        private void PenWidth_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            Settings.CurrentSettings.PenWidth = (int)(e.NewValue ?? 1);
            HighlightRowAndColumn(_lastHighlight);
        }

        private void PenHeight_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            Settings.CurrentSettings.PenHeight = (int)(e.NewValue ?? 1);
            HighlightRowAndColumn(_lastHighlight);
        }

        private void ShowAllPixels_Click(object sender, RoutedEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Viewbox.Width = previousViewBoxWidth;
                Viewbox.Height = previousViewBoxHeight;
            }
            else
            {
                previousViewBoxWidth = Viewbox.ActualWidth;
                previousViewBoxHeight = Viewbox.ActualHeight;
                Viewbox.Width = Viewbox.MinWidth = ScrollViewer.ViewportWidth;
                Viewbox.Height = Viewbox.MinHeight = ScrollViewer.ViewportHeight;
            }
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.CurrentSettings.WindowWidth = Width;
            Settings.CurrentSettings.WindowHeight = Height;
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => Thread.Sleep(100);
            bw.RunWorkerCompleted += (o, args) =>
                                     {
                                         Viewbox.MinWidth = ScrollViewer.ViewportWidth;
                                         Viewbox.MinHeight = ScrollViewer.ViewportHeight;
                                     };
            bw.RunWorkerAsync();
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e) { MainWindow_OnSizeChanged(sender, null); }

        private void SetShowAllPixelsLabel()
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ShowAllPixelsBtn.Content = "Back";
            }
            else
            {
                ShowAllPixelsBtn.Content = "Show all pixels";
            }
        }

        private void MainWindow_OnPreviewKeyDownOrUp(object sender, KeyEventArgs e) { SetShowAllPixelsLabel(); }

        private void HighlightColumnAndRowBox_OnChecked(object sender, RoutedEventArgs e)
        {
            Settings.CurrentSettings.HighlightColumnAndRow = HighlightColumnAndRowBox.IsChecked == true;
        }

        private void HighlightFullColumnAndRowBox_OnChecked(object sender, RoutedEventArgs e)
        {
            Settings.CurrentSettings.HighlightFullColumnAndRow = HighlightFullColumnAndRowBox.IsChecked == true;
        }

        private void ShowPreview_Click(object sender, RoutedEventArgs e) { PreviewWindow.ShowWindow(_pixelControls); }

        private void ShowDevicePreview_Click(object sender, RoutedEventArgs e) { DevicePreviewWindow.ShowWindow(_pixelControls); }

        protected override void OnSourceInitialized(EventArgs e)
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            ((HwndSource)source)?.AddHook(Hook);
            base.OnSourceInitialized(e);
        }

        private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x020E: // WM_MOUSEHWHEEL - Horizontal Wheel
                    if (wParam.ToInt32() > 0)
                    {
                        ScrollViewer.LineRight();
                    }
                    else
                    {
                        ScrollViewer.LineLeft();
                    }
                    return (IntPtr)1;
            }
            return IntPtr.Zero;
        }

        private void AlwaysExecutable_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_savePath))
            {
                ZenMessageStudioProject project = new ZenMessageStudioProject(_pixelControls);
                project.SaveToFile(_savePath);
                _saved = true;
            }
            else
            {
                SaveAs_Executed(sender, e);
            }
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = string.IsNullOrWhiteSpace(_savePath) ? "Project.zmsp" : System.IO.Path.GetFileName(_savePath),
                AddExtension = true,
                DefaultExt = ".zmsp"
            };
            if (sfd.ShowDialog(this) == true)
            {
                _savePath = sfd.FileName;
                Save_Executed(sender, e);
            }
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                FileName = "Project.zmsp"
            };
            if (ofd.ShowDialog(this) == true)
            {
                var project = ZenMessageStudioProject.ReadFromFile(ofd.FileName);
                if (project == null)
                {
                    MessageBox.Show("Invalid file selected.");
                }
                else
                {
                    _savePath = ofd.FileName;
                    foreach (PixelControl pixel in _pixelControls)
                    {
                        pixel.Color = false;
                    }
                    for (int x = 0; x < 128; x++)
                    {
                        for (int y = 0; y < 64; y++)
                        {
                            PixelControl control = _pixelControls.FirstOrDefault(p => p.X == x && p.Y == y);
                            if (control != null)
                            {
                                control.Color = project.PixelData[x, y];
                            }
                        }
                    }
                }
            }
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_saved)
            {
                if (MessageBox.Show("You have unsaved changes that will be lost, are you sure you want to start over?", "Are you sure?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            foreach (PixelControl pixel in _pixelControls)
            {
                pixel.Color = false;
            }
            _saved = true;
            _savePath = null;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_saved)
            {
                e.Cancel = MessageBox.Show("You have unsaved changes, are you sure you want to exit?", "Unsaved changes detected", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes;
            }
        }
        private void Exit_Clicked(object sender, RoutedEventArgs e) => Close();

        private void PenShapeBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.CurrentSettings.PenShape = PenShapeBox.SelectedValue as Settings.PenShapes? ?? Settings.PenShapes.Square;
            HighlightRowAndColumn(_lastHighlight);
        }

        private void ShowPenSettings_Click(object sender, RoutedEventArgs e)
        {
            PenFlyout.IsOpen = true;
        }
    }
}
