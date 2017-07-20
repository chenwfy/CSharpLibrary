using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace CSharpLib.Common
{
    /// <summary>
    /// 公用辅助类
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// 获取页面请求传递参数的基本类型值（比如QueryString或Form回传的值）
        /// </summary>
        /// <typeparam name="T">要转换成的基本类型</typeparam>
        /// <param name="name">请求参数名称，如查询参数名、回传参数名或服务器控件对象名。</param>
        /// <returns>转换后的类型值</returns>
        /// <exception cref="NotSupportedException">参数T不受支持</exception>
        public static T Request<T>(this string name)
        {
            return name.Request<T>(HttpContext.Current);
        }

        /// <summary>
        /// 获取页面请求传递参数的基本类型值（比如QueryString或Form回传的值）
        /// </summary>
        /// <typeparam name="T">要转换成的基本类型</typeparam>
        /// <param name="name">请求参数名称，如查询参数名、回传参数名或服务器控件对象名。</param>
        /// <param name="context">当前的HTTP会话对象</param>
        /// <returns>转换后的类型值</returns>
        /// <exception cref="NotSupportedException">参数T不受支持</exception>
        public static T Request<T>(this string name, HttpContext context)
        {
            if (null != context)
                return context.Request[name].Parse<T>();
            return "".Parse<T>();
        }


        /// <summary>
        /// 集合元素拷贝
        /// </summary>
        /// <typeparam name="T">元素数据类型</typeparam>
        /// <param name="source">原始数据集合</param>
        /// <param name="copySize">拷贝数量</param>
        /// <param name="copyIndex">按照拷贝数量划分，第几次进行拷贝</param>
        /// <returns>拷贝出来的元素集合</returns>
        public static IEnumerable<T> SkipTake<T>(this IEnumerable<T> source, int copySize, int copyIndex)
        {
            return source.Skip((copyIndex - 1) * copySize).Take(copySize);
        }

        /// <summary>
        /// 将DataRow转换为Dictionary<string, object>对象
        /// </summary>
        /// <param name="row">DataRow对象</param>
        /// <returns>转换后的Dictionary<string, object>对象</returns>
        public static Dictionary<string, object> DataRowToDict(this DataRow row)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (DataColumn column in row.Table.Columns)
            {
                dict.Add(column.ColumnName, row[column]);
            }
            return dict;
        }

        /// <summary>
        /// 把字节流数据转换成支持序列化的基础对象
        /// </summary>
        /// <param name="bytes">字节流数据</param>
        /// <returns>支持序列化的基础对象</returns>
        public static object ToObject(this byte[] bytes)
        {
            if (bytes == null)
                return null;
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    ms.Write(bytes, 0, bytes.Length);
                    ms.Position = 0;
                    object obj = binaryFormatter.Deserialize(ms);
                    ms.Close();
                    return obj;
                }
            }
        }

        /// <summary>
        /// 把支持序列化的基础对象转换成字节流数据
        /// </summary>
        /// <param name="obj">支持序列化的基础对象</param>
        /// <returns>字节流数据</returns>
        public static byte[] ToBytes(this object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, obj);
                byte[] buffer = new Byte[ms.Length];
                ms.Position = 0;
                ms.Read(buffer, 0, buffer.Length);
                ms.Close();
                return buffer;
            }
        }

        /// <summary>
        /// 将键值对字串转换为键值对集合
        /// </summary>
        /// <param name="nameValues">待转换的字串</param>
        /// <returns>键值对集合</returns>
        public static NameValueCollection ToNameValueCollection(this string nameValues)
        {
            NameValueCollection nvc = new NameValueCollection();
            string[] rows = nameValues.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < rows.Length; i++)
            {
                string[] columns = rows[i].Split(':');
                string key = columns[0].Trim();
                nvc[key] = rows[i].Substring(rows[i].IndexOf(":") + 1).Trim();
            }
            return nvc;
        }

        /// <summary>
        /// 反转字节流顺序。
        /// </summary>
        /// <param name="bytes">原始字节流数据。</param>
        /// <returns>反转后的字节流数据。</returns>
        public static byte[] Reverse(this byte[] bytes)
        {
            int num = bytes.Length / 2;
            byte by;
            int idx;
            for (int i = 0; i < num; i++)
            {
                by = bytes[i];
                idx = bytes.Length - i - 1;
                bytes[i] = bytes[idx];
                bytes[idx] = by;
            }
            return bytes;
        }

        /// <summary>
        /// ZLIB压缩
        /// </summary>
        /// <param name="buffer">待压缩的字节数组</param>
        /// <returns>压缩后的字节流</returns>
        public static byte[] Compress(this byte[] buffer)
        {
            using (MemoryStream mMemory = new MemoryStream())
            {
                Deflater mDeflater = new Deflater(ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_COMPRESSION);
                using (DeflaterOutputStream mStream = new DeflaterOutputStream(mMemory, mDeflater, 131072))
                {
                    mStream.Write(buffer, 0, buffer.Length);
                    return mMemory.ToArray();
                }
            }
        }

        /// <summary>
        /// ZLIB压缩内容解压
        /// </summary>
        /// <param name="buffer">待解压的字节数组</param>
        /// <returns>解压后的字节流</returns>
        public static byte[] DescCompress(this byte[] buffer)
        {
            byte[] mWriteData = new byte[30720];
            Inflater inflater = new Inflater();
            inflater.SetInput(buffer);
            inflater.Inflate(mWriteData);
            byte[] result = new byte[inflater.TotalOut];
            Array.Copy(mWriteData, 0, result, 0, inflater.TotalOut);
            return result;
        }

        /// <summary>
        /// 指定对象进行反射
        /// 返回第一个元素：反射对象类型名称 .GetType().ToString()
        /// 返回第二个元素：反射对象公开成员名称集合
        /// 返回第三个元素：反射对象公开成员对应的值集合
        /// </summary>
        /// <param name="obj">待反射处理的对象</param>
        /// <returns>反射结果</returns>
        public static Tuple<string, IEnumerable<string>, IEnumerable<object>> ToReflection(this object obj)
        {
            string typeName = obj.GetType().ToString();
            List<string> pNameList = new List<string>(0);
            List<object> pValueList = new List<object>(0);
            foreach (var item in obj.GetType().GetProperties())
            {
                string name = item.Name;
                pNameList.Add(name);
                pValueList.Add(item.GetValue(obj, null));
            }
            return new Tuple<string, IEnumerable<string>, IEnumerable<object>>(typeName, pNameList, pValueList);
        }
    }
}