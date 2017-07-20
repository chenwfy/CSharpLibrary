using System;

namespace CSharpLib.MemberShip
{
    /// <summary>
    /// 成员管理通用接口
    /// </summary>
    public interface IMembership : IUsership, IRoleship, IAuthorization
    {
    }
}