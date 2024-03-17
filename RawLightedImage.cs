using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Newtonsoft.Json;
// ReSharper disable RedundantCast

namespace FractureCommonLib;

[Serializable]
public enum ImageMode
{
    [Description("Depth")]
    Depth,
    [Description("Color")]
    Color,
}

// Based on RawImage with Lighting added
[JsonObject(MemberSerialization.OptIn)]
public class RawLightedImage : ICloneable    
{
    public const int Range = 255;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public RawLightedImage(int width, int height, int depth)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Mode = ImageMode.Depth;

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

    public RawLightedImage(int width, int height)
    {
        Mode = ImageMode.Color;

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

        Width = width;
        Height = height;

        PixelValues = new int[1, 1];

        Lighting = new Vector3[1, 1];

        ColorValues = new Color[Width, Height];
    }

    [JsonConstructor]
    public RawLightedImage() : this(1, 1)
    {
        ColorValues = new Color[1, 1];
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

    [DefaultValue(ImageMode.Depth)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    protected ImageMode Mode
    { get; set; }

    [JsonProperty]
    protected Color[,] ColorValues
    { get; set; }

    public object Clone()
    {
        if (Mode == ImageMode.Depth)
        {
            var ri = new RawLightedImage(Width, Height, Depth);

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    ri.PixelValues[x, y] = PixelValues[x, y];
                    var light = Lighting[x, y];
                    ri.Lighting[x, y] = new Vector3(light.X, light.Y, light.Z);
                }
            }

            return ri;
        }
        else // if(Mode == ImageMode.Color)
        {
            var ri = new RawLightedImage(Width, Height);

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    ri.ColorValues[x, y] = ColorValues[x, y];
                }
            }

