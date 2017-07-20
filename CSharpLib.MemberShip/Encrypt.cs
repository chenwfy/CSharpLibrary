using System;
using System.Text;
using System.Configuration;
using System.Security.Cryptography;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 用户密码加密辅助类
    /// </summary>
    public class Encrypt
    {
        /// <summary>
        /// 
        /// </summary>
        private Encrypt()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        static Encrypt()
        {
        }

        /// <summary>
        /// 自动生成密码混淆码
        /// </summary>
        /// <returns>混淆码</returns>
        public static string CreateSalt()
        {
            byte[] bytSalt = new byte[int.Parse(ConfigurationManager.AppSettings["MemberShip.PasswordSaltSizeInBytes"])];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytSalt);
            return Convert.ToBase64String(bytSalt);
        }

        /// <summary>
        /// 将明文密码加密成指定格式
        /// </summary>
        /// <param name="format">密码格式</param>
        /// <param name="cleanString">用户明文密码</param>
        /// <param name="salt">密码混淆码</param>
        /// <returns>返回加密后的密码字符串</returns>
        public static string EncryptPassword(PasswordFormat format, string cleanString, string salt)
        {
            Encoding encoding = Encoding.GetEncoding(ConfigurationManager.AppSettings["MemberShip.PasswordEncodingFormat"]);
            byte[] clearBytes = encoding.GetBytes(salt.Trim() + "gw" + cleanString);
            byte[] hashedBytes;
            switch (format)
            {
                case PasswordFormat.ClearText:
                    return cleanString;
                case PasswordFormat.MD5:
                    hashedBytes = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encoding.GetBytes(cleanString));
                    return BitConverter.ToString(hashedBytes);
                case PasswordFormat.Sha1:
                    hashedBytes = ((HashAlgorithm)CryptoConfig.CreateFromName("SHA1")).ComputeHash(encoding.GetBytes(cleanString));
                    return BitConverter.ToString(hashedBytes);
                case PasswordFormat.MD5Hash:
                    hashedBytes = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(clearBytes);
                    return BitConverter.ToString(hashedBytes);
                case PasswordFormat.Sha1Hash:
                    hashedBytes = ((HashAlgorithm)CryptoConfig.CreateFromName("SHA1")).ComputeHash(clearBytes);
                    return BitConverter.ToString(hashedBytes);
                default:
                    hashedBytes = ((HashAlgorithm)CryptoConfig.CreateFromName("SHA1")).ComputeHash(clearBytes);
                    return BitConverter.ToString(hashedBytes);
            }
        }
    }
}