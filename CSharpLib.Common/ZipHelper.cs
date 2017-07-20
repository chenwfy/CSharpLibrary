using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace CSharpLib.Common
{
    /// <summary>
    ///zip文件操作类
    /// </summary>
    public class ZipHelper
    {
        private Stream zipFile;
        private string targetPath;

        /// <summary>
        /// 用指定ZIP文件路径初始化实例
        /// </summary>
        /// <param name="filePath"></param>
        public ZipHelper(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("文件不存在");
            }
            else
            {
                zipFile = File.OpenRead(filePath);
                targetPath = Path.GetDirectoryName(filePath);
            }
        }

        /// <summary>
        /// 用指定文件流和解压目录初始化实例
        /// </summary>
        /// <param name="fileStream">ZIP文件流</param>
        /// <param name="targetPath">解压到的目录</param>
        public ZipHelper(Stream fileStream, string targetPath)
        {
            zipFile = fileStream;
        }

        /// <summary>
        /// 解压指定流到指定目录
        /// </summary>
        /// <param name="fileStream">ZIP文件流</param>
        /// <param name="targetPath">解压到的目录</param>
        public static void Unzip(Stream fileStream, string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                throw new ArgumentException("目标目录不存在");
            }
            using (ZipInputStream s = new ZipInputStream(fileStream))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string filePath = Path.Combine(targetPath, theEntry.Name);
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    // create directory
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(Path.Combine(targetPath, directoryName));
                    }

                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(filePath))
                        {

                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解压指定路径的文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void Unzip(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("文件不存在");
            }
            else
            {
                Unzip(File.OpenRead(filePath), Path.GetDirectoryName(filePath));
            }
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        public void Unzip()
        {
            Unzip(zipFile, targetPath);
        }
        
        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose()
        {
            zipFile.Dispose();
        }
    }
}