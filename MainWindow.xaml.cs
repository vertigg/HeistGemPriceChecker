using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NHotkey;
using NHotkey.Wpf;
using HeistGemChecker.OCR;
using HeistGemChecker.src;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Controls.Primitives;
using MessageBox = System.Windows.MessageBox;

namespace HeistGemChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap? _cachedBitmap;
        private Bitmap? _originalScreenshot;
        private string[] _detectedGems = Array.Empty<string>();
        private bool isProcessing = false;

        public MainWindow()
        {
            //HotkeyManager.Current.AddOrReplace("Capture", new KeyGesture(Key.F3), PriceCheck);
            HotkeyManager.Current.AddOrReplace("DebugOCR", new KeyGesture(Key.F3, ModifierKeys.Shift), ShowCommand);
            InitializeComponent();
            InitializeSystemTrayIcon();

        }
        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) => Hide();
        private void ThresholdSlider_DragCompleted(object sender, DragCompletedEventArgs e) => UpdateScreenshotPreview((short)ThresholdSlider.Value);
        private string[] DetectedGems
        {
            get => _detectedGems;
            set 
            {
                _detectedGems = value;
                PriceCheckBtn.IsEnabled = value.Length > 0;
                DetectedGemsTextElement.Text = value.Length > 0 ? string.Join("\n", DetectedGems) : "No gems detected";
            }
        }

        private void ShowCommand(object? sender, HotkeyEventArgs e)
        {
            if (!IsActive) Show();
        }
        private void HeistOCRWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        protected new void Show()
        {
            _originalScreenshot = ImageProcessing.CaptureScreenshot();
            UpdateScreenshotPreview();
            base.Show();
        }

        protected new void Hide()
        {
            if (_originalScreenshot != null) { _originalScreenshot.Dispose(); }
            if (_cachedBitmap != null) { _cachedBitmap.Dispose(); }
            DetectedGems = Array.Empty<string>();
            base.Hide();
        }

        private void UpdateScreenshotPreview(short threshold = 270)
        {
            if (_originalScreenshot != null)
            {
                var cloneRect = new System.Drawing.Rectangle(0, 0, _originalScreenshot.Width, _originalScreenshot.Height);
                _cachedBitmap = _originalScreenshot.Clone(cloneRect, _originalScreenshot.PixelFormat);
                ImageProcessing.ApplyThreshold(ref _cachedBitmap, threshold);
                ScreenshotPreviewElement.Source = ImageProcessing.BitmapToImageSource(_cachedBitmap);
            }
        }

        private async void RunOCRBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isProcessing) return;

            try
            {
                DetectedGems = Array.Empty<string>();
                isProcessing = true;
                RunOCRBtn.IsEnabled = false;

                if (_cachedBitmap != null)
                {
                    var text = await Task.Run(() => HeistOCR.GetTextFromImage(_cachedBitmap));
                    DetectedGems = HeistOCR.GetGemsFromText(text);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                RunOCRBtn.IsEnabled = true;
                isProcessing = false;
            }
        }

        private async void PriceCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DetectedGems.Length > 0)
            {
                try
                {
                    PriceCheckBtn.IsEnabled = false;
                    var apiData = await PoeNinja.GetApiGemData();
                    var res = apiData.Where(item => DetectedGems.Any(find => item.Name.Equals(find, StringComparison.OrdinalIgnoreCase))).ToList();
                    Utils.ShowNotification(res);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    PriceCheckBtn.IsEnabled = true;
                }
            }
        }
    
        private static void InitializeSystemTrayIcon()
        {

            NotifyIcon ni = new()
            {
                Icon = new Icon("data/app.ico"),
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true
            };

            ni.ContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Exit", null, new EventHandler((s, e) => {System.Windows.Application.Current.Shutdown(); }), "Exit")
            }
            );
        }

        private async void PriceCheck(object sender, HotkeyEventArgs e)
        {
            try
            {
                using var screenshot = ImageProcessing.GetProcessedScreenshot();
                var result = HeistOCR.DetectHeistGems(screenshot);

                if (result.Length > 0)
                {
                    var apiData = await PoeNinja.GetApiGemData();
                    var res = apiData.Where(item => result.Any(find => item.Name.Equals(find, StringComparison.OrdinalIgnoreCase))).ToList();
                    Utils.ShowNotification(res);
                }
                else
                {
                    Utils.ShowNotification("No gems detected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