            return ri;
        }

    }

    // Depth mode only
    public void SetAllPixels(int z)
    {
        if(Mode != ImageMode.Depth)
        {
            throw new InvalidOperationException("RawLightedImage SetAllPixels called when not in Depth mode");
        }

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
                PixelValues[x, y] = z;
        }
    }

    // Depth mode only
    public void SetAllLighting(Vector3 lighting)
    {
        if (Mode != ImageMode.Depth)
        {
            throw new InvalidOperationException("RawLightedImage SetAllPixels called when not in Depth mode");
        }

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
                Lighting[x, y] = lighting;
        }
    }

    // Depth mode only
    public void SetPixel(int x, int y, int z, Vector3 light)
    {
        if (Mode != ImageMode.Depth)
        {
            throw new InvalidOperationException("RawLightedImage SetAllPixels called when not in Depth mode");
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
        {
            Debug.Assert(false);
            return;
        }

        PixelValues[x, y] = z;
        Lighting[x, y] = light;
    }

    // Color mode only
    public void SetPixel(int x, int y, Color color)
    {
        if (Mode != ImageMode.Color)
        {
            throw new InvalidOperationException("RawLightedImage SetAllPixels called when not in Color mode");
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            Debug.Assert(false);
            return;
        }

        ColorValues[x, y] = color;
    }   

    // Depth mode only
    // see if there is a way to copy the arrays more efficiently
    public void SetBlock(int[,] pixels, Vector3[,] lighting, int fromWidth, int toWidth, int height, int depth)
    {
        if (Mode != ImageMode.Depth)
        {
            throw new InvalidOperationException("RawLightedImage SetAllPixels called when not in Depth mode");
        }

        if (height != Height)
            throw new ArgumentException("RawLightedImage SetBlock Height does not match");

        if(depth != Depth) 
            throw new ArgumentException("RawLightedImage SetBlock Depth does not match");

        if(fromWidth < 0 || fromWidth > Width)
            throw new ArgumentException("RawLightedImage SetBlock fromWidth does not match");

        if(toWidth < 0 || toWidth > Width)
            throw new ArgumentException("RawLightedImage SetBlock toWidth does not match");

        int subWidth = toWidth - fromWidth + 1; 

        int startValues = fromWidth * Height;
        int numberOfValues = subWidth * Height;

        Array.Copy(pixels, 0, PixelValues, startValues, numberOfValues);
        Array.Copy(lighting, 0, Lighting, startValues, numberOfValues);
    }

    // Color mode only
    public void SetBlock(Color[,] colors, int fromWidth, int toWidth, int height)
    {
        if (Mode != ImageMode.Color)
        {
            throw new InvalidOperationException("RawLightedImage SetAllPixels called when not in Color mode");
        }

        if (height != Height)
            throw new ArgumentException("RawLightedImage SetBlock Height does not match");

        if (fromWidth < 0 || fromWidth > Width)
            throw new ArgumentException("RawLightedImage SetBlock fromWidth does not match");

        if (toWidth < 0 || toWidth > Width)
            throw new ArgumentException("RawLightedImage SetBlock toWidth does not match");

        int subWidth = toWidth - fromWidth + 1;

        int startValues = fromWidth * Height;
        int numberOfValues = subWidth * Height;

        Array.Copy(colors, 0, ColorValues, startValues, numberOfValues);
    }

    public Bitmap GetBitmap(IPalette palette, DisplayInfo displayInfo, float ambientPower)
    {
        Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

        if (Mode == ImageMode.Depth)
        {
            return GetBitmapDepth(bmp, palette, displayInfo, ambientPower);
        }
        else
        {
            return GetBitmapColor(bmp, displayInfo);
        }
    }

    // Depth mode only
    private Bitmap GetBitmapDepth(Bitmap bmp, IPalette palette, DisplayInfo displayInfo, float ambientPower)
    {
        
        if (palette.NumberOfColors != Depth)
        {
            Debug.Assert(false);
            return bmp;
        }

        switch (displayInfo.Mode)
        {
            case DisplayMode.Contrast:
                return GetContrastBitmapDepth(palette, bmp, displayInfo, ambientPower);
            case DisplayMode.Hsl when displayInfo.Hue:
            {
                if (displayInfo.Saturation)
                {
                    if (displayInfo.Lightness)
                        return GetHslBitmapDepth(palette, bmp, displayInfo, ambientPower);
                    else
                        return GetHueSatBitmapDepth(palette, bmp, displayInfo, ambientPower);
                }
                else if (displayInfo.Lightness)
                    return GetHueLightnessBitmapDepth(palette, bmp, displayInfo, ambientPower);
                else
                    return GetHueBitmapDepth(palette, bmp, displayInfo, ambientPower);
            }
            case DisplayMode.Hsl when displayInfo.Saturation:
            {
                if (displayInfo.Lightness)
                    return GetSaturationLightnessBitmapDepth(palette, bmp, displayInfo, ambientPower);
                else
                    return GetSaturationBitmapDepth(palette, bmp, displayInfo, ambientPower);
            }
            case DisplayMode.Hsl when displayInfo.Lightness:
                return GetLightnessBitmapDepth(palette, bmp, displayInfo, ambientPower);
            default:
                return GetBitmapDepth(palette, bmp, ambientPower);
        }
    }

    // Color mode only
    protected Bitmap GetBitmapColor(Bitmap bmp, DisplayInfo displayInfo)
    {
        switch (displayInfo.Mode)
        {
            case DisplayMode.Contrast:
                return GetContrastBitmapColor(bmp, displayInfo);
            case DisplayMode.Hsl when displayInfo.Hue:
                {
                    if (displayInfo.Saturation)
                    {
                        if (displayInfo.Lightness)
                            return GetHslBitmapColor(bmp, displayInfo);
                        else
                            return GetHueSatBitmapColor(bmp, displayInfo);
                    }
                    else if (displayInfo.Lightness)
                        return GetHueLightnessBitmapColor(bmp, displayInfo);
                    else
                        return GetHueBitmapColor(bmp, displayInfo);
                }
            case DisplayMode.Hsl when displayInfo.Saturation:
                {
                    if (displayInfo.Lightness)
                        return GetSaturationLightnessBitmapColor(bmp, displayInfo);
                    else
                        return GetSaturationBitmapColor(bmp, displayInfo);
                }
            case DisplayMode.Hsl when displayInfo.Lightness:
                return GetLightnessBitmapColor(bmp, displayInfo);
            default:
                return GetBitmapColor(bmp);
        }
    }

    private Bitmap GetContrastBitmapDepth(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
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

    private Bitmap GetContrastBitmapColor(Bitmap bmp, DisplayInfo displayInfo)
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
                    Color pixelColor = ColorValues[x, y];
                    byte* pixel = (byte*)intptr;

                    b = ((double)pixelColor.B - (double)displayInfo.MinRgb[0]) * stretch[0] + 0.49999;
                    g = ((double)pixelColor.G - (double)displayInfo.MinRgb[1]) * stretch[1] + 0.49999;
                    r = ((double)pixelColor.R - (double)displayInfo.MinRgb[2]) * stretch[2] + 0.49999;

                    Color finalColor = Color.FromArgb(pixelColor.A, (byte)Math.Min(Math.Max(0, (int)(r)), Range),
                        (byte)Math.Min(Math.Max(0, (int)(g)), Range), (byte)Math.Min(Math.Max(0, (int)(b)), Range));

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

    private DbPair GetMinMaxHueDepth(IPalette palette, Bitmap bmp)
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

    private DbPair GetMinMaxHueColor(Bitmap bmp)
    {
        DbPair sp = new DbPair();
        sp.MaxDb = 0.0;
        sp.MinDb = 360.0;

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                Color pixelColor = ColorValues[x, y];
                HSL hsl = HslConvertor.ToHsl(pixelColor);
                sp.MinDb = Math.Min(sp.MinDb, hsl.H);
                sp.MaxDb = Math.Max(sp.MaxDb, hsl.H);
            }
        }

        return sp;
    }

    private DbPair GetMinMaxSaturationDepth(IPalette palette, Bitmap bmp)
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

    private DbPair GetMinMaxSaturationColor(Bitmap bmp)
    {
        DbPair sp = new DbPair();
        sp.MaxDb = 0.0;
        sp.MinDb = 1.0;

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                Color pixelColor = ColorValues[x, y];
                HSL hsl = HslConvertor.ToHsl(pixelColor);
                sp.MinDb = Math.Min(sp.MinDb, hsl.S);
                sp.MaxDb = Math.Max(sp.MaxDb, hsl.S);
            }
        }

        return sp;
    }

    private DbPair GetMinMaxLightnessDepth(IPalette palette, Bitmap bmp)
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

    private DbPair GetMinMaxLightnessColor(Bitmap bmp)
    {
        DbPair sp = new DbPair();
        sp.MaxDb = 0.0;
        sp.MinDb = 1.0;

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                Color pixelColor = ColorValues[x, y];
                HSL hsl = HslConvertor.ToHsl(pixelColor);
                sp.MinDb = Math.Min(sp.MinDb, hsl.L);
                sp.MaxDb = Math.Max(sp.MaxDb, hsl.L);
            }
        }

        return sp;
    }

    private Bitmap GetHslBitmapDepth(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
    {
        // find min and max sat
        DbPair huePr = GetMinMaxHueDepth(palette, bmp);
        DbPair satPr = GetMinMaxSaturationDepth(palette, bmp);
        DbPair lightPr = GetMinMaxLightnessDepth(palette, bmp);

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

    private Bitmap GetHslBitmapColor(Bitmap bmp, DisplayInfo displayInfo)
    {
        // find min and max sat
        DbPair huePr = GetMinMaxHueColor(bmp);
        DbPair satPr = GetMinMaxSaturationColor(bmp);
        DbPair lightPr = GetMinMaxLightnessColor(bmp);

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
                    Color pixelColor = ColorValues[x, y];
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newSat = displayInfo.MinSaturation + (hsl.S - satPr.MinDb) * satScaleFactor;
                    hsl.S = newSat;

                    double newLight = displayInfo.MinLightness + (hsl.L - lightPr.MinDb) * lightScaleFactor;
                    hsl.L = newLight;

                    double newHue = displayInfo.MinHue + (hsl.H - huePr.MinDb) * hueScaleFactor;
                    hsl.H = newHue;

                    Color finalColor = HslConvertor.ToRgb(hsl, pixelColor.A);

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

    private Bitmap GetHueSatBitmapDepth(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
    {
        // find min and max sat
        DbPair huePr = GetMinMaxHueDepth(palette, bmp);
        DbPair satPr = GetMinMaxSaturationDepth(palette, bmp);

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

    private Bitmap GetHueSatBitmapColor(Bitmap bmp, DisplayInfo displayInfo)
    {
        // find min and max sat
        DbPair huePr = GetMinMaxHueColor(bmp);
        DbPair satPr = GetMinMaxSaturationColor(bmp);

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
                    Color pixelColor = ColorValues[x, y];
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newSat = displayInfo.MinSaturation + (hsl.S - satPr.MinDb) * satScaleFactor;
                    hsl.S = newSat;

                    double newHue = displayInfo.MinHue + (hsl.H - huePr.MinDb) * hueScaleFactor;
                    hsl.H = newHue;

                    Color finalColor = HslConvertor.ToRgb(hsl, pixelColor.A);

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

    private Bitmap GetHueLightnessBitmapDepth(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
    {
        // find min and max sat
        DbPair huePr = GetMinMaxHueDepth(palette, bmp);
        DbPair lightPr = GetMinMaxLightnessDepth(palette, bmp);

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

    private Bitmap GetHueLightnessBitmapColor(Bitmap bmp, DisplayInfo displayInfo)
    {
        // find min and max sat
        DbPair huePr = GetMinMaxHueColor(bmp);
        DbPair lightPr = GetMinMaxLightnessColor(bmp);

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
                    Color pixelColor = ColorValues[x, y];
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newLight = displayInfo.MinLightness + (hsl.L - lightPr.MinDb) * lightScaleFactor;
                    hsl.L = newLight;

                    double newHue = displayInfo.MinHue + (hsl.H - huePr.MinDb) * hueScaleFactor;
                    hsl.H = newHue;

                    Color finalColor = HslConvertor.ToRgb(hsl, pixelColor.A);

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

    private Bitmap GetHueBitmapDepth(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
    {
        // find min and max sat
        DbPair sp = GetMinMaxHueDepth(palette, bmp);

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

    private Bitmap GetHueBitmapColor(Bitmap bmp, DisplayInfo displayInfo)
    {
        // find min and max sat
        DbPair sp = GetMinMaxHueColor(bmp);

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
                    Color pixelColor = ColorValues[x, y];
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newHue = displayInfo.MinSaturation + (hsl.H - sp.MinDb) * scaleFactor;
                    hsl.H = newHue;

                    Color finalColor = HslConvertor.ToRgb(hsl, pixelColor.A);

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

    private Bitmap GetSaturationLightnessBitmapDepth(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
    {
        // find min and max sat
        DbPair satPr = GetMinMaxSaturationDepth(palette, bmp);
        DbPair lightPr = GetMinMaxLightnessDepth(palette, bmp);

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

    private Bitmap GetSaturationLightnessBitmapColor(Bitmap bmp, DisplayInfo displayInfo)
    {
        // find min and max sat
        DbPair satPr = GetMinMaxSaturationColor(bmp);
        DbPair lightPr = GetMinMaxLightnessColor(bmp);

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
                    Color pixelColor = ColorValues[x, y];
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newSat = displayInfo.MinSaturation + (hsl.S - satPr.MinDb) * satScaleFactor;
                    hsl.S = newSat;

                    double newLight = displayInfo.MinLightness + (hsl.L - lightPr.MinDb) * lightScaleFactor;
                    hsl.L = newLight;

                    Color finalColor = HslConvertor.ToRgb(hsl, pixelColor.A);

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

    private Bitmap GetSaturationBitmapDepth(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
    {
        // find min and max sat
        DbPair sp = GetMinMaxSaturationDepth(palette, bmp);

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

    private Bitmap GetSaturationBitmapColor(Bitmap bmp, DisplayInfo displayInfo)
    {
        // find min and max sat
        DbPair sp = GetMinMaxSaturationColor(bmp);

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
                    Color pixelColor = ColorValues[x, y];
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newSat = displayInfo.MinSaturation + (hsl.S - sp.MinDb) * scaleFactor;
                    hsl.S = newSat;

                    Color finalColor = HslConvertor.ToRgb(hsl, pixelColor.A);

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

    private Bitmap GetLightnessBitmapDepth(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, float ambientPower)
    {
        // find min and max sat
        DbPair sp = GetMinMaxLightnessDepth(palette, bmp);

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

    private Bitmap GetLightnessBitmapColor(Bitmap bmp, DisplayInfo displayInfo)
    {
        // find min and max sat
        DbPair sp = GetMinMaxLightnessColor(bmp);

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
                    Color pixelColor = ColorValues[x, y];
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newLt = displayInfo.MinLightness + (hsl.L - sp.MinDb) * scaleFactor;
                    hsl.L = newLt;

                    Color finalColor = HslConvertor.ToRgb(hsl, pixelColor.A);

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

    private Bitmap GetBitmapDepth(IPalette palette, Bitmap bmp, float ambientPower)
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

    private Bitmap GetBitmapColor(Bitmap bmp)
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
                    Color pixelColor = ColorValues[x, y];

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
