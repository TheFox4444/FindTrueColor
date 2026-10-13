using FindTrueColor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Forms = System.Windows.Forms;

namespace FindTrueColor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap snipBitmap;
        private System.Drawing.Color currentColor;

        public MainWindow()
        {
            InitializeComponent();

            // wire handler and then set SelectedIndex so SelectionChanged executes only after all controls are created
            cbMode.SelectionChanged += cbMode_SelectionChanged;
            cbMode.SelectedIndex = 0;
        }

        private async void BtnSnip_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            await System.Threading.Tasks.Task.Delay(200);

            var bounds = Forms.SystemInformation.VirtualScreen;
            Bitmap bmp = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
                g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);

            var snip = new SnippingWindow(bmp);
            snip.ShowDialog();

            Show();
            Activate();

            if (snip.SelectedBitmap != null)
            {
                snipBitmap = snip.SelectedBitmap;
                imgSnip.Source = ToSource(snipBitmap);
                ApplyAverageColor(snipBitmap);
            }
        }

        private async void BtnAverage_Click(object sender, RoutedEventArgs e)
        {
            if (snipBitmap == null) return;
            //Hide();
            //await System.Threading.Tasks.Task.Delay(200);
            ApplyAverageColor(snipBitmap);
            //Show();
            //Activate();
        }

        private async void BtnChoosePixel_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            await System.Threading.Tasks.Task.Delay(200);
            var picker = new PixelPickWindow();
            picker.ShowDialog();
            Show();
            Activate();
            if (picker.HasColor)
                SetColor(picker.PickedColor);
        }

        private void imgSnip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (snipBitmap == null) return;
            var p = e.GetPosition(imgSnip);
            double sx = snipBitmap.Width / imgSnip.ActualWidth;
            double sy = snipBitmap.Height / imgSnip.ActualHeight;
            int x = Math.Min(snipBitmap.Width - 1, Math.Max(0, (int)(p.X * sx)));
            int y = Math.Min(snipBitmap.Height - 1, Math.Max(0, (int)(p.Y * sy)));
            SetColor(snipBitmap.GetPixel(x, y));
        }

        // Convert System.Drawing.Bitmap to WPF BitmapSource
        private BitmapSource ToSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();

                return image;
            }
        }

        // Compute average color and apply
        private void ApplyAverageColor(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null) return;

            long r = 0, g = 0, b = 0;
            int w = bitmap.Width, h = bitmap.Height;
            long total = 0;

            // sample every N pixels if image is large to keep it responsive
            int step = Math.Max(1, Math.Min(w, h) / 200); // adjust sampling density
            for (int y = 0; y < h; y += step)
            {
                for (int x = 0; x < w; x += step)
                {
                    var p = bitmap.GetPixel(x, y);
                    r += p.R;
                    g += p.G;
                    b += p.B;
                    total++;
                }
            }

            if (total == 0) return;
            var avg = System.Drawing.Color.FromArgb(
                255,
                (int)(r / total),
                (int)(g / total),
                (int)(b / total));

            SetColor(avg);
        }

        // Update UI with selected color and conversions
        private void SetColor(System.Drawing.Color c)
        {
            currentColor = c;
            if (bdColor == null) return; // controls not ready

            // Update color preview (bdColor), RGB and Hex
            var mediaColor = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            bdColor.Background = new SolidColorBrush(mediaColor);
            txtRgb.Text = $"RGB: {c.R}, {c.G}, {c.B}";
            txtHex.Text = $"#{c.R:X2}{c.G:X2}{c.B:X2}";

            // Update conversions based on cbMode
            if (cbMode == null) return;
            switch (cbMode.SelectedIndex)
            {
                case 0: // CMYK
                    ColorMath.ToCMYK(mediaColor, out double cc, out double mm, out double yy, out double kk);
                    txtConvert1.Text = $"C: {cc:F3}";
                    txtConvert2.Text = $"M: {mm:F3}";
                    txtConvert3.Text = $"Y: {yy:F3}";
                    txtConvert4.Text = $"K: {kk:F3}";
                    break;
                case 1: // HSV
                    ColorMath.ToHSV(mediaColor, out double h, out double s, out double v);
                    txtConvert1.Text = $"H: {h:F1}";
                    txtConvert2.Text = $"S: {s:F3}";
                    txtConvert3.Text = $"V: {v:F3}";
                    txtConvert4.Text = "";
                    break;
                case 2: // HSV (HALCON)
                    ColorMath.ToHalconHSV(mediaColor, out int hh, out int ss, out int vv);
                    txtConvert1.Text = $"H: {hh}";
                    txtConvert2.Text = $"S: {ss}";
                    txtConvert3.Text = $"V: {vv}";
                    txtConvert4.Text = "";
                    break;
                default:
                    txtConvert1.Text = txtConvert2.Text = txtConvert3.Text = txtConvert4.Text = "";
                    break;
            }
        }

        private void cbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // refresh conversion output for current color
            SetColor(currentColor);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => SetColor(currentColor);
    }
}


