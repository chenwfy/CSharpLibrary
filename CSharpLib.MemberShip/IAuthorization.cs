using System;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 身份授权管理接口
    /// </summary>
    public interface IAuthorization
    {
        /// <summary>
        /// 验证用户登录
        /// </summary>
        /// <param name="user">用户登录的基本信息</param>
        /// <returns>登录是否成功</returns>
        int ValidateUser(IUser user);

        /// <summary>
        /// 设置用户身份状态
        /// </summary>
        /// <param name="user">用户基本信息，包含用户名和用户ID。</param>
        /// <param name="userData">用户自定义数据</param>
        /// <param name="persistentDay">身份持续有效天数</param>
        void SetAuthCookie(IUser user, string userData, double persistentDay);

        /// <summary>
        /// 注销用户身份信息
        /// </summary>
        void SignOut();

        /// <summary>
        /// 获取当前登录用户基本信息
        /// </summary>
        IUser Current { get; }

        /// <summary>
        /// 验证用户请求
        /// </summary>
        void AuthenticateRequest();
    }
}