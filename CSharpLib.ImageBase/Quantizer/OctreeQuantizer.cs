using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace CSharpLib.ImageBase.Quantize
{
    /// <summary>
    /// 
    /// </summary>
    internal unsafe class OctreeQuantizer
        : Quantizer
    {
        /// <summary>
        /// 
        /// </summary>
        private Octree _octree;
        /// <summary>
        /// 
        /// </summary>
        private int _maxColors;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxColors"></param>
        /// <param name="maxColorBits"></param>
        public OctreeQuantizer(int maxColors, int maxColorBits)
            : base(false)
        {
            _octree = new Octree(maxColorBits);
            _maxColors = maxColors;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixel"></param>
        protected override void InitialQuantizePixel(ColorBgra* pixel)
        {
            _octree.AddColor(pixel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        protected override byte QuantizePixel(ColorBgra* pixel)
        {
            byte paletteIndex = (byte)_maxColors;
            if (pixel->A > 0)
            {
                paletteIndex = (byte)_octree.GetPaletteIndex(pixel);
            }
            return paletteIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected override ColorPalette GetPalette(ColorPalette original)
        {
            List<Color> palette = _octree.Palletize(_maxColors - 1);
            for (int index = 0; index < palette.Count; index++)
            {
                original.Entries[index] = palette[index];
            }

            for (int i = palette.Count; i < original.Entries.Length; ++i)
            {
                original.Entries[i] = Color.FromArgb(255, 0, 0, 0);
            }

            original.Entries[_maxColors] = Color.FromArgb(0, 0, 0, 0);
            return original;
        }

        /// <summary>
        /// 
        /// </summary>
        private class Octree
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="maxColorBits"></param>
            public Octree(int maxColorBits)
            {
                _maxColorBits = maxColorBits;
                _leafCount = 0;
                _reducibleNodes = new OctreeNode[9];
                _root = new OctreeNode(0, _maxColorBits, this);
                _previousColor = 0;
                _previousNode = null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pixel"></param>
            public void AddColor(ColorBgra* pixel)
            {
                if (_previousColor == pixel->Bgra)
                {
                    if (null == _previousNode)
                    {
                        _previousColor = pixel->Bgra;
                        _root.AddColor(pixel, _maxColorBits, 0, this);
                    }
                    else
                    {
                        _previousNode.Increment(pixel);
                    }
                }
                else
                {
                    _previousColor = pixel->Bgra;
                    _root.AddColor(pixel, _maxColorBits, 0, this);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void Reduce()
            {
                int index;
                for (index = _maxColorBits - 1; (index > 0) && (null == _reducibleNodes[index]); index--)
                {
                }

                OctreeNode node = _reducibleNodes[index];
                _reducibleNodes[index] = node.NextReducible;

                _leafCount -= node.Reduce();

                _previousNode = null;
            }

            /// <summary>
            /// 
            /// </summary>
            public int Leaves
            {
                get
                {
                    return _leafCount;
                }

                set
                {
                    _leafCount = value;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            protected OctreeNode[] ReducibleNodes
            {
                get
                {
                    return _reducibleNodes;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            protected void TrackPrevious(OctreeNode node)
            {
                _previousNode = node;
            }

            /// <summary>
            /// 
            /// </summary>
            private Color[] _palette;
            /// <summary>
            /// 
            /// </summary>
            private PaletteTable paletteTable;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="colorCount"></param>
            /// <returns></returns>
            public List<Color> Palletize(int colorCount)
            {
                while (Leaves > colorCount)
                {
                    Reduce();
                }

                List<Color> palette = new List<Color>(Leaves);
                int paletteIndex = 0;

                _root.ConstructPalette(palette, ref paletteIndex);

                this._palette = palette.ToArray();
                this.paletteTable = null;

                return palette;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pixel"></param>
            /// <returns></returns>
            public int GetPaletteIndex(ColorBgra* pixel)
            {
                int ret = -1;

                ret = _root.GetPaletteIndex(pixel, 0);

                if (ret < 0)
                {
                    if (this.paletteTable == null)
                    {
                        this.paletteTable = new PaletteTable(this._palette);
                    }

                    ret = this.paletteTable.FindClosestPaletteIndex(pixel->ToColor());
                }

                return ret;
            }

            /// <summary>
            /// 
            /// </summary>
            private static int[] mask = new int[8] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
            /// <summary>
            /// 
            /// </summary>
            private OctreeNode _root;
            /// <summary>
            /// 
            /// </summary>
            private int _leafCount;
            /// <summary>
            /// 
            /// </summary>
            private OctreeNode[] _reducibleNodes;
            /// <summary>
            /// 
            /// </summary>
            private int _maxColorBits;
            /// <summary>
            /// 
            /// </summary>
            private OctreeNode _previousNode;
            /// <summary>
            /// 
            /// </summary>
            private uint _previousColor;
            /// <summary>
            /// 
            /// </summary>
            protected class OctreeNode
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="level"></param>
                /// <param name="colorBits"></param>
                /// <param name="octree"></param>
                public OctreeNode(int level, int colorBits, Octree octree)
                {
                    _leaf = (level == colorBits);

                    _red = 0;
                    _green = 0;
                    _blue = 0;
                    _pixelCount = 0;

                    if (_leaf)
                    {
                        octree.Leaves++;
                        _nextReducible = null;
                        _children = null;
                    }
                    else
                    {
                        _nextReducible = octree.ReducibleNodes[level];
                        octree.ReducibleNodes[level] = this;
                        _children = new OctreeNode[8];
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="pixel"></param>
                /// <param name="colorBits"></param>
                /// <param name="level"></param>
                /// <param name="octree"></param>
                public void AddColor(ColorBgra* pixel, int colorBits, int level, Octree octree)
                {
                    if (_leaf)
                    {
                        Increment(pixel);
                        octree.TrackPrevious(this);
                    }
                    else
                    {
                        int shift = 7 - level;
                        int index = ((pixel->R & mask[level]) >> (shift - 2)) |
                                    ((pixel->G & mask[level]) >> (shift - 1)) |
                                    ((pixel->B & mask[level]) >> (shift));

                        OctreeNode child = _children[index];

                        if (null == child)
                        {
                            child = new OctreeNode(level + 1, colorBits, octree);
                            _children[index] = child;
                        }
                        child.AddColor(pixel, colorBits, level + 1, octree);
                    }

                }

                /// <summary>
                /// 
                /// </summary>
                public OctreeNode NextReducible
                {
                    get
                    {
                        return _nextReducible;
                    }

                    set
                    {
                        _nextReducible = value;
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                public OctreeNode[] Children
                {
                    get
                    {
                        return _children;
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                public int Reduce()
                {
                    int children = 0;
                    _red = 0;
                    _green = 0;
                    _blue = 0;

                    for (int index = 0; index < 8; index++)
                    {
                        if (null != _children[index])
                        {
                            _red += _children[index]._red;
                            _green += _children[index]._green;
                            _blue += _children[index]._blue;
                            _pixelCount += _children[index]._pixelCount;
                            ++children;
                            _children[index] = null;
                        }
                    }

                    _leaf = true;

                    return (children - 1);
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="palette"></param>
                /// <param name="paletteIndex"></param>
                public void ConstructPalette(List<Color> palette, ref int paletteIndex)
                {
                    if (_leaf)
                    {
                        _paletteIndex = paletteIndex++;

                        int r = _red / _pixelCount;
                        int g = _green / _pixelCount;
                        int b = _blue / _pixelCount;

                        palette.Add(Color.FromArgb(r, g, b));
                    }
                    else
                    {
                        for (int index = 0; index < 8; index++)
                        {
                            if (null != _children[index])
                            {
                                _children[index].ConstructPalette(palette, ref paletteIndex);
                            }
                        }
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="pixel"></param>
                /// <param name="level"></param>
                /// <returns></returns>
                public int GetPaletteIndex(ColorBgra* pixel, int level)
                {
                    int paletteIndex = _paletteIndex;

                    if (!_leaf)
                    {
                        int shift = 7 - level;
                        int index = ((pixel->R & mask[level]) >> (shift - 2)) |
                                    ((pixel->G & mask[level]) >> (shift - 1)) |
                                    ((pixel->B & mask[level]) >> (shift));

                        if (null != _children[index])
                        {
                            paletteIndex = _children[index].GetPaletteIndex(pixel, level + 1);
                        }
                        else
                        {
                            paletteIndex = -1;
                        }
                    }

                    return paletteIndex;
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="pixel"></param>
                public void Increment(ColorBgra* pixel)
                {
                    ++_pixelCount;
                    _red += pixel->R;
                    _green += pixel->G;
                    _blue += pixel->B;
                }

                /// <summary>
                /// 
                /// </summary>
                private bool _leaf;
                /// <summary>
                /// 
                /// </summary>
                private int _pixelCount;
                /// <summary>
                /// 
                /// </summary>
                private int _red;
                /// <summary>
                /// 
                /// </summary>
                private int _green;
                /// <summary>
                /// 
                /// </summary>
                private int _blue;
                /// <summary>
                /// 
                /// </summary>
                private OctreeNode[] _children;
                /// <summary>
                /// 
                /// </summary>
                private OctreeNode _nextReducible;
                /// <summary>
                /// 
                /// </summary>
                private int _paletteIndex;
            }
        }
    }
}