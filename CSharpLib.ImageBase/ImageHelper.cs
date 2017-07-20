using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using CSharpLib.ImageBase.Quantize;

namespace CSharpLib.ImageBase
{
    /// <summary>
    ///图片处理
    /// </summary>
    public class ImageHelper
    {
        private Bitmap _sourceImage;
        private Bitmap _originalImage;
        /// <summary>
        /// 
        /// </summary>
        public Bitmap SourceImage
        {
            get { return _sourceImage; }
        }
        /// <summary>
        /// 
        /// </summary>
        public Bitmap OriginalImage
        {
            get { return _originalImage; }
        }
        /// <summary>
        /// 用指定文件路径初始化
        /// </summary>
        /// <param name="fileName">要读取的文件路径</param>
        public ImageHelper(string fileName)
        {
            _originalImage = new Bitmap(fileName);
        }

        /// <summary>
        /// 用指定文件流初始化
        /// </summary>
        /// <param name="fileStream">要读取的文件流</param>
        public ImageHelper(Stream fileStream)
        {
            _originalImage = new Bitmap(fileStream);
        }

        /// <summary>
        /// 用指定的图片对象初始化
        /// </summary>
        /// <param name="image"></param>
        public ImageHelper(Bitmap image)
        {
            _originalImage = image;
        }

        /// <summary>
        /// 按给定大小裁剪图片
        /// </summary>
        /// <param name="source">要裁剪的图片对象</param>
        /// <param name="x">开始裁剪的x坐标</param>
        /// <param name="y">开始裁剪的y坐标</param>
        /// <param name="cropSize">要裁剪的图片大小</param>
        /// <returns></returns>
        public static Bitmap Crop(Bitmap source, int x, int y, Size cropSize)
        {
            Bitmap output = new Bitmap(cropSize.Width, cropSize.Height);
            Graphics gh = Graphics.FromImage(output);
            gh.Clear(Color.Empty);
            gh.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gh.CompositingMode = CompositingMode.SourceCopy;
            gh.CompositingQuality = CompositingQuality.HighQuality;
            gh.SmoothingMode = SmoothingMode.HighQuality;
            gh.DrawImage(source, new Rectangle(new Point(0, 0), cropSize), new Rectangle(new Point(x, y), cropSize), GraphicsUnit.Pixel);
            gh.Dispose();
            return output;
        }

        /// <summary>
        /// 按给定大小裁剪图片
        /// </summary>
        /// <param name="x">开始裁剪的x坐标</param>
        /// <param name="y">开始裁剪的y坐标</param>
        /// <param name="cropSize">要裁剪的图片大小</param>
        public void Crop(int x, int y, Size cropSize)
        {
            this._sourceImage = Crop(_sourceImage == null ? _originalImage : _sourceImage, x, y, cropSize);
        }

        /// <summary>
        /// 按给定大小缩放图片
        /// </summary>
        /// <param name="source">要缩放的图片对象</param>
        /// <param name="newSize">要缩放的大小</param>
        /// <returns></returns>
        public static Bitmap Zoom(Bitmap source, Size newSize)
        {
            if (source.Width == newSize.Width && source.Height == newSize.Height)
            {
                return source;
            }
            Bitmap bm = new Bitmap(source, newSize);
            return bm;
        }

        /// <summary>
        /// 按给定大小缩放图片
        /// </summary>
        /// <param name="newSize">要缩放的大小</param>
        public void Zoom(Size newSize)
        {
            _sourceImage = Zoom(_sourceImage == null ? _originalImage : _sourceImage, newSize);
        }

        /// <summary>
        /// 按给定宽度或高度等比缩放图片
        /// </summary>
        /// <param name="source">要缩放的图片对象</param>
        /// <param name="width">要缩放的宽度</param>
        /// <param name="height">要缩放的高度</param>
        /// <returns></returns>
        public static Bitmap Zoom(Bitmap source, int? width, int? height)
        {
            if (height == null && width == null)
            {
                throw new ArgumentException("缩放图片必须指定高度或宽度。");
            }
            if (height == null)
            {
                height = (int)((float)source.Height * (float)width / (float)source.Width);
            }
            if (width == null)
            {
                width = (int)((float)source.Width * (float)height / (float)source.Height);
            }
            return Zoom(source, new Size((int)width, (int)height));
        }

