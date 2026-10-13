using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Forms;

namespace FindTrueColor
{
    /// <summary>
    /// Interaction logic for PixelPickWindow.xaml
    /// </summary>
    public partial class PixelPickWindow : Window
    {
        public bool HasColor = false;
        public System.Drawing.Color PickedColor;
        public PixelPickWindow() { InitializeComponent(); WindowState = WindowState.Maximized; WindowStyle = WindowStyle.None; Opacity = 0.01; Topmost = true; }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var p = System.Windows.Forms.Cursor.Position;
            using (var g = Graphics.FromHwnd(System.IntPtr.Zero)) { }
            using (var bmp = new Bitmap(1, 1))
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.CopyFromScreen(p.X, p.Y, 0, 0, new System.Drawing.Size(1, 1));
                PickedColor = bmp.GetPixel(0, 0); HasColor = true;
            }
            Close();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
