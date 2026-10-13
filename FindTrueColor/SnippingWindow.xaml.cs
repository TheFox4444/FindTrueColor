using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace FindTrueColor
{
    /// <summary>
    /// Interaction logic for SnippingWindow.xaml
    /// </summary>
    public partial class SnippingWindow : Window
    {
        private System.Windows.Point startPoint;
        private bool isSelecting = false;
        private Drawing.Bitmap screenBitmap;

        public Drawing.Bitmap SelectedBitmap { get; private set; }

        public SnippingWindow(Drawing.Bitmap bitmap)
        {
            InitializeComponent();

            screenBitmap = bitmap;

            // Lấy vùng virtual screen (tất cả màn hình)
            var bounds = Forms.SystemInformation.VirtualScreen;

            // Set vị trí và kích thước window theo virtual screen
            Left = bounds.Left;
            Top = bounds.Top;
            Width = bounds.Width;
            Height = bounds.Height;

            imgScreen.Width = bounds.Width;
            imgScreen.Height = bounds.Height;

            overlay.Width = bounds.Width;
            overlay.Height = bounds.Height;

            imgScreen.Source = ConvertBitmapToBitmapSource(bitmap);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isSelecting = true;
            startPoint = e.GetPosition(this);

            selectionBorder.Visibility = Visibility.Visible;

            Canvas.SetLeft(selectionBorder, startPoint.X);
            Canvas.SetTop(selectionBorder, startPoint.Y);

            selectionBorder.Width = 0;
            selectionBorder.Height = 0;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSelecting) return;

            System.Windows.Point currentPoint = e.GetPosition(this);

            double x = Math.Min(currentPoint.X, startPoint.X);
            double y = Math.Min(currentPoint.Y, startPoint.Y);
            double w = Math.Abs(currentPoint.X - startPoint.X);
            double h = Math.Abs(currentPoint.Y - startPoint.Y);

            Canvas.SetLeft(selectionBorder, x);
            Canvas.SetTop(selectionBorder, y);

            selectionBorder.Width = w;
            selectionBorder.Height = h;

            UpdateOverlayMask(x, y, w, h);
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isSelecting = false;

            if (selectionBorder.Width > 1 && selectionBorder.Height > 1)
            {
                int x = (int)Canvas.GetLeft(selectionBorder);
                int y = (int)Canvas.GetTop(selectionBorder);
                int w = (int)selectionBorder.Width;
                int h = (int)selectionBorder.Height;

                var rect = new Drawing.Rectangle(x, y, w, h);
                SelectedBitmap = screenBitmap.Clone(rect, screenBitmap.PixelFormat);
            }

            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SelectedBitmap = null;
                Close();
            }
        }

        private void UpdateOverlayMask(double x, double y, double w, double h)
        {
            var group = new GeometryGroup
            {
                FillRule = FillRule.EvenOdd
            };

            // Toàn màn hình
            group.Children.Add(new RectangleGeometry(
                new Rect(0, 0, Width, Height)));

            // Vùng chọn (sẽ bị khoét)
            group.Children.Add(new RectangleGeometry(
                new Rect(x, y, w, h)));

            var drawing = new GeometryDrawing
            {
                Geometry = group,
                Brush = Brushes.White
            };

            var maskBrush = new DrawingBrush(drawing);

            overlay.OpacityMask = maskBrush;
        }

        private BitmapSource ConvertBitmapToBitmapSource(Drawing.Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, Drawing.Imaging.ImageFormat.Png);
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
    }
}