        /// <summary>
        /// 按给定宽度或高度等比缩放图片
        /// </summary>
        /// <param name="width">要缩放的宽度</param>
        /// <param name="height">要缩放的高度</param>
        public void Zoom(int? width, int? height)
        {
            Bitmap objImage = _sourceImage == null ? _originalImage : _sourceImage;
            if (height == null && width == null)
            {
                throw new ArgumentException("缩放图片必须指定高度或宽度。");
            }
            if (height == null)
            {
                height = (int)((float)objImage.Height * (float)width / (float)objImage.Width);
            }
            if (width == null)
            {
                width = (int)((float)objImage.Width * (float)height / (float)objImage.Height);
            }
            _sourceImage = Zoom(objImage, new Size((int)width, (int)height));
        }

        /// <summary>
        /// 剪切并缩放图片对象
        /// </summary>
        /// <param name="source">要缩放并剪切的图片对象</param>
        /// <param name="newSize">缩放并剪切后的大小</param>
        /// <param name="cuteTpye">剪切类型1-9，5：居中剪切，其他的从左至右从上到下代表8个方向</param>
        /// <returns></returns>
        public static Bitmap CropAndZoom(Bitmap source, Size newSize, byte cuteTpye)
        {
            if (source.Width == newSize.Width && source.Height == newSize.Height)
            {
                return source;
            }
            float offsetX = 0, offsetY = 0, width = 0, height = 0;
            if ((float)source.Width / (float)source.Height > (float)newSize.Width / (float)newSize.Height)
            {
                width = newSize.Width * source.Height / newSize.Height;
                height = source.Height;
            }
            else
            {
                width = source.Width;
                height = newSize.Height * source.Width / newSize.Width;
            }
            switch (cuteTpye)
            {
                case 1:
                    offsetX = 0;
                    offsetY = 0;
                    break;
                case 2:
                    offsetX = ((float)source.Width - width) / 2;
                    offsetY = 0;
                    break;
                case 3:
                    offsetX = (float)source.Width - width;
                    offsetY = 0;
                    break;
                case 4:
                    offsetX = 0;
                    offsetY = ((float)source.Height - height) / 2;
                    break;
                case 6:
                    offsetX = (float)source.Width - width;
                    offsetY = ((float)source.Height - height) / 2;
                    break;
                case 7:
                    offsetX = 0;
                    offsetY = (float)source.Height - height;
                    break;
                case 8:
                    offsetX = ((float)source.Width - width) / 2;
                    offsetY = (float)source.Height - height;
                    break;
                case 9:
                    offsetX = (float)source.Width - width;
                    offsetY = (float)source.Height - height;
                    break;
                default:
                    offsetX = ((float)source.Width - width) / 2;
                    offsetY = ((float)source.Height - height) / 2;
                    break;
            }
            return Zoom(Crop(source, (int)offsetX, (int)offsetY, new Size((int)width, (int)height)), newSize);
        }

        /// <summary>
        /// 剪切并缩放图片对象
        /// </summary>
        /// <param name="newSize">缩放并剪切后的大小</param>
        /// <param name="cuteTpye">剪切类型1-9，5：居中剪切，其他的从左至右从上到下代表8个方向</param>
        /// <returns></returns>
        public void CropAndZoom(Size newSize, byte cuteTpye)
        {
            _sourceImage = CropAndZoom(_sourceImage == null ? _originalImage : _sourceImage, newSize, cuteTpye);
        }

