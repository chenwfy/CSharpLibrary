using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace CSharpLib.ImageBase.Quantize
{
    internal unsafe class PaletteQuantizer
        : Quantizer
    {
        private Dictionary<uint, byte> _colorMap;

        private Color[] _colors;

        public PaletteQuantizer(List<Color> palette)
            : base(true)
        {
            _colorMap = new Dictionary<uint, byte>();
            _colors = new Color[palette.Count];
            palette.CopyTo(_colors);
        }

        protected override byte QuantizePixel(ColorBgra* pixel)
        {
            byte colorIndex = 0;
            uint colorHash = pixel->Bgra;

            if (_colorMap.ContainsKey(colorHash))
            {
                colorIndex = _colorMap[colorHash];
            }
            else
            {
                if (0 == pixel->A)
                {
                    for (int index = 0; index < _colors.Length; index++)
                    {
                        if (0 == _colors[index].A)
                        {
                            colorIndex = (byte)index;
                            break;
                        }
                    }
                }
                else
                {
                    int leastDistance = int.MaxValue;
                    int red = pixel->R;
                    int green = pixel->G;
                    int blue = pixel->B;

                    for (int index = 0; index < _colors.Length; index++)
                    {
                        Color paletteColor = _colors[index];

                        int redDistance = paletteColor.R - red;
                        int greenDistance = paletteColor.G - green;
                        int blueDistance = paletteColor.B - blue;

                        int distance = (redDistance * redDistance) + (greenDistance * greenDistance) +
                            (blueDistance * blueDistance);

                        if (distance < leastDistance)
                        {
                            colorIndex = (byte)index;
                            leastDistance = distance;

                            if (0 == distance)
                            {
                                break;
                            }
                        }
                    }
                }

                _colorMap.Add(colorHash, colorIndex);
            }

            return colorIndex;
        }

        protected override ColorPalette GetPalette(ColorPalette palette)
        {
            for (int index = 0; index < _colors.Length; index++)
            {
                palette.Entries[index] = _colors[index];
            }
            return palette;
        }
    }
}
