using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FindTrueColor
{
    public static class ColorMath
    {
        public static void ToHSV(Color c, out double h, out double s, out double v)
        {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double d = max - min; v = max;
            s = max == 0 ? 0 : d / max;
            if (d == 0) h = 0;
            else if (max == r) h = 60 * (((g - b) / d) % 6);
            else if (max == g) h = 60 * (((b - r) / d) + 2);
            else h = 60 * (((r - g) / d) + 4);
            if (h < 0) h += 360;
        }
        public static void ToHalconHSV(Color c, out int h, out int s, out int v)
        {
            double hh, ss, vv; ToHSV(c, out hh, out ss, out vv);
            h = (int)(hh * 255 / 360); s = (int)(ss * 255); v = (int)(vv * 255);
        }
        public static void ToCMYK(Color c, out double cc, out double mm, out double yy, out double kk)
        {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            kk = 1 - Math.Max(r, Math.Max(g, b));
            if (kk >= 1) { cc = mm = yy = 0; return; }
            cc = (1 - r - kk) / (1 - kk); mm = (1 - g - kk) / (1 - kk); yy = (1 - b - kk) / (1 - kk);
        }
    }
}
