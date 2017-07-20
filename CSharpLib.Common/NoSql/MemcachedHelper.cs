using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CSharpLib.Common.Cache;
using CSharpLib.Common.NoSql.Memcached;

namespace CSharpLib.Common.NoSql
{
    /// <summary>
    /// Memcached辅助类
    /// </summary>
    public static class MemcachedHelper
    {
        /// <summary>
        /// 
        /// </summary>
        private static MemcachedClient mClient;

        /// <summary>
        /// 
        /// </summary>
        static MemcachedHelper()
        {
            string servers = "Memcached.Servers".GetAppSetting();
            string[] serverList = new string[] { servers };
            if (servers.Contains(","))
                serverList = servers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            SockIOPool pool = SockIOPool.GetInstance();
            pool.SetServers(serverList);
            pool.InitConnections = serverList.Length;
            pool.MinConnections = serverList.Length;
            pool.MaxConnections = 100;
            pool.SocketConnectTimeout = 1000;
            pool.SocketTimeout = 3000;
            pool.MaintenanceSleep = 30;
            pool.Failover = true;
            pool.Nagle = false;
            pool.Initialize();

            mClient = new MemcachedClient();
            mClient.EnableCompression = false;
        }

        /// <summary>
        /// 存入Memcached
        /// </summary>
        /// <param name="cacheKey">缓存键值</param>
        /// <param name="cacheValue">缓存值</param>
        public static void MemcachedSet(this string cacheKey, object cacheValue)
        {
            cacheKey.MemcachedSet(cacheValue, new RelativeTimeExpiration(new TimeSpan(0, 20, 0)));
        }

        /// <summary>
        /// 存入Memcached
        /// </summary>
        /// <param name="cacheKey">缓存键值</param>
        /// <param name="cacheValue">缓存值</param>
        /// <param name="expiration">缓存过期策略（FileMonitorExpiration模式不适用）</param>
        public static void MemcachedSet(this string cacheKey, object cacheValue, IExpiration expiration)
        {
            DateTime expiredTime = DateTime.MaxValue;
            if (expiration is RelativeTimeExpiration)
                expiredTime = ((RelativeTimeExpiration)expiration).ExpiredTime;
            else if (expiration is AbsTimeExpiration)
                expiredTime = ((AbsTimeExpiration)expiration).ExpiredTime;

            if (mClient.KeyExists(cacheKey))
                mClient.Delete(cacheKey);

            mClient.Add(cacheKey, cacheValue, expiredTime);
        }

        /// <summary>
        /// 读取Memcached缓存
        /// </summary>
        /// <param name="cacheKeys">缓存键值</param>
        /// <returns>以Hashtable返回多个键值对应的缓存值</returns>
        public static Hashtable MemcachedRead(this IEnumerable<string> cacheKeys)
        {
            return mClient.GetMultiple(cacheKeys.ToArray());
        }

        /// <summary>
        /// 读取Memcached缓存
        /// </summary>
        /// <param name="cacheKey">缓存键值</param>
        /// <returns>键值对应的缓存值</returns>
        public static object MemcachedRead(this string cacheKey)
        {
            return mClient.Get(cacheKey);
        }

        /// <summary>
        /// 读取Memcached缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="cacheKey">缓存键值</param>
        /// <returns>键值对应的缓存值</returns>
        public static T MemcachedRead<T>(this string cacheKey)
        {
            object value = cacheKey.MemcachedRead();
            if (null == value)
                return default(T);
            return (T)value;
        }

        /// <summary>
        /// 移除Memcached缓存
        /// </summary>
        /// <param name="cacheKey">待移除的缓存键值</param>
        public static void MemcachedRemove(this string cacheKey)
        {
            mClient.Delete(cacheKey);
        }
    }
}