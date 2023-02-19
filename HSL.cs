namespace FractureCommonLib
{
    public struct HSL
    {
        private double _h;
        public double H
        {
            get { return _h; }
            set 
            {
                double h = value;
                while (h < 0.0)
                    h += 360.0;

                while (h >= 360.0)
                    h -= 360.0;

                _h = h; 
            }
        }

        private double _s;
        public double S
        {
            get { return _s; }
            set 
            {
                double s = value;
                if (s < 0.0)
                    s = 0.0;
                if (s > 1.0)
                    s = 1.0;
                _s = s; 
            }
        }

        private double _l;
        public double L
        {
            get { return _l; }
            set 
            {
                double l = value;
                if (l < 0.0)
                    l = 0.0;
                if (l > 1.0)
                    l = 1.0;
                _l = l;
            }
        }

        public HSL(double h = 0.0, double s = 0.0, double l = 0.0)
        {
            _h = h;
            _s = s;
            _l = l;
        }
    }
}
