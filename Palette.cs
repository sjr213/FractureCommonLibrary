using System.Drawing;
using Newtonsoft.Json;

namespace FractureCommonLib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ColorPointPair
    {
        [JsonProperty]
        public int LowIndex;

        [JsonProperty]
        public ColorPoint? LowerPoint;

        [JsonProperty]
        public int HighIndex;

        [JsonProperty]
        public ColorPoint? HigherPoint;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Palette : IPalette
    {
        /////////////////////////////////////////////////////////////////////////
        // data members
        /////////////////////////////////////////////////////////////////////////

        // The list key is color index as int, and value IColorPoint
        [JsonProperty]
        private SortedList<int, ColorPoint> _colorPointList = new(8);

        [JsonProperty]
        private int _numberOfColors;

        [JsonProperty]
        private string _paletteName = "Default";

        /////////////////////////////////////////////////////////////////////////
        // Methods
        /////////////////////////////////////////////////////////////////////////

        // Constructors
        [JsonConstructor]
        public Palette(int numberOfColors)
        {
            _numberOfColors = numberOfColors;
        }

        public Palette(int numberOfColors, SortedList<int, ColorPoint> colorPtList, string name)
        {
            _numberOfColors = numberOfColors;
            _colorPointList = colorPtList;
            _paletteName = name;
        }

        // IPalette interface
        public int NumberOfColors
        {
            get => _numberOfColors;
            set
            {
                _numberOfColors = value;
                NumberOfColorsChangedRedistributePoints();
            }
        }

        public string PaletteName
        {
            get => _paletteName;
            set => _paletteName = value;
        }

        // If the index is already in use try to bump it up one
        private void TryToAddNewPoint(int index, ColorPoint pt, SortedList<int, ColorPoint> ptList)
        {
            if (!ptList.TryGetValue(index, out _))
                ptList.Add(index, pt);
            else if (index < _numberOfColors - 1)
            {
                if (index > 0)
                    --index;
                else
                    ++index;

                if (!ptList.TryGetValue(index, out _))
                {
                    pt.SetPositionByIndex(index, _numberOfColors);
                    ptList.Add(index, pt);
                }
                    
            }
        }

        private void NumberOfColorsChangedRedistributePoints()
        {
            SortedList<int, ColorPoint> newPointList= new SortedList<int, ColorPoint>(_colorPointList.Count);

            foreach (KeyValuePair<int, ColorPoint> entry in _colorPointList)
            {
                ColorPoint colorPt = entry.Value;
                int newIndex = colorPt.GetColorIndex(NumberOfColors);

                TryToAddNewPoint(newIndex, colorPt, newPointList);
            }

            _colorPointList = newPointList;
        }


        public Color GetColor(int index)
        {
            if (_colorPointList.ContainsKey(index))
            {
                ColorPoint colorPt = _colorPointList[index];
                return colorPt.PointColor;
            }

            ColorPointPair surroundingPts = GetSurroundingColorPoints(index);

            if (surroundingPts.LowerPoint == null && surroundingPts.HigherPoint == null)
                return Color.White;
            else if (surroundingPts.LowerPoint == null)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                return surroundingPts.HigherPoint.PointColor;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            else if (surroundingPts.HigherPoint == null)
                return surroundingPts.LowerPoint.PointColor;
            else
            {
                return GetColorWeightedBetweenPoint(index, surroundingPts);
            }
        }

        public int GetNumberOfColorPoints()
        {
            return _colorPointList.Count;
        }

        // returns null if there isn't a point
        // Needs to be an exact match to the index
        // can return null if a point isn't found at that index
        public ColorPoint? GetColorPoint(int index)
        {
            if (_colorPointList.ContainsKey(index))
            {
                ColorPoint colorPt = _colorPointList[index];
                return colorPt;
            }

            return null;
        }


        // If a point exists at this index it is replaced
        public void AddColorPoint(ColorPoint colorPoint)
        {
            ColorPoint copiedPt = colorPoint.Clone();

            int newIndex = copiedPt.GetColorIndex( NumberOfColors );

            TryToAddNewPoint(newIndex, copiedPt, _colorPointList);
        }


        // Throws if there isn't a point here
        public void RemoveColorPoint(int index)
        {
            if (_colorPointList.ContainsKey(index))
            {
                _colorPointList.Remove(index);
            }
            else
            {
                string msg = "Cannot remove color point that doesn't exist at index: ";
                msg += index.ToString();
                throw new PaletteException(msg);
            }    
        }


        // Returns true if there is already a color point
        // representing this index position
        public bool IsPointAtIndex(int index)
        {
            return _colorPointList.ContainsKey(index);
        }

        // throws if the point doesn't exist or there 
        // already is a point at the newIndex
        public void MoveColorPoint(int oldIndex, int newIndex)
        {
            if(oldIndex < 0 || oldIndex >= NumberOfColors)
            {
                string msg = "Palette.MoveColorPoint() oldIndex is out of range!";
                throw new PaletteException(msg);
            }

            if(newIndex < 0 || newIndex >= NumberOfColors)
            {
                string msg = "Palette.MoveColorPoint() newIndex is out of range!";
                throw new PaletteException(msg);
            }

            if( IsPointAtIndex(newIndex) )
            {
                string msg = "Cannot move color point to an occupied index: ";
                msg += newIndex.ToString();
                throw new PaletteException(msg);
            }

            if (!IsPointAtIndex(oldIndex))
            {
                string msg = "Cannot move color point that doesn't exist at index: ";
                msg += oldIndex.ToString();
                throw new PaletteException(msg);
            }

            ColorPoint originalPt = _colorPointList[oldIndex];
            _colorPointList.Remove(oldIndex);

            // reset the position of the color point and insert
            ColorPoint copiedPt = originalPt.Clone();
            copiedPt.SetPositionByIndex(newIndex, NumberOfColors);

            TryToAddNewPoint(newIndex, copiedPt, _colorPointList);
        }


        protected ColorPointPair GetSurroundingColorPoints(int index)
        {
            ColorPointPair ptPair = new ColorPointPair();

            foreach (KeyValuePair<int, ColorPoint> entry in _colorPointList)
            {
                if (entry.Key < index)
                {
                    ptPair.LowIndex = entry.Key;
                    ptPair.LowerPoint = entry.Value;
                }
                else
                {
                    ptPair.HighIndex = entry.Key;
                    ptPair.HigherPoint = entry.Value;
                    return ptPair;
                }
            }
            return ptPair;
        }


        protected Color GetColorWeightedBetweenPoint(int index, ColorPointPair ptPair)
        {
            if (ptPair.HigherPoint == null || ptPair.LowerPoint == null)
                throw new Exception("Both color points are null in Palette.GetColorWeightedBetweenPoint()");

            int indexPt1 = ptPair.LowIndex;
            int indexPt2 = ptPair.HighIndex;

            int delta = indexPt2 - indexPt1;
            int distanceFromLow = index - indexPt1;

            if(delta == 0)
                throw new Exception("points have the same index in Palette.GetColorWeightedBetweenPoint()");

            double eRatio2 = ((double)distanceFromLow) / delta;
            double eRatio1 = 1.0 - eRatio2;

            Color color1 = ptPair.LowerPoint.PointColor;
            Color color2 = ptPair.HigherPoint.PointColor;

            
            Byte a = (Byte)(eRatio1 * color1.A + eRatio2 * color2.A );
            Byte r = (Byte)(eRatio1 * color1.R + eRatio2 * color2.R);
            Byte g = (Byte)(eRatio1 * color1.G + eRatio2 * color2.G);
            Byte b = (Byte)(eRatio1 * color1.B + eRatio2 * color2.B);
            Color newColor = Color.FromArgb(a, r, g, b);

            return newColor;
        }

        public SortedList<int, ColorPoint> GetCopyOfColorPointList()
        {
            SortedList<int, ColorPoint> newPointList= new SortedList<int, ColorPoint>(_colorPointList.Count);

            foreach (KeyValuePair<int, ColorPoint> entry in _colorPointList)
            {
                ColorPoint colorPt = entry.Value;
                ColorPoint copiedPt = colorPt.Clone();
                TryToAddNewPoint(entry.Key, copiedPt, newPointList);
            }

            return newPointList;
        }


        public void SpreadPinsEvenly()
        {
            int nPins = _colorPointList.Count;
            if (nPins == 0)
                return;

            int nColors = NumberOfColors;
            double nSpacing = ((double)nColors) / (nPins - 1);

            SortedList<int, ColorPoint> colorPointListCopy = new SortedList<int, ColorPoint>(0);

            int index = 0;
            foreach (KeyValuePair<int, ColorPoint> entry in _colorPointList)
            {
                ColorPoint copiedPt = entry.Value.Clone();
                if (index == 0)
                {
                    int newIndex = 0;
                    copiedPt.SetPositionByIndex(newIndex, nColors);

                    TryToAddNewPoint(newIndex, copiedPt, colorPointListCopy);
                }
                else if (index == nPins - 1)
                {
                    int newIndex = nColors - 1;
                    copiedPt.SetPositionByIndex(newIndex, nColors);
                    TryToAddNewPoint(newIndex, copiedPt, colorPointListCopy);
                }
                else
                {
                    int newIndex = (int)(index * nSpacing + 0.5) -1;
                    copiedPt.SetPositionByIndex(newIndex, nColors);
                    TryToAddNewPoint(newIndex, copiedPt, colorPointListCopy);
                }
                ++index;
            }

            _colorPointList = colorPointListCopy;
        }


        public Object Clone()
        {
            var copiedPalette = new Palette(_numberOfColors)
            {
                PaletteName = PaletteName
            };

            // SortedList<int, IColorPoint> _ColorPointList
            foreach( KeyValuePair<int, ColorPoint> pt in _colorPointList)
            {
                var copiedPt = pt.Value.Clone();
                copiedPalette._colorPointList.Add(pt.Key, copiedPt);
            }

            return copiedPalette;
        }

    }
}
