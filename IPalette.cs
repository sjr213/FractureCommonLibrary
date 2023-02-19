using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;

namespace FractureCommonLib
{
    public interface IPalette : ICloneable
    {
        int NumberOfColors
        {
            get;
            set;
        }

        Color GetColor(int index);

        string PaletteName
        {
            get;
            set;
        }

        // Color point methods
        /////////////////////////////////

        // returns null if there isn't a point
        ColorPoint? GetColorPoint(int index);

        // replaces an existing point
        void AddColorPoint(ColorPoint colorPoint);

        // Throws if there isn't a point here
        void RemoveColorPoint(int index);

        // Returns true if there is already a color point
        // representing this index position
        bool IsPointAtIndex(int index);

        // throws if the point doesn't exist or there 
        // already is a point at the newIndex
        void MoveColorPoint(int oldIndex, int newIndex);

        int GetNumberOfColorPoints();

        SortedList<int, ColorPoint> GetCopyOfColorPointList();
    }
}
