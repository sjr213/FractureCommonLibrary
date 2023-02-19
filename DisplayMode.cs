using System.ComponentModel;

namespace FractureCommonLib
{
    [Serializable]
    public enum DisplayMode
    {
        [Description("None")]
        Off = 0,
        [Description("Contrast")]
        Contrast,
        [Description("HSL")]
        Hsl
    }
}
