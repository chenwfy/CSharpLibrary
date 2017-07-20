using System;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 密码加密格式
    /// </summary>
    public enum PasswordFormat
    {
        /// <summary>
        /// 明文密码
        /// </summary>
        ClearText = 0,

        /// <summary>
        /// MD5加密
        /// </summary>
        MD5 = 1,

        /// <summary>
        /// Sha1加密
        /// </summary>
        Sha1 = 2,

        /// <summary>
        /// 加Salt后MD5加密
        /// </summary>
        MD5Hash = 3,

        /// <summary>
        /// 加Salt后Sha1加密
        /// </summary>
        Sha1Hash = 4
    }
}