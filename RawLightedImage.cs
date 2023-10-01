using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Newtonsoft.Json;
// ReSharper disable RedundantCast

namespace FractureCommonLib
{
    // Based on RawImage with Lighting added
    [JsonObject(MemberSerialization.OptIn)]
    public class RawLightedImage : ICloneable
    {
        public const int Range = 255;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RawLightedImage(int width, int height, int depth)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            if (width < 1)
            {
                Debug.Assert(false);
                width = 1;
            }

            if (height < 1)
            {
                Debug.Assert(false);
                height = 1;
            }

            if (depth < 1)
            {
                Debug.Assert(false);
                depth = 1;
            }

            Width = width;
            Height = height;
            Depth = depth;

            PixelValues = new int[Width, Height];

            SetAllPixels(0);

            Lighting = new Vector3[Width, Height];

            SetAllLighting(new Vector3());
        }

        [JsonProperty]
        public int Width
        { get; set; }

        [JsonProperty]
        public int Height
        { get; set; }

        [JsonProperty]
        public int Depth
        { get; set; }

        [JsonProperty]
        protected int[,] PixelValues
        { get; set; }

        [JsonProperty]
        protected Vector3[,] Lighting
        { get; set; }

        public object Clone()
        {
            var ri = new RawLightedImage(Width, Height, Depth);

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    ri.PixelValues[x, y] = PixelValues[x, y];
                    var light = Lighting[x, y];
                    ri.Lighting[x,y] = new Vector3(light.X, light.Y, light.Z);
                }
            }

            return ri;
        }

        public void SetAllPixels(int z)
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                    PixelValues[x, y] = z;
            }
        }

        public void SetAllLighting(Vector3 lighting)
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                    Lighting[x, y] = lighting;
            }
        }

        public void SetPixel(int x, int y, int z, Vector3 light)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
            {
                Debug.Assert(false);
                return;
            }

            PixelValues[x, y] = z;
            Lighting[x, y] = light;
        }

        // see if there is a way to copy the arrays more efficiently
        public void SetBlock(int[,] pixels, Vector3[,] lighting, int fromWidth, int toWidth, int height, int depth)
        {
            if (height != Height)
                throw new ArgumentException("RawLightedImage SetBlock Width does not match");

            if(depth != Depth) 
                throw new ArgumentException("RawLightedImage SetBlock Depth does not match");

            if(fromWidth < 0 || fromWidth > Width)
                throw new ArgumentException("RawLightedImage SetBlock fromHeight does not match");

            if(toWidth < 0 || toWidth > Width)
                throw new ArgumentException("RawLightedImage SetBlock toHeight does not match");

            int subWidth = toWidth - fromWidth + 1; 

            int startValues = fromWidth * Height;
            int numberOfValues = subWidth * Height;

            Array.Copy(pixels, 0, PixelValues, startValues, numberOfValues);
            Array.Copy(lighting, 0, Lighting, startValues, numberOfValues);
        }

        public Bitmap GetBitmap(IPalette palette, DisplayInfo displayInfo, float ambientPower)
        {
            Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            if (palette.NumberOfColors != Depth)
            {
                Debug.Assert(false);
                return bmp;
            }

            switch (displayInfo.Mode)
            {
                case DisplayMode.Contrast:
                    return GetContrastBitmap(palette, bmp, displayInfo, ambientPower);
                case DisplayMode.Hsl when displayInfo.Hue:
                {
                    if (displayInfo.Saturation)
                    {
                        if (displayInfo.Lightness)
                            return GetHslBitmap(palette, bmp, displayInfo, ambientPower);
                        else
                            return GetHueSatBitmap(palette, bmp, displayInfo, ambientPower);
                    }
                    else if (displayInfo.Lightness)
                        return GetHueLightnessBitmap(palette, bmp, displayInfo, ambientPower);
                    else
                        return GetHueBitmap(palette, bmp, displayInfo, ambientPower);
                }
                case DisplayMode.Hsl when displayInfo.Saturation:
                {
                    if (displayInfo.Lightness)
                        return GetSaturationLightnessBitmap(palette, bmp, displayInfo, ambientPower);
                    else
                        return GetSaturationBitmap(palette, bmp, displayInfo, ambientPower);
                }
                case DisplayMode.Hsl when displayInfo.Lightness:
                    return GetLightnessBitmap(palette, bmp, displayInfo, ambientPower);
                default:
                    return GetBitmap(palette, bmp, ambientPower);
            }
        }

        private Bitmap GetContrastBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
        {
            double[] stretch = { 0.0, 0.0, 0.0 };
            for (int x = 0; x < 3; x++)
                // ReSharper disable once RedundantCast
                stretch[x] = ((double)Range / (double)(displayInfo.MaxRgb[x] - displayInfo.MinRgb[x]));


            Rectangle imageRect = new Rectangle(0, 0, Width, Height);
            BitmapData bmpData;
            IntPtr intptr;

            try
            {
                bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                intptr = bmpData.Scan0;
            }
            catch (Exception ex)
            {
                throw new ImageException("RawImage.GetBitmap(): failed during LockBits()", ex);
            }

            // copy colors
            unsafe
            {
                double r, g, b;
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        int pos = bmpData.Stride * y + x * 4;
                        Color pixelColor = palette.GetColor(PixelValues[x, y]);
                        byte* pixel = (byte*)intptr;

                        b = ((double)pixelColor.B - (double)displayInfo.MinRgb[0]) * stretch[0] + 0.49999;
                        g = ((double)pixelColor.G - (double)displayInfo.MinRgb[1]) * stretch[1] + 0.49999;
                        r = ((double)pixelColor.R - (double)displayInfo.MinRgb[2]) * stretch[2] + 0.49999;

                        Color palColor = Color.FromArgb(pixelColor.A, (byte)Math.Min(Math.Max(0, (int)(r)), Range),
                            (byte)Math.Min(Math.Max(0, (int)(g)), Range), (byte)Math.Min(Math.Max(0, (int)(b)), Range));

                        var finalColor = LightingUtil.CalculateLight(palColor, Lighting[x, y], ambientPower);

                        pixel[pos] = finalColor.B;
                        pixel[pos + 1] = finalColor.G;
                        pixel[pos + 2] = finalColor.R;
                        pixel[pos + 3] = finalColor.A;
                    }
                }
            }

            // unlock
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public struct DbPair
        {
            public double MinDb;
            public double MaxDb;
        }

        private DbPair GetMinMaxHue(IPalette palette, Bitmap bmp)
        {
            DbPair sp = new DbPair();
            sp.MaxDb = 0.0;
            sp.MinDb = 360.0;

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Color pixelColor = palette.GetColor(PixelValues[x, y]);
                    HSL hsl = HslConvertor.ToHsl(pixelColor);
                    sp.MinDb = Math.Min(sp.MinDb, hsl.H);
                    sp.MaxDb = Math.Max(sp.MaxDb, hsl.H);
                }
            }

            return sp;
        }

        private DbPair GetMinMaxSaturation(IPalette palette, Bitmap bmp)
        {
            DbPair sp = new DbPair();
            sp.MaxDb = 0.0;
            sp.MinDb = 1.0;

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Color pixelColor = palette.GetColor(PixelValues[x, y]);
                    HSL hsl = HslConvertor.ToHsl(pixelColor);
                    sp.MinDb = Math.Min(sp.MinDb, hsl.S);
                    sp.MaxDb = Math.Max(sp.MaxDb, hsl.S);
                }
            }

            return sp;
        }

        private DbPair GetMinMaxLightness(IPalette palette, Bitmap bmp)
        {
            DbPair sp = new DbPair();
            sp.MaxDb = 0.0;
            sp.MinDb = 1.0;

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Color pixelColor = palette.GetColor(PixelValues[x, y]);
                    HSL hsl = HslConvertor.ToHsl(pixelColor);
                    sp.MinDb = Math.Min(sp.MinDb, hsl.L);
                    sp.MaxDb = Math.Max(sp.MaxDb, hsl.L);
                }
            }

            return sp;
        }

        private Bitmap GetHslBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
        {
            // find min and max sat
            DbPair huePr = GetMinMaxHue(palette, bmp);
            DbPair satPr = GetMinMaxSaturation(palette, bmp);
            DbPair lightPr = GetMinMaxLightness(palette, bmp);

            // figure scale factor
            double satScaleFactor = 1.0;
            if (satPr.MaxDb > satPr.MinDb)
                satScaleFactor = (displayInfo.MaxSaturation - displayInfo.MinSaturation) / (satPr.MaxDb - satPr.MinDb);

            double lightScaleFactor = 1.0;
            if (lightPr.MaxDb > lightPr.MinDb)
                lightScaleFactor = (displayInfo.MaxLightness - displayInfo.MinLightness) / (lightPr.MaxDb - lightPr.MinDb);

            double hueScaleFactor = 1.0;
            if (huePr.MaxDb > huePr.MinDb)
                hueScaleFactor = (displayInfo.MaxHue - displayInfo.MinHue) / (huePr.MaxDb - huePr.MinDb);

            Rectangle imageRect = new Rectangle(0, 0, Width, Height);
            BitmapData bmpData;
            IntPtr intptr;

            try
            {
                bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                intptr = bmpData.Scan0;
            }
            catch (Exception ex)
            {
                throw new ImageException("RawImage.GetBitmap(): failed during LockBits()", ex);
            }

            // copy colors
            unsafe
            {
                // For each pixel
                //      Calculate hsl
                //      Calculate new hsl
                //      Convert to new HSL
                //      Convert new HSL to RGB and set
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        int pos = bmpData.Stride * y + x * 4;
                        Color pixelColor = palette.GetColor(PixelValues[x, y]);
                        byte* pixel = (byte*)intptr;

                        HSL hsl = HslConvertor.ToHsl(pixelColor);

                        double newSat = displayInfo.MinSaturation + (hsl.S - satPr.MinDb) * satScaleFactor;
                        hsl.S = newSat;

                        double newLight = displayInfo.MinLightness + (hsl.L - lightPr.MinDb) * lightScaleFactor;
                        hsl.L = newLight;

                        double newHue = displayInfo.MinHue + (hsl.H - huePr.MinDb) * hueScaleFactor;
                        hsl.H = newHue;

                        Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);

                        var lite = Lighting[x, y];
                        Color finalColor = LightingUtil.CalculateLight(newColor, lite, ambientPower);

                        pixel[pos] = finalColor.B;
                        pixel[pos + 1] = finalColor.G;
                        pixel[pos + 2] = finalColor.R;
                        pixel[pos + 3] = finalColor.A;
                    }
                }
            }

            // unlock
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private Bitmap GetHueSatBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
        {
            // find min and max sat
            DbPair huePr = GetMinMaxHue(palette, bmp);
            DbPair satPr = GetMinMaxSaturation(palette, bmp);

            // figure scale factor
            double satScaleFactor = 1.0;
            if (satPr.MaxDb > satPr.MinDb)
                satScaleFactor = (displayInfo.MaxSaturation - displayInfo.MinSaturation) / (satPr.MaxDb - satPr.MinDb);

            double hueScaleFactor = 1.0;
            if (huePr.MaxDb > huePr.MinDb)
                hueScaleFactor = (displayInfo.MaxHue - displayInfo.MinHue) / (huePr.MaxDb - huePr.MinDb);

            Rectangle imageRect = new Rectangle(0, 0, Width, Height);
            BitmapData bmpData;
            IntPtr intptr;

            try
            {
                bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                intptr = bmpData.Scan0;
            }
            catch (Exception ex)
            {
                throw new ImageException("RawImage.GetBitmap(): failed during LockBits()", ex);
            }

            // copy colors
            unsafe
            {
                // For each pixel
                //      Calculate hsl
                //      Calculate new hsl
                //      Convert to new HSL
                //      Convert new HSL to RGB and set
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        int pos = bmpData.Stride * y + x * 4;
                        Color pixelColor = palette.GetColor(PixelValues[x, y]);
                        byte* pixel = (byte*)intptr;

                        HSL hsl = HslConvertor.ToHsl(pixelColor);

                        double newSat = displayInfo.MinSaturation + (hsl.S - satPr.MinDb) * satScaleFactor;
                        hsl.S = newSat;

                        double newHue = displayInfo.MinHue + (hsl.H - huePr.MinDb) * hueScaleFactor;
                        hsl.H = newHue;

                        Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);
                        var lite = Lighting[x, y];
                        Color finalColor = LightingUtil.CalculateLight(newColor, lite, ambientPower);

                        pixel[pos] = finalColor.B;
                        pixel[pos + 1] = finalColor.G;
                        pixel[pos + 2] = finalColor.R;
                        pixel[pos + 3] = finalColor.A;
                    }
                }
            }

            // unlock
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private Bitmap GetHueLightnessBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
        {
            // find min and max sat
            DbPair huePr = GetMinMaxHue(palette, bmp);
            DbPair lightPr = GetMinMaxLightness(palette, bmp);

            // figure scale factor
            double lightScaleFactor = 1.0;
            if (lightPr.MaxDb > lightPr.MinDb)
                lightScaleFactor = (displayInfo.MaxLightness - displayInfo.MinLightness) / (lightPr.MaxDb - lightPr.MinDb);

            double hueScaleFactor = 1.0;
            if (huePr.MaxDb > huePr.MinDb)
                hueScaleFactor = (displayInfo.MaxHue - displayInfo.MinHue) / (huePr.MaxDb - huePr.MinDb);

            Rectangle imageRect = new Rectangle(0, 0, Width, Height);
            BitmapData bmpData;
            IntPtr intptr;

            try
            {
                bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                intptr = bmpData.Scan0;
            }
            catch (Exception ex)
            {
                throw new ImageException("RawImage.GetBitmap(): failed during LockBits()", ex);
            }

            // copy colors
            unsafe
            {
                // For each pixel
                //      Calculate hsl
                //      Calculate new hsl
                //      Convert to new HSL
                //      Convert new HSL to RGB and set
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        int pos = bmpData.Stride * y + x * 4;
                        Color pixelColor = palette.GetColor(PixelValues[x, y]);
                        byte* pixel = (byte*)intptr;

                        HSL hsl = HslConvertor.ToHsl(pixelColor);

                        double newLight = displayInfo.MinLightness + (hsl.L - lightPr.MinDb) * lightScaleFactor;
                        hsl.L = newLight;

                        double newHue = displayInfo.MinHue + (hsl.H - huePr.MinDb) * hueScaleFactor;
                        hsl.H = newHue;

                        Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);
                        var lite = Lighting[x, y];
                        Color finalColor = LightingUtil.CalculateLight(newColor, lite, ambientPower);

                        pixel[pos] = finalColor.B;
                        pixel[pos + 1] = finalColor.G;
                        pixel[pos + 2] = finalColor.R;
                        pixel[pos + 3] = finalColor.A;
                    }
                }
            }

            // unlock
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private Bitmap GetHueBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
        {
            // find min and max sat
            DbPair sp = GetMinMaxHue(palette, bmp);

            // figure scale factor
            double scaleFactor = 1.0;
            if (sp.MaxDb > sp.MinDb)
                scaleFactor = (displayInfo.MaxHue - displayInfo.MinHue) / (sp.MaxDb - sp.MinDb);

            Rectangle imageRect = new Rectangle(0, 0, Width, Height);
            BitmapData bmpData;
            IntPtr intptr;

            try
            {
                bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                intptr = bmpData.Scan0;
            }
            catch (Exception ex)
            {
                throw new ImageException("RawImage.GetBitmap(): failed during LockBits()", ex);
            }

            // copy colors
            unsafe
            {
                // For each pixel
                //      Calculate hue
                //      Calculate new hue
                //      Convert to new HSL
                //      Convert new HSL to RGB and set
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        int pos = bmpData.Stride * y + x * 4;
                        Color pixelColor = palette.GetColor(PixelValues[x, y]);
                        byte* pixel = (byte*)intptr;

                        HSL hsl = HslConvertor.ToHsl(pixelColor);

                        double newHue = displayInfo.MinSaturation + (hsl.H - sp.MinDb) * scaleFactor;
                        hsl.H = newHue;

                        Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);
                        var lite = Lighting[x, y];
                        Color finalColor = LightingUtil.CalculateLight(newColor, lite, ambientPower);

                        pixel[pos] = finalColor.B;
                        pixel[pos + 1] = finalColor.G;
                        pixel[pos + 2] = finalColor.R;
                        pixel[pos + 3] = finalColor.A;
                    }
                }
            }

            // unlock
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private Bitmap GetSaturationLightnessBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
        {
            // find min and max sat
            DbPair satPr = GetMinMaxSaturation(palette, bmp);
            DbPair lightPr = GetMinMaxLightness(palette, bmp);

            // figure scale factor
            double satScaleFactor = 1.0;
            if (satPr.MaxDb > satPr.MinDb)
                satScaleFactor = (displayInfo.MaxSaturation - displayInfo.MinSaturation) / (satPr.MaxDb - satPr.MinDb);

            double lightScaleFactor = 1.0;
            if (lightPr.MaxDb > lightPr.MinDb)
                lightScaleFactor = (displayInfo.MaxLightness - displayInfo.MinLightness) / (lightPr.MaxDb - lightPr.MinDb);

            Rectangle imageRect = new Rectangle(0, 0, Width, Height);
            BitmapData bmpData;
            IntPtr intptr;

            try
            {
                bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                intptr = bmpData.Scan0;
            }
            catch (Exception ex)
            {
                throw new ImageException("RawImage.GetBitmap(): failed during LockBits()", ex);
            }

            // copy colors
            unsafe
            {
                // For each pixel
                //      Calculate hsl
                //      Calculate new hsl
                //      Convert to new HSL
                //      Convert new HSL to RGB and set
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        int pos = bmpData.Stride * y + x * 4;
                        Color pixelColor = palette.GetColor(PixelValues[x, y]);
                        byte* pixel = (byte*)intptr;

                        HSL hsl = HslConvertor.ToHsl(pixelColor);

                        double newSat = displayInfo.MinSaturation + (hsl.S - satPr.MinDb) * satScaleFactor;
                        hsl.S = newSat;

                        double newLight = displayInfo.MinLightness + (hsl.L - lightPr.MinDb) * lightScaleFactor;
                        hsl.L = newLight;

                        Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);
                        var lite = Lighting[x, y];
                        Color finalColor = LightingUtil.CalculateLight(newColor, lite, ambientPower);

                        pixel[pos] = finalColor.B;
                        pixel[pos + 1] = finalColor.G;
                        pixel[pos + 2] = finalColor.R;
                        pixel[pos + 3] = finalColor.A;
                    }
                }
            }

            // unlock
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private Bitmap GetSaturationBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
        {
            // find min and max sat
            DbPair sp = GetMinMaxSaturation(palette, bmp);

            // figure scale factor
            double scaleFactor = 1.0;
            if (sp.MaxDb > sp.MinDb)
                scaleFactor = (displayInfo.MaxSaturation - displayInfo.MinSaturation) / (sp.MaxDb - sp.MinDb);

            Rectangle imageRect = new Rectangle(0, 0, Width, Height);
            BitmapData bmpData;
            IntPtr intptr;

            try
            {
                bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                intptr = bmpData.Scan0;
            }
            catch (Exception ex)
            {
                throw new ImageException("RawImage.GetBitmap(): failed during LockBits()", ex);
            }

            // copy colors
            unsafe
            {
                // For each pixel
                //      Calculate sat
                //      Calculate new sat
                //      Convert to new HSL
                //      Convert new HSL to RGB and set
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        int pos = bmpData.Stride * y + x * 4;
                        Color pixelColor = palette.GetColor(PixelValues[x, y]);
                        byte* pixel = (byte*)intptr;

                        HSL hsl = HslConvertor.ToHsl(pixelColor);

                        double newSat = displayInfo.MinSaturation + (hsl.S - sp.MinDb) * scaleFactor;
                        hsl.S = newSat;

                        Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);
                        var lite = Lighting[x, y];
                        Color finalColor = LightingUtil.CalculateLight(newColor, lite, ambientPower);

                        pixel[pos] = finalColor.B;
                        pixel[pos + 1] = finalColor.G;
                        pixel[pos + 2] = finalColor.R;
                        pixel[pos + 3] = finalColor.A;
                    }
                }
            }

            // unlock
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private Bitmap GetLightnessBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
        {
            // find min and max sat
            DbPair sp = GetMinMaxLightness(palette, bmp);

            // figure scale factor
            double scaleFactor = 1.0;
            if (sp.MaxDb > sp.MinDb)
                scaleFactor = (displayInfo.MaxLightness - displayInfo.MinLightness) / (sp.MaxDb - sp.MinDb);

            Rectangle imageRect = new Rectangle(0, 0, Width, Height);
            BitmapData bmpData;
            IntPtr intptr;

            try
            {
                bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                intptr = bmpData.Scan0;
            }
            catch (Exception ex)
            {
                throw new ImageException("RawImage.GetBitmap(): failed during LockBits()", ex);
            }

            // copy colors
            unsafe
            {
                // For each pixel
                //      Calculate sat
                //      Calculate new sat
                //      Convert to new HSL
                //      Convert new HSL to RGB and set
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        int pos = bmpData.Stride * y + x * 4;
                        Color pixelColor = palette.GetColor(PixelValues[x, y]);
                        byte* pixel = (byte*)intptr;

                        HSL hsl = HslConvertor.ToHsl(pixelColor);

                        double newLt = displayInfo.MinLightness + (hsl.L - sp.MinDb) * scaleFactor;
                        hsl.L = newLt;

                        Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);
                        var lite = Lighting[x, y];
                        Color finalColor = LightingUtil.CalculateLight(newColor, lite, ambientPower);

                        pixel[pos] = finalColor.B;
                        pixel[pos + 1] = finalColor.G;
                        pixel[pos + 2] = finalColor.R;
                        pixel[pos + 3] = finalColor.A;
                    }
                }
            }

            // unlock
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private Bitmap GetBitmap(IPalette palette, Bitmap bmp, float ambientPower)
        {
            Rectangle imageRect = new Rectangle(0, 0, Width, Height);
            BitmapData bmpData;
            IntPtr intptr;

            try
            {
                bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                intptr = bmpData.Scan0;
            }
            catch (Exception ex)
            {
                throw new ImageException("RawImage.GetBitmap(): failed during LockBits()", ex);
            }

            // copy colors
            unsafe
            {
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        int pos = bmpData.Stride * y + x * 4;
                        Color palColor = palette.GetColor(PixelValues[x, y]);
                        var lite = Lighting[x, y];
                        Color pixelColor = LightingUtil.CalculateLight(palColor, lite, ambientPower);

                        byte* pixel = (byte*)intptr;

                        pixel[pos] = pixelColor.B;
                        pixel[pos + 1] = pixelColor.G;
                        pixel[pos + 2] = pixelColor.R;
                        pixel[pos + 3] = pixelColor.A;
                    }
                }
            }

            // unlock
            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}
