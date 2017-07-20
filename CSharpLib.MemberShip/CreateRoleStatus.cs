using System;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 创建角色结果状态枚举
    /// </summary>
    public enum CreateRoleStatus
    {
        /// <summary>
        /// 处理出错
        /// </summary>
        Error = 0,

        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 角色名无效
        /// </summary>
        InvalidRoleName,

        /// <summary>
        /// 角色已存在
        /// </summary>
        DuplicateRoleName
    }
}