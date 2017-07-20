using System;
using System.Collections.Generic;

namespace CSharpLib.Common
{
    /// <summary>
    /// 创建指定类实例辅助类
    /// </summary>
    public static class ProviderHelper
    {
        /// <summary>
        /// 数据驱动存放字典
        /// </summary>
        private static readonly Dictionary<string, object> _instancesDict = new Dictionary<string, object>();

        /// <summary>
        /// 创建指定驱动类实例，如果创建成功则进行缓存处理
        /// </summary>
        /// <typeparam name="T">驱动类类型</typeparam>
        /// <param name="typeName">驱动类型名称</param>
        /// <returns>驱动类实例</returns>
        public static T CreateInstance<T>(this string typeName) where T : class
        {
            if (!_instancesDict.ContainsKey(typeName))
            {
                lock (_instancesDict)
                {
                    if (!_instancesDict.ContainsKey(typeName))
                    {
                        Type type = Type.GetType(typeName);
                        if (type == null)
                            return default(T);
                        else
                        {
                            object obj = Activator.CreateInstance(type);
                            if (obj == null)
                                return default(T);
                            else
                                _instancesDict.Add(typeName, obj);
                        }
                    }
                }
            }
            return _instancesDict[typeName] as T;
        }

        /// <summary>
        /// 移除指定的驱动类实例缓存
        /// </summary>
        /// <param name="typeName"></param>
        public static void Remove(string typeName)
        {
            if (_instancesDict.ContainsKey(typeName))
            {
                lock (_instancesDict)
                {
                    _instancesDict.Remove(typeName);
                }
            }
        }

        /// <summary>
        /// 重置所有驱动类实例缓存
        /// </summary>
        public static void Clear()
        {
            _instancesDict.Clear();
        }


        /// <summary>
        /// 创建指定驱动类实例，不做缓存处理
        /// </summary>
        /// <typeparam name="T">驱动类类型</typeparam>
        /// <param name="typeName">驱动类型名称</param>
        /// <returns>驱动类实例</returns>
        public static T CreateInstanceOnce<T>(this string typeName) where T : class
        {
            Type type = Type.GetType(typeName);
            if (type == null)
                return default(T);
            else
            {
                object obj = Activator.CreateInstance(type);
                if (obj == null)
                    return default(T);
                return (T)obj;
            }
        }
    }
}