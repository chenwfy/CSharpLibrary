using System;
using System.IO;
using System.Text;

namespace CSharpLib.Common
{
    /// <summary>
    /// 文件操作帮助类
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// 按指定的编码将指定文件读取为字节流，如果文件不存在则返回NULL
        /// </summary>
        /// <param name="filePath">待读取的文件路径</param>
        /// <returns>文件字节流</returns>
        public static byte[] ReadFile(this string filePath)
        {
            return filePath.ReadFile(Encoding.UTF8);
        }

        /// <summary>
        /// 按指定的编码将指定文件读取为字节流，如果文件不存在则返回NULL
        /// </summary>
        /// <param name="filePath">待读取的文件路径</param>
        /// <param name="encoding">读取编码</param>
        /// <returns>文件字节流</returns>
        public static byte[] ReadFile(this string filePath, Encoding encoding)
        {
            if (!File.Exists(filePath) || !filePath.FileSaveCompleted())
                return null;
            
            using (FileStream FS = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                BinaryReader br = new BinaryReader(FS, encoding);
                byte[] buffer = br.ReadBytes((int)FS.Length);
                br.Close();
                FS.Close();

                return buffer;
            }
        }

        /// <summary>
        /// 按指定的编码将字节流保存为指定路径的文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileData">待保存的数据</param>
        /// <returns>保存文件是否成功</returns>
        public static bool WriteFile(this string filePath, byte[] fileData)
        {
            return filePath.WriteFile(fileData, Encoding.UTF8);
        }

        /// <summary>
        /// 按指定的编码将字节流保存为指定路径的文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileData">待保存的数据</param>
        /// <param name="encoding">保存编码</param>
        /// <returns>保存文件是否成功</returns>
        public static bool WriteFile(this string filePath, byte[] fileData, Encoding encoding)
        {
            if (null == fileData || fileData.Length == 0)
                return false;

            if (File.Exists(filePath) && !filePath.FileSaveCompleted())
                return false;

            using (FileStream FS = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                BinaryWriter bw = new BinaryWriter(FS, encoding);
                bw.Write(fileData);
                bw.Close();
                FS.Close();

                return true;
            }
        }

        /// <summary>
        /// 判断文件是否已经保存完成
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileSaveCompleted(this string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            int count = 0;
            bool result = false;
            while (count < 100)
            {
                try
                {
                    using (FileStream FS = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        result = true;
                        FS.Close();
                        FS.Dispose();
                        break;
                    }
                }
                catch
                {
                    result = false;
                    count++;
                    System.Threading.Thread.Sleep(10);
                }
            }
            return result;
        }
    }
}