        /// <summary>
        /// 缩放并剪切图片对象
        /// </summary>
        /// <param name="source">要缩放并剪切的图片对象</param>
        /// <param name="newSize">缩放并剪切后的大小</param>
        /// <param name="cuteTpye">剪切类型1-9，5：居中剪切，其他的从左至右从上到下代表8个方向</param>
        /// <returns></returns>
        public static Bitmap ZoomAndCrop(Bitmap source, Size newSize, byte cuteTpye)
        {
            if (source.Width == newSize.Width && source.Height == newSize.Height)
            {
                return source;
            }
            float offsetX = 0, offsetY = 0, width = 0, height = 0;
            if ((float)source.Width / (float)source.Height > (float)newSize.Width / (float)newSize.Height)
            {
                width = source.Width * newSize.Height / source.Height;
                height = newSize.Height;
            }
            else
            {
                width = newSize.Width;
                height = newSize.Width * source.Height / source.Width;
            }
            switch (cuteTpye)
            {
                case 1:
                    offsetX = 0;
                    offsetY = 0;
                    break;
                case 2:
                    offsetX = (width - (float)newSize.Width) / 2;
                    offsetY = 0;
                    break;
                case 3:
                    offsetX = width - (float)newSize.Width;
                    offsetY = 0;
                    break;
                case 4:
                    offsetX = 0;
                    offsetY = (height - (float)newSize.Height) / 2;
                    break;
                case 6:
                    offsetX = width - (float)newSize.Width;
                    offsetY = (height - (float)newSize.Height) / 2;
                    break;
                case 7:
                    offsetX = 0;
                    offsetY = height - (float)newSize.Height;
                    break;
                case 8:
                    offsetX = (width - (float)newSize.Width) / 2;
                    offsetY = height - (float)newSize.Height;
                    break;
                case 9:
                    offsetX = width - (float)newSize.Width;
                    offsetY = height - (float)newSize.Height;
                    break;
                default:
                    offsetX = (width - (float)newSize.Width) / 2;
                    offsetY = (height - (float)newSize.Height) / 2;
                    break;
            }
            return Crop(Zoom(source, new Size((int)width, (int)height)), (int)offsetX, (int)offsetY, newSize);

        }

        /// <summary>
        /// 缩放并剪切图片对象
        /// </summary>
        /// <param name="newSize">缩放并剪切后的大小</param>
        /// <param name="cuteTpye">剪切类型1-9，5：居中剪切，其他的从左至右从上到下代表8个方向</param>
        public void ZoomAndCrop(Size newSize, byte cuteTpye)
        {
            _sourceImage = ZoomAndCrop(_sourceImage == null ? _originalImage : _sourceImage, newSize, cuteTpye);
        }

