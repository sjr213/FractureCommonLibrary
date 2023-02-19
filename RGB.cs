using System.Drawing;

namespace FractureCommonLib
{
    public struct RGB
    {
        private double _r;
        public double R
        {
            get { return _r; }
            set 
            {
                double r = value;
                if (r < 0.0)
                    r = 0.0;
                if (r > 1.0)
                    r = 1.0;

                _r = r; 
            }
        }
        private double _g;
        public double G
        {
            get { return _g; }
            set 
            {
                double g = value;
                if(g < 0.0)
                    g = 0.0;
                if (g > 1.0)
                    g = 1.0;

                _g = g;
            }
        }
        private double _b;
        public double B
        {
            get { return _b; }
            set 
            {
                double b = value;
                if(b < 0.0)
                    b = 0.0;
                if(b > 1.0)
                    b = 1.0;
                _b = b;
            }
        }

        public RGB(double r = 0.0, double g = 0.0, double b = 0.0)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public Color ToColor(byte a)
        {
            byte r = Math.Max((byte)0, Math.Min((byte)(R * 255 + 0.5), (byte)255));
            byte g = Math.Max((byte)0, Math.Min((byte)(G * 255 + 0.5), (byte)255));
            byte b = Math.Max((byte)0, Math.Min((byte)(B * 255 + 0.5), (byte)255));

            Color color = Color.FromArgb(a, r, g, b);

            return color;
        }
    }
}
