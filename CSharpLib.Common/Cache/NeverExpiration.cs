using System;

namespace CSharpLib.Common.Cache
{
    /// <summary>
    /// 缓存过期策略--永不过期（永久缓存）
    /// </summary>
    public class NeverExpiration : IExpiration
    {
        /// <summary>
        /// 指示永不过期的缓存策略
        /// </summary>
        public NeverExpiration()
        {
        }
    }
}