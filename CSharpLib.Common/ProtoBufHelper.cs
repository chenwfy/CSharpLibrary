using System;
using System.IO;
using ProtoBuf;

namespace CSharpLib.Common
{
    /// <summary>
    /// PROTOBUF类型数据序列化与反序列化帮助类
    /// </summary>
    public static class ProtoBufHelper
    {
        /// <summary>
        /// 将输入流反序列为指定的对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="inputStream">输入流</param>
        /// <returns>反序列化后的结果</returns>
        public static T ProtoBufDeserialize<T>(this Stream inputStream)
        {
            return ProtoBuf.Serializer.Deserialize<T>(inputStream);
        }

        /// <summary>
        /// 将输入字节流反序列为指定的对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="inputBuffer">输入字节流</param>
        /// <returns>反序列化后的结果</returns>
        public static T ProtoBufDeserialize<T>(this byte[] inputBuffer)
        {
            using (MemoryStream stream = new MemoryStream(inputBuffer))
            {
                return stream.ProtoBufDeserialize<T>();
            }
        }

        /// <summary>
        /// 将输入字节流反序列为指定类型的对象
        /// </summary>
        /// <param name="typeName">目标对象类型</param>
        /// <param name="inputBuffer">输入字节流</param>
        /// <returns>反序列化后的OBJECT对象</returns>
        public static object ProtoBufDeserialize(this string typeName, byte[] inputBuffer)
        {
            Type type = Type.GetType(typeName);
            if (null != type)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(inputBuffer, 0, inputBuffer.Length);
                    ms.Position = 0;
                    return Serializer.NonGeneric.Deserialize(type, ms);
                }
            }
            return null;
        }

        /// <summary>
        /// 将指定类型的对象序列化为字节流
        /// </summary>
        /// <typeparam name="T">待序列化的对象类型</typeparam>
        /// <param name="source">待序列化的对象</param>
        /// <returns>字节流</returns>
        public static byte[] ProtoBufSerialize<T>(this T source)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize<T>(stream, source);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// 将指定类型的对象序列化为字节流
        /// </summary>
        /// <param name="source">待序列化的对象</param>
        /// <returns>字节流</returns>
        public static byte[] ProtoBufSerialize(this object source)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, source);
                return ms.ToArray();
            }
        }
    }
}