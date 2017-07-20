using System;
using System.Collections;
using System.Drawing;

namespace CSharpLib.ImageBase.Quantize
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PaletteTable
    {
        /// <summary>
        /// 
        /// </summary>
        private Color[] palette;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Color this[int index]
        {
            get
            {
                return this.palette[index];
            }

            set
            {
                this.palette[index] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int GetDistanceSquared(Color a, Color b)
        {
            int dsq = 0;
            int v;

            v = a.B - b.B;
            dsq += v * v;
            v = a.G - b.G;
            dsq += v * v;
            v = a.R - b.R;
            dsq += v * v;

            return dsq;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        public int FindClosestPaletteIndex(Color pixel)
        {
            int dsqBest = int.MaxValue;
            int ret = 0;

            for (int i = 0; i < this.palette.Length; ++i)
            {
                int dsq = GetDistanceSquared(this.palette[i], pixel);

                if (dsq < dsqBest)
                {
                    dsqBest = dsq;
                    ret = i;
                }
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="palette"></param>
        public PaletteTable(Color[] palette)
        {
            this.palette = (Color[])palette.Clone();
        }
    }
}