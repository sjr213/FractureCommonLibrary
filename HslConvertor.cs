using System.Drawing;

namespace FractureCommonLib
{
    public static class HslConvertor
    {
        private static RgbType.RgbPart GetMaxComponent(Color rgb, byte min, byte max)
        {
            if (max == min)
                return RgbType.RgbPart.total;

            if (rgb.R == max)
                return RgbType.RgbPart.red;

            if(rgb.G == max)
                return RgbType.RgbPart.green;

            if (rgb.B == max)
                return RgbType.RgbPart.blue;

            return RgbType.RgbPart.blue;
        }

        private static double GetHue(Color rgb, byte min, byte max, RgbType.RgbPart maxComponent)
        {
            if (maxComponent == RgbType.RgbPart.total)
                return 0.0;

            double C = max - min;

            if (maxComponent == RgbType.RgbPart.red)
            {
                double h = (rgb.G - rgb.B) / C % 6;
                return 60.0 * h;
            }

            if (maxComponent == RgbType.RgbPart.green)
            {
                double h = (rgb.B - rgb.R) / C + 2.0;
                return 60.0 * h;
            }

            if (maxComponent == RgbType.RgbPart.blue)
            {
                double h = (rgb.R - rgb.G) / C + 4.0;
                return 60.0 * h;
            }

            return 0.0;
        }

        private static double GetLightness(byte max, byte min)
        {
            double dMax = max / 255.0;
            double dMin = min / 255.0;
            return (dMin + dMax) / 2;
        }

        private static double GetSaturation(byte max, byte min, double l)
        {
            if (min == max)
                return 0.0;

            double dMax = max / 255.0;
            double dMin = min / 255.0;

            if (l <= 0.5)
            {
                return (dMax - dMin) / (2 * l);
            }

            return (dMax - dMin) / (2.0 - 2 * l);
        }

        public static HSL ToHsl(Color rgb)
        {
            HSL hsl = new HSL();
            byte max = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
            byte min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);

            RgbType.RgbPart maxComponent = GetMaxComponent(rgb, min, max);

            hsl.H = GetHue(rgb, min, max, maxComponent);
            hsl.L = GetLightness(max, min);
            hsl.S = GetSaturation(max, min, hsl.L);

            return hsl;
        }

        public static Color ToRgb(HSL hsl, byte a)
        {
            double c = (1 - Math.Abs(2 * hsl.L - 1)) * hsl.S;
            double h = hsl.H / 60.0;
            double x = c * (1.0 - Math.Abs(h % 2 - 1));

            double r = 0, g = 0, b = 0;
            if (h >= 0 && h < 1.0)
            {
                r = c;
                g = x;
                b = 0.0;
            }
            else if (h >= 1.0 && h < 2.0)
            {
                r = x;
                g = c;
                b = 0.0;
            }
            else if (h >= 2.0 && h < 3.0)
            {
                r = 0.0;
                g = c;
                b = x;
            }
            else if (h >= 3.0 && h < 4.0)
            {
                r = 0.0;
                g = x;
                b = c;
            }
            else if (h >= 4.0 && h < 5.0)
            {
                r = x;
                g = 0.0;
                b = c;
            }
            else if (h >= 5.0 && h < 6.0)
            {
                r = c;
                g = 0.0;
                b = x;
            }
            else
            {
                r = 0.0;
                g = 0.0;
                b = 0.0;
            }

            double m = hsl.L - 0.5 * c;

            RGB rgb = new RGB(r + m, g + m, b + m);

            return rgb.ToColor(a);
        }


    }
}
