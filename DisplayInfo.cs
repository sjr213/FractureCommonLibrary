using Newtonsoft.Json;

namespace FractureCommonLib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DisplayInfo: ICloneable
    {
        public static double MAX_HUE = 359.9;
        public static double IdealMaxHue = 300;

        static void Swap<T>(ref T lhs, ref T rhs)
        {
            (lhs, rhs) = (rhs, lhs);
        }

        [JsonProperty]
        public DisplayMode Mode
        { get; set; } = DisplayMode.Off;

        [JsonProperty]
        public bool Saturation
        { get; set; }

        [JsonProperty]
        public bool Hue
        { get; set; }

        [JsonProperty]
        public bool Lightness
        { get; set; }

        [JsonProperty]
        public byte[] MinRgb
        { get; set; } = new byte[] { 0, 0, 0 };

        [JsonProperty]
        public byte[] MaxRgb
        { get; set; } = new byte[] { 255, 255, 255 };

        [JsonProperty]
        public double MinHue
        { get; set; }

        [JsonProperty]
        public double MaxHue
        { get; set; } = IdealMaxHue;

        [JsonProperty]
        public double MinSaturation
        { get; set; }

        [JsonProperty]
        public double MaxSaturation
        { get; set; } = 1.0;

        [JsonProperty]
        public double MinLightness
        { get; set; }

        [JsonProperty]
        public double MaxLightness
        { get; set; } = 1.0;

        // make sure all min are min and max are max
        void Validate()
        {
            for (int index = (int)RgbType.RgbPart.blue; index < (int)RgbType.RgbPart.total; ++index)
            {
                if (MinRgb[index] > MaxRgb[index])
                {
                    (MinRgb[index], MaxRgb[index]) = (MaxRgb[index], MinRgb[index]);
                }
            }

            if (MinHue > MaxHue)
            {
                (MinHue, MaxHue) = (MaxHue, MinHue);
            }

            if (MinHue < 0.0)
                MinHue = 0.0;

            if (MaxHue > MAX_HUE)
                MaxHue = MAX_HUE;

            if (MinSaturation > MaxSaturation)
            {
                (MinSaturation, MaxSaturation) = (MaxSaturation, MinSaturation);
            }

            if (MinSaturation < 0.0)
                MinSaturation = 0.0;

            if (MaxSaturation > 1.0)
                MaxSaturation = 1.0;

            if (MinLightness > MaxLightness)
            {
                (MinLightness, MaxLightness) = (MaxLightness, MinLightness);
            }

            if (MinLightness < 0.0)
                MinLightness = 0.0;

            if (MaxLightness > 1.0)
                MaxLightness = 1.0;
        }

        public void ResetContrast()
        {
            for (int i = 0; i < 3; i++)
            {
                MinRgb[i] = 0;
                MaxRgb[i] = 255;
            }
        }

        public void ResetHsl()
        {
            MinHue = 0.0;
            MaxHue = MAX_HUE;
            MinSaturation = 0.0;
            MaxSaturation = 1.0;
            MinLightness = 0.0;
            MaxLightness = 1.0;
        }

        public object Clone()
        {
            DisplayInfo copy = new()
            {
                Mode = Mode,
                Saturation = Saturation,
                Hue = Hue,
                Lightness = Lightness
            };

            for (int x = 0; x < 3; ++x)
            {
                copy.MinRgb[x] = MinRgb[x];
                copy.MaxRgb[x] = MaxRgb[x];
            }

            copy.MinHue = MinHue;
            copy.MaxHue = MaxHue;
            copy.MinSaturation = MinSaturation;
            copy.MaxSaturation = MaxSaturation;
            copy.MinLightness = MinLightness;
            copy.MaxLightness = MaxLightness;

            return copy;
        }

    }
}
