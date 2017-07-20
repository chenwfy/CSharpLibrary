using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace CSharpLib.Common.Cache
{
    /// <summary>
    /// 简易缓存（内存缓存，不可跨进程）帮助类
    /// </summary>
    public static class CacheHelper
    {
        #region 私有成员

        /// <summary>
        /// 存放缓存的字典
        /// </summary>
        private static readonly IDictionary<string, IDictionary<string, CacheInfo>> cacheDictionary;

        /// <summary>
        /// 缓存日志记录器名称
        /// </summary>
        private static readonly string cacheLogName = "CacheLog";

        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <returns>当前时间</returns>
        private static string GetNow()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// 缓存日志记录
        /// </summary>
        /// <param name="log">日志消息</param>
        private static void Log(string log)
        {
            log.Log(cacheLogName);
        }

        /// <summary>
        /// 获取指定名称（可理解为缓存分类，每个类型下有N个键值对缓存项）的缓存字典
        /// </summary>
        /// <param name="cacheName">缓存名称</param>
        /// <returns>名称对应的缓存字典</returns>
        private static IDictionary<string, CacheInfo> GetCacheDictionary(string cacheName)
        {
            return cacheDictionary.ContainsKey(cacheName) ? cacheDictionary[cacheName] : null;
        }

        #endregion

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static CacheHelper()
        {
            cacheDictionary = new Dictionary<string, IDictionary<string, CacheInfo>>(0);
            ClearTask();
        }

        #region 读取缓存

        /// <summary>
        /// 判断是否存在缓存键
        /// </summary>
        /// <param name="cacheName">缓存名称（可理解为缓存分类，每个类型下有N个键值对缓存项）</param>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>是否存在缓存键</returns>
        public static bool Exists(this string cacheName, string cacheKey)
        {
            IDictionary<string, CacheInfo> cacheDict = GetCacheDictionary(cacheName);
            return null != cacheDict && cacheDict.ContainsKey(cacheKey) && cacheDict[cacheKey].ExpiredTime >= DateTime.Now;
        }

        /// <summary>
        /// 读取缓存，如果读取失败，则返回NULL
        /// </summary>
        /// <param name="cacheName">缓存名称（可理解为缓存分类，每个类型下有N个键值对缓存项）</param>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>缓存值</returns>
        public static object GetCache(this string cacheName, string cacheKey)
        {
            IDictionary<string, CacheInfo> cacheDict = GetCacheDictionary(cacheName);
            if (null != cacheDict && cacheDict.ContainsKey(cacheKey))
            {
                CacheInfo cacheData = cacheDict[cacheKey];
                if (cacheData.ExpiredTime >= DateTime.Now)
                {
                    return cacheData.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 读取缓存，如果读取失败，则返回缓存值类型默认值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="cacheName">缓存名称（可理解为缓存分类，每个类型下有N个键值对缓存项）</param>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>缓存值</returns>
        public static T GetCache<T>(this string cacheName, string cacheKey)
        {
            object cacheValue = cacheName.GetCache(cacheKey);
            return null != cacheValue ? (T)cacheValue : default(T);
        }

        /// <summary>
        /// 获取指定缓存名称下缓存项的总数
        /// </summary>
        /// <param name="cacheName">缓存名称（可理解为缓存分类，每个类型下有N个键值对缓存项）</param>
        /// <returns>缓存名称下缓存项的总数</returns>
        public static int GetCacheCount(this string cacheName)
        {
            IDictionary<string, CacheInfo> cacheDict = GetCacheDictionary(cacheName);
            return null != cacheDict ? cacheDict.Keys.Count : 0;
        }

        #endregion

        #region 移除缓存

        /// <summary>
        /// 移除缓存（移除缓存名称项）
        /// </summary>
        /// <param name="cacheName">缓存名称（可理解为缓存分类，每个类型下有N个键值对缓存项）</param>
        /// <returns>移除结果</returns>
        public static void RemoveCache(this string cacheName)
        {
            if (cacheDictionary.ContainsKey(cacheName))
            {
                lock (cacheDictionary)
                {
                    if (cacheDictionary.ContainsKey(cacheName))
                    {
                        cacheDictionary.Remove(cacheName);
                    }
                }
            }
        }

        /// <summary>
        /// 移除缓存（移除指定缓存键的缓存项）
        /// </summary>
        /// <param name="cacheName">缓存名称（可理解为缓存分类，每个类型下有N个键值对缓存项）</param>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>移除结果</returns>
        public static void RemoveCache(this string cacheName, string cacheKey)
        {
            IDictionary<string, CacheInfo> cacheDict = GetCacheDictionary(cacheName);
            if (null != cacheDict && cacheDict.ContainsKey(cacheKey))
            {
                lock (cacheDictionary)
                {
                    cacheDict = GetCacheDictionary(cacheName);
                    if (null != cacheDict && cacheDict.ContainsKey(cacheKey))
                    {
                        cacheDict.Remove(cacheKey);
                    }
                }
            }
        }

        /// <summary>
        /// 清除缓存（缓存名称项下的所有缓存项）
        /// </summary>
        /// <param name="cacheName">缓存名称（可理解为缓存分类，每个类型下有N个键值对缓存项）</param>
        public static void Clear(this string cacheName)
        {
            if (cacheDictionary.ContainsKey(cacheName))
            {
                lock (cacheDictionary)
                {
                    if (cacheDictionary.ContainsKey(cacheName))
                    {
                        cacheDictionary[cacheName].Clear();
                    }
                }
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public static void Clear()
        {
            cacheDictionary.Clear();
        }

        #endregion

        #region 写入缓存

        /// <summary>
        /// 写入缓存，默认缓存过期时间为20分钟
        /// </summary>
        /// <param name="cacheName">缓存名称（可理解为缓存分类，每个类型下有N个键值对缓存项）</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheValue">缓存值</param>
        public static void SetCache(this string cacheName, string cacheKey, object cacheValue)
        {
            cacheName.SetCache(cacheKey, cacheValue, new RelativeTimeExpiration(new TimeSpan(0, 20, 0)));
        }

        /// <summary>
        /// 写入缓存
        /// </summary>
        /// <param name="cacheName">缓存名称（可理解为缓存分类，每个类型下有N个键值对缓存项）</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheValue">缓存值</param>
        /// <param name="expiration">缓存过期策略（NeverExpiration、RelativeTimeExpiration、AbsTimeExpiration、FileMonitorExpiration类实现该接口）</param>
        public static void SetCache(this string cacheName, string cacheKey, object cacheValue, IExpiration expiration)
        {
            CacheInfo cacheInfo = new CacheInfo { Value = cacheValue, ExpiredTime = DateTime.MaxValue };
            if (expiration is RelativeTimeExpiration)
                cacheInfo.ExpiredTime = ((RelativeTimeExpiration)expiration).ExpiredTime;
            else if (expiration is AbsTimeExpiration)
                cacheInfo.ExpiredTime = ((AbsTimeExpiration)expiration).ExpiredTime;

            if (!cacheDictionary.ContainsKey(cacheName))
            {
                lock (cacheDictionary)
                {
                    if (!cacheDictionary.ContainsKey(cacheName))
                        cacheDictionary.Add(cacheName, new Dictionary<string, CacheInfo>() { { cacheKey, cacheInfo } });
                    else
                    {
                        if (cacheDictionary[cacheName].ContainsKey(cacheKey))
                            cacheDictionary[cacheName][cacheKey] = cacheInfo;
                        else
                            cacheDictionary[cacheName].Add(cacheKey, cacheInfo);
                    }
                }
            }
            else
            {
                lock (cacheDictionary)
                {
                    if (cacheDictionary[cacheName].ContainsKey(cacheKey))
                        cacheDictionary[cacheName][cacheKey] = cacheInfo;
                    else
                        cacheDictionary[cacheName].Add(cacheKey, cacheInfo);
                }
            }

            if (expiration is FileMonitorExpiration)
                FileDependencyForCache(cacheName, cacheKey, ((FileMonitorExpiration)expiration).FileFullName);
        }

        /// <summary>
        /// 绑定根据文件变化决定过期策略的缓存项进行事件绑定
        /// </summary>
        /// <param name="cacheName">缓存名称</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="targetFile">待监控的文件</param>
        private static void FileDependencyForCache(string cacheName, string cacheKey, string targetFile)
        {
            if (File.Exists(targetFile))
            {
                FileSystemWatcher fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(targetFile), Path.GetFileName(targetFile));
                fileWatcher.IncludeSubdirectories = false;
                fileWatcher.EnableRaisingEvents = true;
                fileWatcher.Changed += new FileSystemEventHandler((sender, e) =>
                {
                    if (e.FullPath.FileSaveCompleted())
                    {
                        cacheName.RemoveCache(cacheKey);
                    }
                });
                fileWatcher.Deleted += new FileSystemEventHandler((sender, e) =>
                {
                    cacheName.RemoveCache(cacheKey);
                });
            }
        }

        #endregion

        #region 定时清理

        /// <summary>
        /// 清理任务
        /// </summary>
        private static void ClearTask()
        {
            Thread thread = new Thread(new ThreadStart(TimerClear));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 清理
        /// </summary>
        private static void TimerClear()
        {
            IList<string> keyList = new List<string>(0);
            int timeSpan = 20100;
            while (true)
            {
                Thread.Sleep(timeSpan);
                if (cacheDictionary.Keys.Count > 0)
                {
                    lock (cacheDictionary)
                    {
                        foreach (var cacheName in cacheDictionary.Keys)
                        {
                            keyList = new List<string>(0);
                            if (cacheDictionary[cacheName] != null && cacheDictionary[cacheName].Keys.Count > 0)
                            {
                                foreach (var cacheKey in cacheDictionary[cacheName].Keys)
                                {
                                    if (cacheDictionary[cacheName][cacheKey].ExpiredTime < DateTime.Now)
                                    {
                                        keyList.Add(cacheKey);
                                    }
                                }

                                if (keyList.Count > 0)
                                {
                                    foreach (var item in keyList)
                                    {
                                        cacheName.RemoveCache(item);
                                    }

                                    keyList.Clear();
                                }
                            }
                        }
                    }
                }

                timeSpan = 20000;
            }
        }

        #endregion
    }
}