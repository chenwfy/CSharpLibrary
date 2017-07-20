using System;

namespace CSharpLib.Common.Cache
{
    /// <summary>
    /// 缓存信息类
    /// </summary>
    public class CacheInfo
    {
        /// <summary>
        /// 缓存值
        /// </summary>
        public object Value { get; internal set; }

        /// <summary>
        /// 缓存过期时间
        /// </summary>
        public DateTime ExpiredTime { get; internal set; }
    }
}