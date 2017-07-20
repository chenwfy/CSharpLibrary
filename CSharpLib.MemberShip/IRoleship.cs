using System;
using System.Collections.Generic;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 角色管理接口
    /// </summary>
    public interface IRoleship
    {
        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="role">角色信息</param>
        /// <returns>创建结果</returns>
        CreateRoleStatus CreateRole(IRole role);

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="role">更新后角色信息</param>
        /// <returns>是否已更新</returns>
        bool UpdateRole(IRole role);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="role">角色信息</param>
        /// <param name="checkRoleInUse">是否检查角色已被使用，若检查在使用中则不删除反之不在使用中则删除，不检查则强制删除该角色。</param>
        /// <returns>是否已删除</returns>
        bool DeleteRole(IRole role, bool checkRoleInUse);

        /// <summary>
        /// 获取所有角色信息列表
        /// </summary>
        /// <returns></returns>
        IRole[] GetRoleList();

        /// <summary>
        /// 获取指定的角色信息
        /// </summary>
        /// <param name="role">角色基本信息</param>
        /// <returns>角色信息</returns>
        IRole GetRoleInfo(IRole role);

        /// <summary>
        /// 按照名称匹配角色信息
        /// </summary>
        /// <param name="nameForMatch">匹配字符</param>
        /// <returns>匹配到的角色信息列表</returns>
        IRole[] FindRole(string nameForMatch);

        /// <summary>
        /// 获取指定用户所属的所有角色列表
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>用户所属的所有角色列表</returns>
        IRole[] GetUserRoles(IUser user);

        /// <summary>
        /// 获取指定角色下所有用户
        /// </summary>
        /// <param name="role">角色信息</param>
        /// <returns>用户列表</returns>
        IUser[] GetUsersInRole(IRole role);

        /// <summary>
        /// 查找指定角色下匹配用户名的列表
        /// </summary>
        /// <param name="role">角色信息</param>
        /// <param name="usernameToMatch">匹配用户名</param>
        /// <returns>用户列表</returns>
        IUser[] FindUsersInRole(IRole role, string usernameToMatch);

        /// <summary>
        /// 获取所有不属于指定角色的用户
        /// </summary>
        /// <param name="role">角色信息</param>
        /// <returns>用户列表</returns>
        IUser[] GetUsersNotInRole(IRole role);

        /// <summary>
        /// 查找所有不属于指定角色下用户的匹配用户名的列表
        /// </summary>
        /// <param name="role">角色信息</param>
        /// <param name="usernameToMatch">匹配用户名</param>
        /// <returns>用户列表</returns>
        IUser[] FindUsersNotInRole(IRole role, string usernameToMatch);

        /// <summary>
        /// 添加用户到指定角色中
        /// </summary>
        /// <param name="users">用户列表</param>
        /// <param name="roles">角色列表</param>
        /// <returns>操作是否成功</returns>
        bool AddUsersToRoles(IUser[] users, params IRole[] roles);

        /// <summary>
        /// 删除用户所属角色
        /// </summary>
        /// <param name="users">用户列表</param>
        /// <param name="roles">角色列表</param>
        /// <returns>操作是否成功</returns>
        bool RemoveUsersFromRoles(IUser[] users, params IRole[] roles);
    }
}