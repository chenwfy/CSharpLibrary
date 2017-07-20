using System;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 用户角色成员接口
    /// </summary>
    public interface IRole
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        int RoleId { get; set; }
        
        /// <summary>
        /// 角色名称
        /// </summary>
        string RoleName { get; set; }
    }
}