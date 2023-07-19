using System.Drawing;
using System.Globalization;

namespace FractureCommonLib
{
    [Serializable]
    public class ColorPoint
    {
        // field members
        //////////////////////////////////////////////////////////

        private Color _pointColor = Color.White;

        private double _position;


        // constructors
        ////////////////////////////////////////////////////////// 

        public ColorPoint()
        {}

        public ColorPoint(Color pointColor, double position)
        {
            // alpha a == 0 is fully transparent (alpha, red, green, blue)
            _pointColor = pointColor;
            _position = position;
        }


        // IColorPoint overrides
        ////////////////////////////////////////////////////////// 

        // The color that applies to this index
        // There may be different colors for before and after the Color Point index
        public Color PointColor
        {
            get => _pointColor;

            set => _pointColor = value;
        }

        // Value between 0 and 1.0 where 1.0 maps to the highest color
        public double Position
        {
            get => _position;
            set
            {
                if(value < 0.0 || value > 1.0 )
                {
                    string msg = "Position value should be between zero and 1 but was: ";
                    msg += value.ToString(CultureInfo.InvariantCulture);
                    throw new ColorPointException(msg);
                }

                _position = value;
            }
        }

        // The color index in a palette this point maps to
        public int GetColorIndex(int numberOfColors)
        {
            if (numberOfColors < 2)
            {
                string msg = "Palettes should have at least 2 colors, this one has: ";
                msg += numberOfColors.ToString();
                throw new ColorPointException(msg);
            }
            int index = (int)(_position * (numberOfColors-1));
            return index;
        }


       public void SetPositionByIndex(int index, int numberOfColors)
        {
           if (index < 0)
           {
               string msg = "ColorPoint.SetPositionByIndex() index is less than zero: ";
               msg += index.ToString();
               throw new ColorPointException(msg);
           }

           if (numberOfColors < 2)
           {
               string msg = "ColorPoint.SetPositionByIndex()numberOfColors is less than 2: ";
               msg += numberOfColors.ToString();
               throw new ColorPointException(msg);
           }

           if (numberOfColors <= index)
           {
               string msg = "ColorPoint.SetPositionByIndex() index is greater than or equal the number of colors";
               throw new ColorPointException(msg);
           }

           _position = ( (double)index ) / (numberOfColors-1);
        }


        public ColorPoint Clone()
        {
            var newPt = new ColorPoint(_pointColor, _position);
            return newPt;
        }

    }
}
