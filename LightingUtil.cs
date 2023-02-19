namespace FractureCommonLib;

using System.Drawing;
using System.Numerics;

public static class LightingUtil
{
    public static Color CalculateLight(int depth, IPalette palette, Vector3 lite, float ambientPower)
    {
        var rgb = palette.GetColor(depth);
        return CalculateLight(rgb, lite, ambientPower);
    }

    public static Color CalculateLight(Color color, Vector3 lite, float ambientPower)
    {
        float r = Math.Min(Math.Max((ambientPower * color.R / 255.0f) + lite.X, 0.0f), 1.0f);
        float g = Math.Min(Math.Max((ambientPower * color.G / 255.0f) + lite.Y, 0.0f), 1.0f);
        float b = Math.Min(Math.Max((ambientPower * color.B / 255.0f) + lite.Z, 0.0f), 1.0f);

        return Color.FromArgb(color.A, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }
}
