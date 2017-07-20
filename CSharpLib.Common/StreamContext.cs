using System;
using System.IO;
using System.Text;
using CSharpLib.Common;

namespace CSharpLib.Common
{
    /// <summary>
    /// 输入、输出流
    /// </summary>
    public class StreamContext
    {
        /// <summary>
        /// 读取
        /// </summary>
        private BinaryReader Reader { get; set; }

        /// <summary>
        /// 写入
        /// </summary>
        private BinaryWriter Writer { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        private Encoding Encoding { get; set; }

        /// <summary>
        /// 输入、输出流
        /// </summary>
        /// <param name="reader">读取</param>
        /// <param name="write">写入</param>
        /// <param name="encoding">编码</param>
        public StreamContext(BinaryReader reader, BinaryWriter write, Encoding encoding)
        {
            this.Reader = reader;
            this.Writer = write;
            this.Encoding = encoding;
        }

        /// <summary>
        /// 输入、输出流
        /// </summary>
        /// <param name="reader">读取</param>
        /// <param name="encoding">编码</param>
        public StreamContext(BinaryReader reader, Encoding encoding)
        {
            this.Reader = reader;
            this.Encoding = encoding;
        }

        /// <summary>
        /// 输入、输出流
        /// </summary>
        /// <param name="write">写入</param>
        /// <param name="encoding">编码</param>
        public StreamContext(BinaryWriter write, Encoding encoding)
        {
            this.Writer = write;
            this.Encoding = encoding;
        }

        #region 读取输入流
        
        /// <summary>
        /// 从请求数据流中读取下一个字节，并使流的当前位置提升 1 个字节。
        /// </summary>
        /// <returns>从EASE请求数据流中读取的下一个字节。</returns>
        public byte ReadByte()
        {
            return this.Reader.ReadByte();
        }

        /// <summary>
        /// 从EASE请求数据流中读取下一段指定长度的字节，并使流的当前位置提升 指定长度 个字节。
        /// </summary>
        /// <param name="len">长度</param>
        /// <returns>字节数组</returns>
        public byte[] ReadBytes(int len)
        {
            return this.Reader.ReadBytes(len);
        }

        /// <summary>
        /// 从请求数据流中读取 2 字节无符号整数，并将流的当前位置提升 2 个字节。
        /// </summary>
        /// <returns>从EASE请求数据流中读取的 2 字节无符号整数。</returns>
        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(this.Reader.ReadBytes(2).Reverse(), 0);
        }

        /// <summary>
        /// 从请求数据流中读取 4 字节无符号整数，并将流的当前位置提升 4 个字节。
        /// </summary>
        /// <returns>从EASE请求数据流中读取的 4 字节无符号整数。</returns>
        public uint ReadUInt32()
        {
            return BitConverter.ToUInt32(this.Reader.ReadBytes(4).Reverse(), 0);
        }

        /// <summary>
        /// 从请求数据流中读取 8 字节无符号整数，并将流的当前位置提升 8 个字节。
        /// </summary>
        /// <returns>从EASE请求数据流中读取的 8 字节无符号整数。</returns>
        public ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(this.Reader.ReadBytes(8).Reverse(), 0);
        }

        /// <summary>
        /// 从请求数据流中读取 2 字节无符号整数，并将流的当前位置提升 2 个字节。并继续读取该整数长度的字节，并使当前流当前位置提升该长度个字节，然后将字节数组编码为制定编码的字符串
        /// </summary>
        /// <returns>编码后的字符串</returns>
        public string ReadString()
        {
            return this.Encoding.GetString(this.Reader.ReadBytes(this.ReadUInt16()));
        }

        #endregion

        #region 写入输出流

        /// <summary>
        /// 将一个无符号字节写入流，并将流的当前位置提升 1 个字节。
        /// </summary>
        /// <param name="value">要写入的无符号字节。</param>
        public void Write(byte value)
        {
            this.Writer.Write(value);
        }

        /// <summary>
        /// 将 2 字节无符号整数写入流，并将流的当前位置提升 2 个字节。
        /// </summary>
        /// <param name="value">要写入的 2 字节无符号整数。</param>
        public void Write(ushort value)
        {
            this.Writer.Write(BitConverter.GetBytes(value).Reverse());
        }

        /// <summary>
        /// 将 4 字节无符号整数写入流，并将流的当前位置提升 4 个字节。
        /// </summary>
        /// <param name="value">要写入的 4 字节无符号整数。</param>
        public void Write(uint value)
        {
            this.Writer.Write(BitConverter.GetBytes(value).Reverse());
        }

        /// <summary>
        /// 将 8 字节无符号整数写入流，并将流的当前位置提升 8 个字节。
        /// </summary>
        /// <param name="value">要写入的 8 字节无符号整数。</param>
        public void Write(ulong value)
        {
            this.Writer.Write(BitConverter.GetBytes(value).Reverse());
        }

        /// <summary>
        /// 将字符串编码后写入流，并将流的当前位置提升至编码后字节流长度再加 2 个字节（位于字符串前面：用于描述字符串编码后流长度的 2 字节无符号整数）。
        /// </summary>
        /// <param name="content">要编码的字符串内容。</param>
        /// <remarks>字符串默认使用UTF-8编码。</remarks>
        public void Write(string content)
        {
            this.Write(this.Encoding, content);
        }

        /// <summary>
        /// 将字符串编码后写入流，并将流的当前位置提升至编码后字节流长度再加 2 个字节（位于字符串前面：用于描述字符串编码后流长度的 2 字节无符号整数）。
        /// </summary>
        /// <param name="encoding">字符串要采用的编码格式。</param>
        /// <param name="content">要编码的字符串内容。</param>
        public void Write(Encoding encoding, string content)
        {
            byte[] buffer = encoding.GetBytes(content);
            this.Write((ushort)buffer.Length);
            this.Writer.Write(buffer);
        }

        /// <summary>
        /// 将字节流数据写入流，并将流的当前位置提升至输入字节流长度再加 4 个字节（位于输入字节流前面：用于描述输入字节流长度的 4 字节无符号整数）。
        /// </summary>
        /// <param name="buffer">要输入的字节流数据。</param>
        public void WriteStream(byte[] buffer)
        {
            this.Write((uint)buffer.Length);
            this.Writer.Write(buffer);
        }

        /// <summary>
        /// 将文件字节流数据写入流，并将流的当前位置提升至输入字节流长度再加 4 个字节（位于输入字节流前面：用于描述输入字节流长度的 4 字节无符号整数）。
        /// </summary>
        /// <param name="fullpath">要读取的文件物理路径。</param>
        public void WriteStream(string fullpath)
        {
            byte[] buffer;
            using (FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
            }
            this.WriteStream(buffer);
        }
        
        #endregion

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (this.Writer != null)
            {
                this.Writer.Close();
                this.Writer.Dispose();
            }
            if (this.Reader != null)
            {
                this.Reader.Close();
                this.Reader.Dispose();
            }
        }
    }
}