        /// <summary>
        /// 图片圆角处理(不支持圆角透明)
        /// </summary>
        /// <param name="source">要处理的图片对象</param>
        /// <param name="cornerRadius">圆角半径</param>
        /// <param name="bg">圆角背景颜色</param>
        /// <returns>处理后的图片对象</returns>
        public static Bitmap Circularity(Bitmap source, int cornerRadius, Color bg)
        {
            int width = source.Width, height = source.Height;
            int diameter = cornerRadius * 2;
            GraphicsPath roundedRect = new GraphicsPath();
            roundedRect.AddArc(0, 0, diameter, diameter, 180, 90);
            roundedRect.AddLine(0, 0, 0, cornerRadius);
            roundedRect.CloseFigure();
            roundedRect.AddArc(width - diameter, 0, diameter, diameter, 270, 90);
            roundedRect.AddLine(width, 0, width - cornerRadius, 0);
            roundedRect.CloseFigure();
            roundedRect.AddArc(width - diameter, height - diameter, diameter, diameter, 0, 90);
            roundedRect.AddLine(width - cornerRadius, height, width, height);
            roundedRect.CloseFigure();
            roundedRect.AddArc(0, height - diameter, diameter, diameter, 90, 90);
            roundedRect.AddLine(0, height, cornerRadius, height);
            roundedRect.CloseFigure();
            Bitmap output = new Bitmap(source);
            Graphics g = Graphics.FromImage(output);
            Brush b = new SolidBrush(bg);//圆角背景
            g.DrawPath(new Pen(b), roundedRect);
            g.FillPath(b, roundedRect);
            g.Dispose();
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cornerRadius"></param>
        /// <param name="bg"></param>
        public void Circularity(int cornerRadius, Color bg)
        {
            this._sourceImage = Circularity(_sourceImage == null ? _originalImage : _sourceImage, cornerRadius, bg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sSrcFilePath"></param>
        /// <param name="sDstFilePath"></param>
        /// <param name="sText1"></param>
        /// <param name="sColor1"></param>
        /// <param name="sSize1"></param>
        /// <param name="sFont1"></param>
        /// <param name="sText2"></param>
        /// <param name="sColor2"></param>
        /// <param name="sSize2"></param>
        /// <param name="sFont2"></param>
        /// <param name="sBgColor"></param>
        /// <param name="sTransparence"></param>
        public static void CreateWatermark(string sSrcFilePath, string sDstFilePath, string sText1, string sColor1, string sSize1, string sFont1, string sText2, string sColor2, string sSize2, string sFont2, string sBgColor, string sTransparence)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(sSrcFilePath);
            Graphics g = Graphics.FromImage(image);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; //文字抗锯齿
            g.DrawImage(image, 0, 0, image.Width, image.Height);
            Font f1 = new Font(sFont1, float.Parse(sSize1));
            Font f2 = new Font(sFont2, float.Parse(sSize2));
            Brush brushfortext1 = new SolidBrush(ColorTranslator.FromHtml(sColor1));
            Brush brushfortext2 = new SolidBrush(ColorTranslator.FromHtml(sColor2));
            Brush brushforbg = new SolidBrush(Color.FromArgb(Convert.ToInt16(255 * float.Parse(sTransparence)), ColorTranslator.FromHtml(sBgColor)));
            g.RotateTransform(-20);
            Rectangle rect = new Rectangle(-image.Width / 2 - 50, image.Height - 50, image.Width * 2, 40);
            g.DrawRectangle(new Pen(brushforbg), rect);
            g.FillRectangle(brushforbg, rect);
            Rectangle rectfortext1 = new Rectangle(-image.Width / 2 + image.Width / 5, image.Height - 45, image.Width * 2, 60);
            for (int i = 0; i < 10; i++)
                g.DrawString(sText1, f1, brushfortext1, rectfortext1);
            Rectangle rectfortext2 = new Rectangle(-image.Width / 2 + image.Width / 5, image.Height - 25, image.Width * 2, 60);
            for (int i = 0; i < 10; i++)
                g.DrawString(sText2, f2, brushfortext2, rectfortext2);
            image.Save(sDstFilePath, ImageFormat.Jpeg);
            image.Dispose();

        }

        /// <summary>
        /// 在图片上添加文字
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <param name="fontColor"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontName"></param>
        /// <returns></returns>
        public static Bitmap CreatePlainText(Bitmap source, string text, Color fontColor, float fontSize, string fontName)
        {
            Bitmap output = new Bitmap(source);
            Graphics g = Graphics.FromImage(output);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; //文字抗锯齿
            Font f = new Font(fontName, fontSize);
            Brush b = new SolidBrush(fontColor);

            //计算文字区域的宽度和高度
            int txtWidth = (int)(fontSize * 1.6 * text.Length);
            int txtHeight = (int)(fontSize * 1.6 + 2);
            if (txtWidth > source.Width - 10)
            {
                txtWidth = source.Width - 10;
                //txtHeight = txtHeight * 2 + 5;
            }
            if (txtHeight > source.Height)
                txtHeight = source.Height;

            Rectangle rect = new Rectangle(5, source.Height - txtHeight - 5, txtWidth, txtHeight); //适当空开一段距离        
            for (int i = 0; i < 3; i++) //加强亮度
                g.DrawString(text, f, b, rect);
            return output;
        }

        /// <summary>
        /// 在图片上添加文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontColor"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontName"></param>
        public void CreatePlainText(string text, Color fontColor, float fontSize, string fontName)
        {
            this._sourceImage = CreatePlainText(_sourceImage == null ? _originalImage : _sourceImage, text, fontColor, fontSize, fontName);
        }


        /// <summary>
        /// 为图片添加图片水印
        /// </summary>
        /// <param name="original">原图</param>
        /// <param name="logo">水印图片路径</param>
        /// <param name="savePath">最后保存路径（物理路径）</param>
        /// <param name="pos">水印图片位置，1：左上角，2：右上角，3：左下角，4：右下角，5：中间</param>
        /// <param name="maxColor">最大色彩，2-255</param>
        /// <param name="maxColorBits">最大位数，1-8</param>
        /// <param name="ditherAmount">平滑度0-8</param>
        /// <param name="zipRate">JPG压缩比例1-100</param>
        public static void CreateWatermark(Bitmap original, string logo, string savePath, int pos, int maxColor, int maxColorBits, int ditherAmount, long zipRate)
        {
            //为了防止带索引的图片获取对象出错，先吧原图保存为JPG
            string tmpJpg = savePath.Substring(0, savePath.LastIndexOf(".")) + "tmp.jpg";
            original.Save(tmpJpg, ImageFormat.Jpeg);
            //SaveToJpeg(original, tmpJpg, 100);
            Image orgImage = Image.FromFile(tmpJpg);
            Image wtmImage = Image.FromFile(logo);
            int x = 0;
            int y = 0;
            switch (pos)
            {
                case 1:
                    x = 0;
                    y = 0;
                    break;
                case 2:
                    x = orgImage.Width - wtmImage.Width;
                    y = 0;
                    break;
                case 3:
                    x = 0;
                    y = orgImage.Height - wtmImage.Height;
                    break;
                case 4:
                    x = orgImage.Width - wtmImage.Width;
                    y = orgImage.Height - wtmImage.Height;
                    break;
                default:
                    x = (orgImage.Width - wtmImage.Width) / 2;
                    y = (orgImage.Height - wtmImage.Height) / 2;
                    break;
            }

            int w = wtmImage.Width > orgImage.Width ? orgImage.Width : wtmImage.Width;
            int h = wtmImage.Height > orgImage.Height ? orgImage.Height : wtmImage.Height;

            using (Graphics g = Graphics.FromImage(orgImage))
            {
                g.DrawImage(wtmImage, new Rectangle(x < 0 ? 0 : x, y < 0 ? 0 : y, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
            }

            Bitmap saveImage = new Bitmap(orgImage);
            orgImage.Dispose();
            Save(saveImage, savePath, maxColor, maxColorBits, ditherAmount, zipRate);
            File.Delete(tmpJpg);
        }

        /// <summary>
        /// 为图片添加图片水印
        /// </summary>
        /// <param name="original">原图</param>
        /// <param name="logo">水印图片路径</param>
        /// <param name="savePath">最后保存路径（物理路径）</param>
        /// <param name="pos">水印图片位置，1：左上角，2：右上角，3：左下角，4：右下角，5：中间</param>
        public static void CreateWatermark(Bitmap original, string logo, string savePath, int pos)
        {
            CreateWatermark(original, logo, savePath, pos, 255, 8, 8, 100);
        }


        /// <summary>
        /// 为图片添加图片水印
        /// </summary>
        /// <param name="logo">水印图片路径</param>
        /// <param name="savePath">最后保存路径（物理路径）</param>
        /// <param name="pos">水印图片位置，1：左上角，2：右上角，3：左下角，4：右下角，5：中间</param>
        /// <param name="maxColor">最大色彩，2-255</param>
        /// <param name="maxColorBits">最大位数，1-8</param>
        /// <param name="ditherAmount">平滑度0-8</param>
        /// <param name="zipRate">JPG压缩比例1-100</param>
        public void CreateWatermark(string logo, string savePath, int pos, int maxColor, int maxColorBits, int ditherAmount, long zipRate)
        {
            CreateWatermark(_sourceImage == null ? _originalImage : _sourceImage, logo, savePath, pos, maxColor, maxColorBits, ditherAmount, zipRate);
        }


        /// <summary>
        /// 为图片添加图片水印
        /// </summary>
        /// <param name="logo">水印图片路径</param>
        /// <param name="savePath">最后保存路径（物理路径）</param>
        /// <param name="pos">水印图片位置，1：左上角，2：右上角，3：左下角，4：右下角，5：中间</param>
        public void CreateWatermark(string logo, string savePath, int pos)
        {
            CreateWatermark(_sourceImage == null ? _originalImage : _sourceImage, logo, savePath, pos);
        }


        /// <summary>
        /// 把图像保存到GIF文件
        /// </summary>
        /// <param name="original">要保持的图片对象</param>
        /// <param name="fileName">保存的文件完整路径</param>
        public static void SaveToGif(Bitmap original, string fileName)
        {
            SaveToGif(original, fileName, 255, 8, 8);
        }
        
        
        /// <summary>
        /// 把图像保存到GIF文件
        /// </summary>
        /// <param name="original">要保持的图片对象</param>
        /// <param name="fileName">保存的文件完整路径</param>
        /// <param name="maxColor">最大色彩，2-255</param>
        /// <param name="maxColors">最大色度，1-8</param>
        /// <param name="ditherAmount">位深，0-8</param>
        public static void SaveToGif(Bitmap original, string fileName, int maxColor, int maxColors, int ditherAmount)
        {
            OctreeQuantizer gifQuantizer = new OctreeQuantizer(maxColor, maxColors);
            gifQuantizer.DitherLevel = ditherAmount;
            Bitmap current = gifQuantizer.Quantize(original);
            current.Save(fileName, ImageFormat.Gif);
            current.Dispose();
        }



        /// <summary>
        /// 把图像保存到Png文件
        /// </summary>
        /// <param name="original">要保持的图片对象</param>
        /// <param name="fileName">保存的文件完整路径</param>
        public static void SaveToPng(Bitmap original, string fileName)
        {
            SaveToPng(original, fileName, 128, 8, 0);
        }
        
        
        
        /// <summary>
        /// 把图像保存到Png文件
        /// </summary>
        /// <param name="original">要保持的图片对象</param>
        /// <param name="fileName">保存的文件完整路径</param>
        /// <param name="maxColor">最大色彩，2-255</param>
        /// <param name="maxColors">最大色度，1-8</param>
        /// <param name="ditherAmount">平滑，0-8</param>
        public static void SaveToPng(Bitmap original, string fileName, int maxColor, int maxColors, int ditherAmount)
        {
            Color color = original.GetPixel(0, 0);
            if (color.ToArgb() == 0 && color.Name == "0")
            {
                OctreeQuantizer quantizer = new OctreeQuantizer(maxColor, maxColors);
                quantizer.DitherLevel = ditherAmount;
                Bitmap target = quantizer.Quantize(original);
                ImageCodecInfo[] codes = ImageCodecInfo.GetImageEncoders();
                EncoderParameters encParam = new EncoderParameters(1);
                EncoderParameter interlace = new EncoderParameter(Encoder.ScanMethod, (byte)(EncoderValue.ScanMethodInterlaced));
                encParam.Param[0] = interlace;
                target.Save(fileName, codes[4], encParam);
                target.Dispose();
            }
            else
            {
                string obit = original.PixelFormat.ToString();
                string _path = fileName.Substring(0, fileName.LastIndexOf("."));
                string tmpGif = _path + ".gif";
                string tmpBmp = _path + ".bmp";
                OctreeQuantizer quantizer = new OctreeQuantizer(maxColor, maxColors);
                quantizer.DitherLevel = ditherAmount;
                Bitmap bm_gif = quantizer.Quantize(original);
                bm_gif.Save(tmpGif, ImageFormat.Gif);
                bm_gif.Dispose();

                File.Copy(tmpGif, fileName, true);
                File.Delete(tmpGif);

                Image image = Image.FromFile(fileName);
                image.Save(tmpBmp, ImageFormat.Bmp);
                image.Dispose();

                ImageCodecInfo[] codes = ImageCodecInfo.GetImageEncoders();
                EncoderParameters encParam = new EncoderParameters(1);
                EncoderParameter interlace = new EncoderParameter(Encoder.ScanMethod, (byte)(EncoderValue.ScanMethodInterlaced));
                encParam.Param[0] = interlace;
                Image img = Image.FromFile(tmpBmp);
                File.Delete(fileName);
                img.Save(fileName, codes[4], encParam);
                img.Dispose();
                File.Delete(tmpBmp);
            }
        }



        /// <summary>
        /// 保存为JPG
        /// </summary>
        /// <param name="bmImage">要保持的图片对象</param>
        /// <param name="savePath">保存的文件完整路径</param>
        public static void SaveToJpeg(Bitmap bmImage, string savePath)
        {
            SaveToJpeg(bmImage, savePath, 100);
        }
        
        
        
        /// <summary>
        /// 保存为JPG
        /// </summary>
        /// <param name="bmImage">要保持的图片对象</param>
        /// <param name="savePath">保存的文件完整路径</param>
        /// <param name="zipLevel">压缩比例，0-100</param>
        public static void SaveToJpeg(Bitmap bmImage, string savePath, long zipLevel)
        {
            EncoderParameters eParams = new EncoderParameters();
            eParams.Param[0] = new EncoderParameter(Encoder.Quality, new long[] { zipLevel });
            ImageCodecInfo eCode = GetEncoderInfo("image/jpeg");
            bmImage.Save(savePath, eCode, eParams);
            bmImage.Dispose();
        }


        /// <summary>
        /// 获取图片文件编码
        /// </summary>
        /// <param name="mimeType">文件类型</param>
        /// <returns>文件编码</returns>
        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            return ImageCodecInfo.GetImageEncoders().Where(e => e.MimeType.Equals(mimeType.ToLower())).SingleOrDefault();
        }

        /// <summary>
        /// 把图像保存到文件
        /// </summary>
        /// <param name="bmImage">要保持的图片对象</param>
        /// <param name="savePath">保存的文件完整路径</param>
        /// <param name="maxColor">最大色彩，2-255</param>
        /// <param name="maxColorBits">最大位数，1-8</param>
        /// <param name="ditherAmount">平滑度0-8</param>
        /// <param name="zipRate">JPG压缩比例1-100</param>
        public static void Save(Bitmap bmImage, string savePath, int maxColor, int maxColorBits, int ditherAmount, long zipRate)
        {
            switch (Path.GetExtension(savePath))
            {
                case ".png": SaveToPng(bmImage, savePath, maxColor, maxColorBits, ditherAmount); break;
                case ".gif": SaveToGif(bmImage, savePath, maxColor, maxColorBits, ditherAmount); break;
                case ".tiff": bmImage.Save(savePath, ImageFormat.Tiff); break;
                case ".bmp": bmImage.Save(savePath, ImageFormat.Bmp); break;
                case ".jpg":
                case ".jpeg": SaveToJpeg(bmImage, savePath, zipRate); break;
            }
        }

        /// <summary>
        /// 保存到文件---PNG
        /// </summary>
        /// <param name="savePath">文件完整路径</param>
        public void SavePNG(string savePath)
        {
            Save(_sourceImage == null ? _originalImage : _sourceImage, savePath, 128, 8, 4, 88);
        }


        /// <summary>
        /// 保存到文件---PNG
        /// </summary>
        /// <param name="savePath">文件完整路径</param>
        /// <param name="maxColor">最大色彩，2-255</param>
        /// <param name="maxColorBits">最大位数，1-8</param>
        /// <param name="ditherAmount">平滑度0-8</param>
        public void SavePNG(string savePath, int maxColor, int maxColorBits, int ditherAmount)
        {
            Save(_sourceImage == null ? _originalImage : _sourceImage, savePath, maxColor, maxColorBits, ditherAmount, 88);
        }



        /// <summary>
        /// 保存到文件---GIF
        /// </summary>
        /// <param name="savePath">文件完整路径</param>
        public void SaveGIF(string savePath)
        {
            Save(_sourceImage == null ? _originalImage : _sourceImage, savePath, 255, 8, 8, 88);
        }


        /// <summary>
        /// 保存到文件---GIF
        /// </summary>
        /// <param name="savePath">文件完整路径</param>
        /// <param name="maxColor">最大色彩，2-255</param>
        /// <param name="maxColorBits">最大位数，1-8</param>
        /// <param name="ditherAmount">平滑度0-8</param>
        public void SaveGIF(string savePath, int maxColor, int maxColorBits, int ditherAmount)
        {
            Save(_sourceImage == null ? _originalImage : _sourceImage, savePath, maxColor, maxColorBits, ditherAmount, 88);
        }

        /// <summary>
        /// 保存到文件---JPG
        /// </summary>
        /// <param name="savePath">文件完整路径</param>
        public void SaveJPG(string savePath)
        {
            Save(_sourceImage == null ? _originalImage : _sourceImage, savePath, 255, 8, 8, 88);
        }


        /// <summary>
        /// 保存到文件---JPG
        /// </summary>
        /// <param name="savePath">文件完整路径</param>
        /// <param name="zipRate">JPG压缩比例1-100</param>
        public void SaveJPG(string savePath, long zipRate)
        {
            Save(_sourceImage == null ? _originalImage : _sourceImage, savePath, 255, 8, 8, zipRate);
        }


        /// <summary>
        /// 保存到文件--默认全部参数最优
        /// </summary>
        /// <param name="savePath">文件完整路径</param>
        public void Save(string savePath)
        {
            Save(_sourceImage == null ? _originalImage : _sourceImage, savePath, 255, 8, 8, 100);
        }

        /// <summary>
        /// 把图像保存到文件
        /// </summary>
        /// <param name="bmImage">要保持的图片对象</param>
        /// <param name="savePath">保存的文件完整路径</param>
        /// <param name="createDirectory">是否创建目录</param>
        /// <param name="maxColor">最大色彩，2-255</param>
        /// <param name="maxColorBits">最大位数，1-8</param>
        /// <param name="ditherAmount">平滑度0-8</param>
        /// <param name="zipRate">JPG压缩比例1-100</param>
        public static void Save(Bitmap bmImage, string savePath, bool createDirectory, int maxColor, int maxColorBits, int ditherAmount, long zipRate)
        {
            string dir = Path.GetDirectoryName(savePath);
            if (createDirectory && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Save(bmImage, savePath, maxColor, maxColorBits, ditherAmount, zipRate);
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="savePath">文件完整路径</param>
        /// <param name="createDirectory">是否创建目录</param>
        /// <param name="maxColor">最大色彩，2-255</param>
        /// <param name="maxColorBits">最大位数，1-8</param>
        /// <param name="ditherAmount">平滑度0-8</param>
        /// <param name="zipRate">JPG压缩比例1-100</param>
        public void Save(string savePath, bool createDirectory, int maxColor, int maxColorBits, int ditherAmount, long zipRate)
        {
            Save(_sourceImage == null ? _originalImage : _sourceImage, savePath, createDirectory, maxColor, maxColorBits, ditherAmount, zipRate);
        }


        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="savePath">文件完整路径</param>
        /// <param name="createDirectory">是否创建目录</param>
        public void Save(string savePath, bool createDirectory)
        {
            Save(_sourceImage == null ? _originalImage : _sourceImage, savePath, createDirectory, 255, 8, 8, 100);
        }

        /// <summary>
        /// 释放ImageHelper使用的资源
        /// </summary>
        public void Dispose()
        {
            if (_sourceImage != null)
            {
                _sourceImage.Dispose();
            }
            _originalImage.Dispose();
        }
    }
}