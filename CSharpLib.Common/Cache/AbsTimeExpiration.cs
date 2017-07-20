using System;

namespace CSharpLib.Common.Cache
{
    /// <summary>
    /// 缓存过期策略--指定绝对时间过期（超过指定的时间后缓存过期）
    /// </summary>
    public class AbsTimeExpiration : IExpiration
    {
        /// <summary>
        /// 缓存过期时间
        /// </summary>
        public DateTime ExpiredTime { get; private set; }

        /// <summary>
        /// 指示按照绝对时间过期的缓存过期策略
        /// </summary>
        /// <param name="expiredTimeSpan">距离现在的时间单位（只能大于当前时间）</param>
        public AbsTimeExpiration(TimeSpan expiredTimeSpan)
            : this(DateTime.Now.Add(expiredTimeSpan))
        {
        }

        /// <summary>
        /// 指示按照绝对时间过期的缓存过期策略
        /// </summary>
        /// <param name="expiredTime">缓存过期的时间（只能大于当前时间）</param>
        public AbsTimeExpiration(DateTime expiredTime)
        {
            this.ExpiredTime = expiredTime;
        }
    }
}