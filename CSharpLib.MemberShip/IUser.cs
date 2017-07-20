using System;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 成员用户接口
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// 获取或设置用户ID
        /// </summary>
        int UserId { get; set; }

        /// <summary>
        /// 获取或设置用户名
        /// </summary>
        string UserName { get; set; }
    }
}