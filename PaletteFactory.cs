using System.Drawing;

namespace FractureCommonLib
{
    static public class PaletteFactory
    {
        // throws PaletteException
        // Palettes must have more than 2 colors
        public static Palette CreateStandardPalette(int numberOfColors)
        {
            if(numberOfColors < 2)
            {
                string msg = "Cannot create a palette with less than 2 colors!";
                throw new PaletteException(msg);
            }

            Palette newPalette = new Palette(numberOfColors);

            // add minimum color points
            ColorPoint black = new ColorPoint(Color.Black, 0.0);
            newPalette.AddColorPoint(black);

            ColorPoint white = new ColorPoint(Color.White, 1.0);
            newPalette.AddColorPoint(white);

            if(numberOfColors < 8)
                return newPalette;

            // else add some color points
            double subDivision = 1.0 / 7.0;

            ColorPoint magenta = new ColorPoint(Color.Magenta, subDivision);
            newPalette.AddColorPoint(magenta);

            ColorPoint blue = new ColorPoint(Color.Blue, 2*subDivision);
            newPalette.AddColorPoint(blue);

            ColorPoint turquoise = new ColorPoint(Color.Turquoise, 3*subDivision);
            newPalette.AddColorPoint(turquoise);

            ColorPoint green = new ColorPoint(Color.Green, 4*subDivision);
            newPalette.AddColorPoint(green);

            ColorPoint yellow = new ColorPoint(Color.Yellow, 5*subDivision);
            newPalette.AddColorPoint(yellow);

            ColorPoint red = new ColorPoint(Color.Red, 6*subDivision);
            newPalette.AddColorPoint(red);

            return newPalette;
        }

        public static Palette CreatePaletteFromPins(int numberOfColors, List<ColorPoint> pts)
        {
            int numberOfPins = pts.Count;
            Palette newPalette = new Palette(numberOfColors);

            if (numberOfPins < 1)
            {
                // add minimum color points
                ColorPoint black = new ColorPoint(Color.Black, 0.0);
                newPalette.AddColorPoint(black);

                ColorPoint white = new ColorPoint(Color.White, 1.0);
                newPalette.AddColorPoint(white);

                return newPalette;
            }

            foreach(ColorPoint p in pts)
            {
                newPalette.AddColorPoint(p);
            }

            return newPalette;
        }

        // Create a palette with 2 pins one on each end with color1 at 0 and color1 at numberOfColors-1
        public static IPalette CreateTwoPinPalette(int numberOfColors, Color color1, Color color2)
        {
            if (numberOfColors < 2)
            {
                string msg = "Cannot create a palette with less than 2 colors!";
                throw new PaletteException(msg);
            }

            Palette newPalette = new Palette(numberOfColors);

            // add minimum color points
            ColorPoint low = new ColorPoint(color1, 0.0);
            newPalette.AddColorPoint(low);

            ColorPoint high = new ColorPoint(color2, 1.0);
            newPalette.AddColorPoint(high);

            return newPalette;
        }

    }
}
