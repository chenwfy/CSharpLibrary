using System;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 成员资格管理接口
    /// </summary>
    public interface IUsership
    {
        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="user">用户相关信息</param>
        /// <returns>创建用户的状态</returns>
        CreateUserStatus CreateUser(IUser user);

        /// <summary>
        /// 更改用户密码
        /// </summary>
        /// <param name="user">用户相关信息</param>
        /// <param name="newPassword">新密码</param>
        /// <returns>密码更改是否成功</returns>
        bool ChangePassword(IUser user, string newPassword);

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="user">用户登录的基本信息</param>
        /// <returns>用户信息</returns>
        IUser GetUserInfo(IUser user);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="user">用户相关信息</param>
        /// <returns>操作结果</returns>
        bool UpdateUser(IUser user);

        /// <summary>
        /// 删除用户信息
        /// </summary>
        /// <param name="user">用户相关信息</param>
        /// <returns>操作结果</returns>
        bool RemoveUser(IUser user);

        /// <summary>
        /// 获取所有用户列表
        /// </summary>
        /// <returns>用户列表</returns>
        IUser[] GetUserList();

        /// <summary>
        /// 按照用户名匹配搜索用户信息
        /// </summary>
        /// <param name="userNameForMatch">用户名匹配字符</param>
        /// <returns>匹配到的用户信息列表</returns>
        IUser[] FindUserList(string userNameForMatch);
    }
}