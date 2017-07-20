using System;

namespace CSharpLib.Common.Cache
{
    /// <summary>
    /// 缓存过期策略--相对时间过期（指定缓存从现在开始的有效时间长度，超过该时间则过期）
    /// </summary>
    public class RelativeTimeExpiration : IExpiration
    {
        /// <summary>
        /// 缓存过期时间
        /// </summary>
        public DateTime ExpiredTime { get; private set; }

        /// <summary>
        /// 指示从现在开始超过指定的时间范围后过期的缓存过期策略
        /// </summary>
        /// <param name="expiredTimeSpan">距离现在的时间单位（只能大于当前时间）</param>
        public RelativeTimeExpiration(TimeSpan expiredTimeSpan)
        {
            this.ExpiredTime = DateTime.Now.Add(expiredTimeSpan);
        }
    }
}