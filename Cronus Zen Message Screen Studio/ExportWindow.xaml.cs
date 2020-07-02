using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    ///     Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow
    {
        private readonly ExportProcessor _exportProcessor;
        public ExportWindow(List<PixelControl> pixelControls)
        {
            _exportProcessor = new ExportProcessor(pixelControls);
            InitializeComponent();
        }

        private void Individual_Click(object sender, RoutedEventArgs e)
        {
            ExportProcessor.ExportSettings settings = ExportProcessor.ExportSettings.IndividualPixels;
            if (IncludeClearScreen.IsChecked == true)
            {
                settings |= ExportProcessor.ExportSettings.IncludeClear;
            }
            PerformExport(settings, false);
        }

        private void IndividualZenStudio_Click(object sender, RoutedEventArgs e)
        {
            ExportProcessor.ExportSettings settings = ExportProcessor.ExportSettings.IndividualPixels;
            if (IncludeClearScreen.IsChecked == true)
            {
                settings |= ExportProcessor.ExportSettings.IncludeClear;
            }
            PerformExport(settings, true);
        }

        private void Packed16_Click(object sender, RoutedEventArgs e) => PerformPackedExport(true);

        private void Packed16ZenStudio_Click(object sender, RoutedEventArgs e) => PerformPackedExport(true, true);

        private void Packed8_Click(object sender, RoutedEventArgs e) => PerformPackedExport(false);

        private void Packed8ZenStudio_Click(object sender, RoutedEventArgs e) => PerformPackedExport(false, true);

        private void PerformPackedExport(bool use16Bit, bool sendToZenStudio = false)
        {
            ExportProcessor.ExportSettings settings = ExportProcessor.ExportSettings.Packed1DArray;
            if (use16Bit)
            {
                settings |= ExportProcessor.ExportSettings.Packed16Bit;
            }
            else
            {
                settings |= ExportProcessor.ExportSettings.Packed8Bit;
            }

            if (IncludeFunction.IsChecked == true)
            {
                settings |= ExportProcessor.ExportSettings.IncludeFunction;

                if (PackedStatic.IsChecked == true)
                {
                    settings |= ExportProcessor.ExportSettings.PackedStatic;
                }
                if (PackedInvertSupport.IsChecked == true)
                {
                    settings |= ExportProcessor.ExportSettings.PackedInvertSupport;
                }

            }

            PerformExport(settings, sendToZenStudio);
        }

        private void PerformExport(ExportProcessor.ExportSettings settings, bool sendToZenStudio)
        {
            if (SampleScriptBox.IsChecked == true)
            {
                settings |= ExportProcessor.ExportSettings.SampleScript;
            }

            if (ForceWhitePixels.IsChecked == true)
            {
                settings |= ExportProcessor.ExportSettings.ForceWhite;
            }
            else if (ForceBlackPixels.IsChecked == true)
            {
                settings |= ExportProcessor.ExportSettings.ForceBlack;
            }

            string identifier = Identifier.Text.Trim();
            identifier = Regex.Replace(identifier, "[^a-zA-Z0-9_]", "_");
            string data = _exportProcessor.GenerateExportData(settings, identifier);
            if (sendToZenStudio == false)
            {
                _exportProcessor.Savefile(data);
            }
            else
            {
                ZenStudioCommands.SendOpenCompilerTab(data);
            }
        }

        private void ImgButton_Click(object sender, RoutedEventArgs e) { _exportProcessor.GenerateAndSaveImage(); }

        private void PackedExcalibur_Click(object sender, RoutedEventArgs e)
        {
            ExportProcessor.ExportSettings settings = ExportProcessor.ExportSettings.Packed1DArray | ExportProcessor.ExportSettings.PackedExcalibur;
            if (ForceWhitePixels.IsChecked == true)
            {
                settings |= ExportProcessor.ExportSettings.ForceWhite;
            }
            else if (ForceBlackPixels.IsChecked == true)
            {
                settings |= ExportProcessor.ExportSettings.ForceBlack;
            }

            string data = _exportProcessor.GenerateExportData(settings, "");
            _exportProcessor.Savefile(data);
        }
    }
}
