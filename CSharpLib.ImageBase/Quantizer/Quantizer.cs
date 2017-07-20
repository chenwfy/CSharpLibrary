using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CSharpLib.ImageBase.Quantize
{
    internal unsafe abstract class Quantizer
    {
        private bool singlePass;

        protected int ditherLevel;
        public int DitherLevel
        {
            get
            {
                return this.ditherLevel;
            }

            set
            {
                this.ditherLevel = value;
            }
        }

        public Quantizer(bool singlePass)
        {
            this.singlePass = singlePass;
        }

        public Bitmap Quantize(Image source)
        {
            int height = source.Height;
            int width = source.Width;

            Rectangle bounds = new Rectangle(0, 0, width, height);

            Bitmap copy;

            if (source is Bitmap && source.PixelFormat == PixelFormat.Format32bppArgb)
            {
                copy = (Bitmap)source;
            }
            else
            {
                copy = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(copy))
                {
                    g.PageUnit = GraphicsUnit.Pixel;
                    g.DrawImage(source, 0, 0, bounds.Width, bounds.Height);
                }
            }

            Bitmap output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            BitmapData sourceData = null;

            try
            {
                sourceData = copy.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                if (!singlePass)
                {
                    FirstPass(sourceData, width, height);
                }

                output.Palette = this.GetPalette(output.Palette);

                SecondPass(sourceData, output, width, height, bounds);
            }

            finally
            {
                copy.UnlockBits(sourceData);
            }

            if (copy != source)
            {
                copy.Dispose();
                copy = null;
            }

            return output;
        }

        protected virtual void FirstPass(BitmapData sourceData, int width, int height)
        {
            byte* pSourceRow = (byte*)sourceData.Scan0.ToPointer();
            Int32* pSourcePixel;

            for (int row = 0; row < height; row++)
            {
                pSourcePixel = (Int32*)pSourceRow;

                for (int col = 0; col < width; col++, pSourcePixel++)
                {
                    InitialQuantizePixel((ColorBgra*)pSourcePixel);
                }

                pSourceRow += sourceData.Stride;
            }
        }

        protected virtual void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds)
        {
            BitmapData outputData = null;
            Color[] pallete = output.Palette.Entries;
            int weight = ditherLevel;

            try
            {
                outputData = output.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

                byte* pSourceRow = (byte*)sourceData.Scan0.ToPointer();
                Int32* pSourcePixel = (Int32*)pSourceRow;

                byte* pDestinationRow = (byte*)outputData.Scan0.ToPointer();
                byte* pDestinationPixel = pDestinationRow;

                int[] errorThisRowR = new int[width + 1];
                int[] errorThisRowG = new int[width + 1];
                int[] errorThisRowB = new int[width + 1];

                for (int row = 0; row < height; row++)
                {
                    int[] errorNextRowR = new int[width + 1];
                    int[] errorNextRowG = new int[width + 1];
                    int[] errorNextRowB = new int[width + 1];

                    int ptrInc;

                    if ((row & 1) == 0)
                    {
                        pSourcePixel = (Int32*)pSourceRow;
                        pDestinationPixel = pDestinationRow;
                        ptrInc = +1;
                    }
                    else
                    {
                        pSourcePixel = (Int32*)pSourceRow + width - 1;
                        pDestinationPixel = pDestinationRow + width - 1;
                        ptrInc = -1;
                    }

                    for (int col = 0; col < width; ++col)
                    {
                        ColorBgra srcPixel = *(ColorBgra*)pSourcePixel;
                        ColorBgra target = new ColorBgra();

                        target.B = Utility.ClampToByte(srcPixel.B - ((errorThisRowB[col] * weight) / 8));
                        target.G = Utility.ClampToByte(srcPixel.G - ((errorThisRowG[col] * weight) / 8));
                        target.R = Utility.ClampToByte(srcPixel.R - ((errorThisRowR[col] * weight) / 8));
                        target.A = srcPixel.A;

                        byte pixelValue = QuantizePixel(&target);
                        *pDestinationPixel = pixelValue;

                        ColorBgra actual = ColorBgra.FromColor(pallete[pixelValue]);

                        int errorR = actual.R - target.R;
                        int errorG = actual.G - target.G;
                        int errorB = actual.B - target.B;


                        const int a = 7;
                        const int b = 5;
                        const int c = 3;

                        int errorRa = (errorR * a) / 16;
                        int errorRb = (errorR * b) / 16;
                        int errorRc = (errorR * c) / 16;
                        int errorRd = errorR - errorRa - errorRb - errorRc;

                        int errorGa = (errorG * a) / 16;
                        int errorGb = (errorG * b) / 16;
                        int errorGc = (errorG * c) / 16;
                        int errorGd = errorG - errorGa - errorGb - errorGc;

                        int errorBa = (errorB * a) / 16;
                        int errorBb = (errorB * b) / 16;
                        int errorBc = (errorB * c) / 16;
                        int errorBd = errorB - errorBa - errorBb - errorBc;

                        errorThisRowR[col + 1] += errorRa;
                        errorThisRowG[col + 1] += errorGa;
                        errorThisRowB[col + 1] += errorBa;

                        errorNextRowR[width - col] += errorRb;
                        errorNextRowG[width - col] += errorGb;
                        errorNextRowB[width - col] += errorBb;

                        if (col != 0)
                        {
                            errorNextRowR[width - (col - 1)] += errorRc;
                            errorNextRowG[width - (col - 1)] += errorGc;
                            errorNextRowB[width - (col - 1)] += errorBc;
                        }

                        errorNextRowR[width - (col + 1)] += errorRd;
                        errorNextRowG[width - (col + 1)] += errorGd;
                        errorNextRowB[width - (col + 1)] += errorBd;

                        unchecked
                        {
                            pSourcePixel += ptrInc;
                            pDestinationPixel += ptrInc;
                        }
                    }

                    pSourceRow += sourceData.Stride;

                    pDestinationRow += outputData.Stride;

                    errorThisRowB = errorNextRowB;
                    errorThisRowG = errorNextRowG;
                    errorThisRowR = errorNextRowR;
                }
            }

            finally
            {
                output.UnlockBits(outputData);
            }
        }

        protected virtual void InitialQuantizePixel(ColorBgra* pixel)
        {
        }

        protected abstract byte QuantizePixel(ColorBgra* pixel);

        protected abstract ColorPalette GetPalette(ColorPalette original);
    }
}
