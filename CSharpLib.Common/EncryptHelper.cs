using System;
using System.Text;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace CSharpLib.Common
{
    /// <summary>
    /// 加密解密相关帮助类
    /// </summary>
    public static class EncryptHelper
    {
        #region MD5

        /// <summary>
        /// 生成指定长度的随机混淆码
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>生成的随机混淆码</returns>
        public static string CreateSalt(int length)
        {
            if (length < 1)
                return string.Empty;

            byte[] bytSalt = new byte[length];
            new RNGCryptoServiceProvider().GetBytes(bytSalt);
            return Convert.ToBase64String(bytSalt);
        }

        /// <summary>
        /// 获取指定的明文字符串加密为MD5密码串
        /// </summary>
        /// <param name="sourceString">待加密的字串</param>
        /// <returns>加密后的MD5密码串</returns>
        public static string CreateMD5Encrypt(this string sourceString)
        {
            return sourceString.CreateMD5Encrypt(Encoding.UTF8);
        }

        /// <summary>
        /// 获取指定的明文字符串加密为MD5 16位密码串（小写）
        /// </summary>
        /// <param name="sourceString">待加密的字串</param>
        /// <returns>加密后的MD5密码串</returns>
        public static string CreateMD5EncryptShort(this string sourceString)
        {
            return sourceString.CreateMD5Encrypt(Encoding.UTF8).Replace("-", "").ToLower().Substring(8, 16);
        }

        /// <summary>
        /// 获取指定的明文字符串加密为MD5密码串
        /// </summary>
        /// <param name="sourceString">待加密的字串</param>
        /// <param name="encoding">编码格式</param>
        /// <returns>加密后的MD5密码串</returns>
        public static string CreateMD5Encrypt(this string sourceString, Encoding encoding)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                return sourceString.CreateMD5Encrypt(md5, encoding);
            }
        }

        /// <summary>
        /// 获取指定的明文字符串加密为MD5密码串
        /// </summary>
        /// <param name="sourceString">待加密的字串</param>
        /// <param name="hashAlgorithm">加密算法</param>
        /// <param name="encoding">编码格式</param>
        /// <returns>加密后的MD5密码串</returns>
        public static string CreateMD5Encrypt(this string sourceString, HashAlgorithm hashAlgorithm, Encoding encoding)
        {
            return BitConverter.ToString(hashAlgorithm.ComputeHash(encoding.GetBytes(sourceString)));
        }

        #endregion

        #region BASE64

        /// <summary>
        /// 获取指定明文字符串你的BASE64编码字串（原生带混淆）
        /// </summary>
        /// <param name="sourceString">待编码的原始字符串</param>
        /// <returns>编码后的BASE64字串</returns>
        public static string CreateBase64Encode(this string sourceString)
        {
            if (string.IsNullOrEmpty(sourceString))
                return null;

            byte[] buffer = Encoding.UTF8.GetBytes(sourceString);
            string orgBase64 = Convert.ToBase64String(buffer);
            StringBuilder sBuilder = new StringBuilder("U3");
            for (int i = 0; i < orgBase64.Length; i++)
            {
                sBuilder.Append((char)(orgBase64[i]));
            }
            sBuilder.Append("7P");
            return HttpUtility.UrlEncode(sBuilder.ToString());
        }

        /// <summary>
        /// 获取指定的BASE64编码（原生带混淆）后字串的原始明文字串
        /// </summary>
        /// <param name="sourceString">待解码的编码字符</param>
        /// <returns>解码后的明文字符串</returns>
        public static string CreateBase64Decode(this string sourceString)
        {
            if (string.IsNullOrEmpty(sourceString))
                return null;

            string orgBase64 = sourceString.Substring(2, sourceString.Length - 4);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < orgBase64.Length; i++)
            {
                sBuilder.Append((char)(orgBase64[i]));
            }
            byte[] buffer = Convert.FromBase64String(sBuilder.ToString());
            return Encoding.UTF8.GetString(buffer);
        }

        #endregion

        #region DES

        /// <summary>
        /// 加密向量
        /// </summary>
        private static readonly byte[] DES_KEY = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        /// <summary>
        /// 修正秘钥字串
        /// </summary>
        /// <param name="key">原始秘钥</param>
        /// <returns>修正后的秘钥</returns>
        private static byte[] GetKeyBuffer(string key)
        {
            string encryptKey = key;
            if (key.Length > 8)
                encryptKey = key.Substring(0, 8);
            else
                encryptKey = key.PadRight(8, ' ');
            return Encoding.UTF8.GetBytes(encryptKey);
        }

        /// <summary>
        /// 获取指定明文字符串和秘钥的DES加密字串
        /// </summary>
        /// <param name="sourceString">待加密的明文字串</param>
        /// <param name="key">秘钥（长度应大于等于8位）</param>
        /// <returns>加密后的密码串</returns>
        public static string CreateDESEncod(this string sourceString, string key)
        {
            byte[] keyBuffer = GetKeyBuffer(key);
            byte[] sourceBuffer = Encoding.UTF8.GetBytes(sourceString);
            using (DESCryptoServiceProvider desService = new DESCryptoServiceProvider())
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream stream = new CryptoStream(mStream, desService.CreateEncryptor(keyBuffer, DES_KEY), CryptoStreamMode.Write))
                    {
                        stream.Write(sourceBuffer, 0, sourceBuffer.Length);
                        stream.FlushFinalBlock();
                        return Convert.ToBase64String(mStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定的密码串用指定的秘钥解密后的明文字串
        /// </summary>
        /// <param name="sourceString">待解密的密码字串</param>
        /// <param name="key">秘钥（长度应大于等于8位，并且和加密秘钥相同）</param>
        /// <returns>明文字串</returns>
        public static string CreateDESDecode(this string sourceString, string key)
        {
            byte[] keyBuffer = GetKeyBuffer(key);
            byte[] sourceBuffer = Convert.FromBase64String(sourceString);
            using (DESCryptoServiceProvider desService = new DESCryptoServiceProvider())
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream stream = new CryptoStream(mStream, desService.CreateDecryptor(keyBuffer, DES_KEY), CryptoStreamMode.Write))
                    {
                        stream.Write(sourceBuffer, 0, sourceBuffer.Length);
                        stream.FlushFinalBlock();
                        return Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
            }
        }

        #endregion

        #region int Offset

        /// <summary>
        /// 获取整数连续偏移后的四字节数组
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static byte[] GetIntOffsetBytes(this int sourceNum)
        {
            long b1 = (sourceNum & 0xff000000) >> 24;
            if (b1 < 0)
                b1 += 0x100;
            long b2 = (sourceNum & 0x00ff0000) >> 16;
            if (b2 < 0)
                b2 += 0x100;
            long b3 = (sourceNum & 0x0000ff00) >> 8;
            if (b3 < 0)
                b3 += 0x100;
            long b4 = (sourceNum & 0x000000ff);
            if (b4 < 0)
                b4 += 0x100;

            return new byte[4] { (byte)b1, (byte)b2, (byte)b3, (byte)b4 };
        }

        /// <summary>
        /// 从连续偏移的四字节数组还原为int整数
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static int GetIntFromOffsetBytes(this byte[] buffer)
        {
            long sourceNum = (long)buffer[3] & 0x000000ff;
            sourceNum += ((long)buffer[2] << 8) & 0x0000ff00;
            sourceNum += ((long)buffer[1] << 16) & 0x00ff0000;
            sourceNum += ((long)buffer[0] << 24) & 0xff000000;
            return (int)sourceNum;
        }

        #endregion
    }
}