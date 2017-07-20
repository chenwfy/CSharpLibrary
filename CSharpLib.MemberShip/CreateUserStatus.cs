using System;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 创建用户结果状态枚举
    /// </summary>
    public enum CreateUserStatus
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
        /// 用户名无效
        /// </summary>
        InvalidUserName,

        /// <summary>
        /// 密码无效
        /// </summary>
        InvalidPassword,

        /// <summary>
        /// 提示问题无效
        /// </summary>
        InvalidQuestion,

        /// <summary>
        /// 提示问题答案无效
        /// </summary>
        InvalidAnswer,

        /// <summary>
        /// 电子邮箱无效
        /// </summary>
        InvalidEmail,

        /// <summary>
        /// 用户名已存在
        /// </summary>
        DuplicateUserName,

        /// <summary>
        /// 电子邮箱已存在
        /// </summary>
        DuplicateEmail,

        /// <summary>
        /// 用户被拒绝
        /// </summary>
        UserRejected
    }
}