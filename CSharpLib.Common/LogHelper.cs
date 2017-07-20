using System;
using System.Text;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using System.Configuration;
using System.IO;
using log4net;

namespace CSharpLib.Common
{
    /// <summary>
    /// 日志消息级别
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 调试类型
        /// </summary>
        Debug,

        /// <summary>
        /// 信息类型
        /// </summary>
        Info,

        /// <summary>
        /// 警告类型
        /// </summary>
        Warn,

        /// <summary>
        /// 错误类型
        /// </summary>
        Error,

        /// <summary>
        /// 致命类型
        /// </summary>
        Fatal
    }

    /// <summary>
    ///日志记录帮助类
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 日志配置文件路径
        /// </summary>
        private static readonly string ConfigPath;

        #region 私有方法

        private static void DebugObject(this ILog debugLog, string msg, object obj, int maxDepth)
        {
            if (debugLog != null && debugLog.IsDebugEnabled)
            {
                StringBuilder sb = new StringBuilder(msg);
                sb.Append(Environment.NewLine);
                sb.Append("├→");
                DebugProperties(obj, 1, (maxDepth < 1 ? 1 : maxDepth), sb);
                debugLog.Debug(sb);
            }
        }

        private static void DebugProperties(object obj, int depth, int maxDepth, StringBuilder sb)
        {
            if (depth > maxDepth)
            {
                sb.Append("Out of max depth∶");
                sb.Append(maxDepth);
                return;
            }
            if (obj == null)
            {
                sb.Append("Object is null");
                return;
            }
            Type type = obj.GetType();
            if (obj is NameValueCollection)
            {
                sb.Append(type.ToString());
                AppendNameValueCollectionString(depth + 1, sb, obj as NameValueCollection);
                return;
            }
            else if (obj is IConvertible)
            {
                sb.Append(type.ToString());
                sb.Append("∶");
                sb.Append(obj.ToString());
                return;
            }
            sb.Append(type.ToString());
            PropertyInfo[] properties = type.GetProperties();
            object o = null;
            foreach (PropertyInfo propInfo in properties)
            {
                sb.Append(Environment.NewLine);
                AppendDepthString(depth, sb);
                sb.Append(propInfo.Name);
                sb.Append("（");
                sb.Append(propInfo.PropertyType.ToString());
                sb.Append("）");
                try
                {
                    o = propInfo.GetValue(obj, null);
                }
                catch (TargetParameterCountException)
                {
                    try
                    {
                        int count = Convert.ToInt32(type.GetProperty("Count").GetValue(obj, null));
                        sb.Append("∶Count=");
                        sb.Append(count);
                        for (int i = 0; i < count; i++)
                        {
                            o = propInfo.GetValue(obj, new object[] { i });
                            sb.Append(Environment.NewLine);
                            AppendDepthString(depth + 1, sb);
                            DebugProperties(o, depth + 2, maxDepth, sb);
                        }
                    }
                    catch (Exception)
                    {

                    }
                    continue;
                }
                catch (NotImplementedException)
                {
                    sb.Append("∶NotImplementedException");
                    continue;
                }
                catch (NotSupportedException)
                {
                    sb.Append("∶NotSupportedException");
                    continue;
                }
                catch (Exception ex)
                {
                    sb.Append("∶");
                    sb.Append(ex.Message);
                    continue;
                }
                if (o is NameValueCollection)
                {
                    AppendNameValueCollectionString(depth + 1, sb, o as NameValueCollection);
                }
                else if (o is ICollection)
                {
                    AppendCollectionString(depth + 1, maxDepth, sb, o as ICollection);
                }
                else if (o is IConvertible)
                {
                    sb.Append("∶");
                    sb.Append(o.ToString());
                }
                else
                {
                    sb.Append(Environment.NewLine);
                    AppendDepthString(depth + 1, sb);
                    DebugProperties(o, depth + 2, maxDepth, sb);
                }
            }
        }

        private static void AppendCollectionString(int depth, int maxDepth, StringBuilder sb, ICollection collection)
        {
            sb.Append("∶Count = ");
            sb.Append(collection.Count);

            if (collection is byte[])
            {
                byte[] buffer = collection as byte[];
                if (buffer.Length > 0)
                {
                    sb.Append(Environment.NewLine);
                    AppendDepthString(depth, sb);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        sb.Append(buffer[i]);
                        sb.Append(" ");
                    }
                }
            }
            else
            {
                IEnumerator en = collection.GetEnumerator();
                while (en.MoveNext())
                {
                    sb.Append(Environment.NewLine);
                    AppendDepthString(depth, sb);
                    object obj = en.Current;
                    if (obj is DictionaryEntry)
                    {
                        DictionaryEntry de = (DictionaryEntry)obj;
                        sb.Append("Key∶");
                        sb.Append(de.Key.ToString());
                        sb.Append("\t\t");
                        sb.Append("Value∶");
                        if (de.Value != null)
                        {
                            sb.Append(de.Value.ToString());
                        }
                    }
                    else if (obj is IConvertible)
                    {
                        sb.Append(obj.ToString());
                    }
                    else
                        DebugProperties(obj, depth + 1, maxDepth, sb);
                }
            }
        }

        private static void AppendNameValueCollectionString(int depth, StringBuilder sb, NameValueCollection collection)
        {
            sb.Append("∶Count = ");
            sb.Append(collection.Count);
            for (int i = 0; i < collection.Count; i++)
            {
                sb.Append(Environment.NewLine);
                AppendDepthString(depth, sb);
                sb.Append(collection.GetKey(i));
                sb.Append("=");
                sb.Append(collection[i]);
            }
        }

        private static void AppendDepthString(int depth, StringBuilder sb)
        {
            for (int i = 0; i < depth; i++)
            {
                sb.Append("　　");
            }
            sb.Append("├→");
        }

        private static string GetRequestInfo()
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Request == null)
                return Environment.NewLine;
            else
                return string.Format("{0}Path And Query: {1}{0}Request Type: {2}{0}Http Referrer: {3}{0}User Agent: {4}{0}IP Address: {5}{0}",
                    Environment.NewLine,
                    context.Request.Url.PathAndQuery,
                    context.Request.RequestType,
                    context.Request.UrlReferrer == null ? string.Empty : context.Request.UrlReferrer.PathAndQuery,
                    context.Request.UserAgent,
                    context.Request.UserHostAddress);
        }

        private static ILog GetLogger(string repository, string name)
        {
            if (string.IsNullOrEmpty(repository))
                return LogManager.Exists(name);
            else
            {
                if (log4net.Core.LoggerManager.RepositorySelector.ExistsRepository(repository))
                    return LogManager.Exists(repository, name);
                else
                    return null;
            }
        }

        private static ILog CreateLogger(this ILog logger, string name, string filename, string repository)
        {
            ILog newLogger = GetLogger(repository, name);
            if (newLogger == null)
            {
                lock (ConfigPath)
                {
                    newLogger = GetLogger(repository, name);
                    if (newLogger == null)
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(ConfigPath);
                        XmlNodeList nodeList = xml.SelectNodes("/log4net/*[@name='" + logger.Logger.Name + "']");
                        if (nodeList.Count == 2)
                        {
                            xml.DocumentElement.InnerXml = string.Empty;
                            foreach (XmlNode node in nodeList)
                            {
                                if (node.Name == "appender")
                                {
                                    node.Attributes["name"].Value = name;
                                    node["file"].Attributes["value"].Value = filename;
                                }
                                else if (node.Name == "logger")
                                {
                                    node.Attributes["name"].Value = name;
                                    node["appender-ref"].Attributes["ref"].Value = name;
                                }
                                xml.DocumentElement.AppendChild(node);
                            }
                            if (string.IsNullOrEmpty(repository))
                                log4net.Config.XmlConfigurator.Configure(xml.DocumentElement);
                            else
                            {
                                log4net.Repository.ILoggerRepository rep;
                                if (log4net.Core.LoggerManager.RepositorySelector.ExistsRepository(repository))
                                    rep = LogManager.GetRepository(repository);
                                else
                                    rep = LogManager.CreateRepository(repository);
                                log4net.Config.XmlConfigurator.Configure(rep, xml.DocumentElement);
                            }
                            newLogger = GetLogger(repository, name);
                        }
                        else
                            throw new XmlException("Create new logger failed due to XPath \"/log4net/*[@name='" + logger.Logger.Name + "'] returns invalid records from " + ConfigPath);
                    }
                }
            }
            return newLogger;
        }

        #endregion

        static LogHelper()
        {
            string cfgFile = "log4net".GetAppSetting();
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                if (string.IsNullOrEmpty(cfgFile))
                    cfgFile = "log4net.config";
                cfgFile = Path.Combine(PathHelper.AppBaseDirectory(), cfgFile);
            }
            else
            {
                if (string.IsNullOrEmpty(cfgFile))
                    cfgFile = "~/log4net.config";
                cfgFile = cfgFile.MapPath();
            }
            if (File.Exists(cfgFile))
            {
                ConfigPath = cfgFile;
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(ConfigPath));
            }
        }

        /// <summary>
        /// 配置日志记录器
        /// </summary>
        [Obsolete("该方法已过时，相关逻辑已移至静态构造函数中，此处保留以兼容以前程序的调用。")]
        public static void Configure()
        {

        }

        /// <summary>
        /// 记录普通消息到日志（INFO）
        /// </summary>
        /// <param name="msg">消息内容</param>
        public static void Info(this string msg)
        {
            ILog infoLog = LogManager.Exists("InfoLog");
            if (infoLog != null && infoLog.IsInfoEnabled)
                infoLog.Info(msg);
        }

        /// <summary>
        /// 记录普通消息到日志（INFO）
        /// </summary>
        /// <param name="format">带格式的消息内容</param>
        /// <param name="args">参数列表</param>
        public static void Info(this string format, params object[] args)
        {
            ILog infoLog = LogManager.Exists("InfoLog");
            if (infoLog != null && infoLog.IsInfoEnabled)
                infoLog.InfoFormat(format, args);
        }

        /// <summary>
        /// 记录异常消息到日志（ERROR）
        /// </summary>
        /// <param name="ex">异常消息</param>
        public static void Error(this Exception ex)
        {
            ILog errLog = LogManager.Exists("ErrorLog");
            if (errLog != null && errLog.IsErrorEnabled)
                errLog.Error(ex.Message + GetRequestInfo() + ex.ToString());
        }

        /// <summary>
        /// 记录对象调试消息到日志（DEBUG）
        /// </summary>
        /// <param name="obj">要调试的对象</param>
        public static void Debug(this object obj)
        {
            obj.Debug("Object Debug Stack");
        }

        /// <summary>
        /// 记录对象调试消息到日志（DEBUG）
        /// </summary>
        /// <param name="obj">要调试的对象，嵌套最大深度为7层。</param>
        /// <param name="msg">调试信息头部自定义消息内容</param>
        public static void Debug(this object obj, string msg)
        {
            obj.Debug(msg, 7);
        }

        /// <summary>
        /// 记录对象调试消息到日志（DEBUG）
        /// </summary>
        /// <param name="obj">要调试的对象</param>
        /// <param name="msg">调试信息头部自定义消息内容</param>
        /// <param name="maxDepth">调试对象嵌套最大深度（从第1层开始)</param>
        public static void Debug(this object obj, string msg, int maxDepth)
        {
            LogManager.Exists("DebugLog").DebugObject(msg, obj, maxDepth);
        }

        /// <summary>
        /// 记录普通消息到指定的日志记录器（INFO）
        /// </summary>
        /// <param name="msg">要记录的日志信息</param>
        /// <param name="name">日志记录器名称</param>
        public static void Log(this string msg, string name)
        {
            ILog logger = LogManager.Exists(name);
            if (logger != null && logger.IsInfoEnabled)
            {
                logger.Info(msg);
            }
        }

        /// <summary>
        /// 记录普通消息到指定的日志记录器（INFO）
        /// </summary>
        /// <param name="msg">要记录的日志信息</param>
        /// <param name="name">日志记录器名称</param>
        /// <param name="args">日志信息参数列表</param>
        public static void Log(this string msg, string name, params object[] args)
        {
            ILog logger = LogManager.Exists(name);
            if (logger != null && logger.IsInfoEnabled)
            {
                logger.InfoFormat(msg, args);
            }
        }

        /// <summary>
        /// 记录特定等级的日志消息到指定的日志记录器
        /// </summary>
        /// <param name="level">日志消息所属等级</param>
        /// <param name="name">日志记录器名称</param>
        /// <param name="obj">日志消息对象</param>
        /// <param name="args">日志消息相关参数</param>
        public static void Log(this LogLevel level, string name, object obj, params object[] args)
        {
            Log(level, string.Empty, name, obj, args);
        }

        /// <summary>
        /// 记录特定等级的日志消息到指定的日志记录器
        /// </summary>
        /// <param name="level">日志消息所属等级</param>
        /// <param name="repository">日志记录器所属容器名称</param>
        /// <param name="name">日志记录器名称</param>
        /// <param name="obj">日志消息对象</param>
        /// <param name="args">日志消息相关参数</param>
        public static void Log(this LogLevel level, string repository, string name, object obj, params object[] args)
        {
            ILog logger = GetLogger(repository, name);
            if (logger != null)
            {
                #region 按等级记录日志

                if (level == LogLevel.Debug)
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.DebugObject("Object Debug Stack", obj, 7);
                        if (args != null && args.Length > 0)
                        {
                            foreach (object item in args)
                            {
                                logger.DebugObject("Object Debug Stack", item, 7);
                            }
                        }
                    }
                }
                else if (level == LogLevel.Info)
                {
                    if (logger.IsInfoEnabled)
                    {
                        if (obj is string)
                        {
                            if (args != null && args.Length > 0)
                                logger.InfoFormat(obj as string, args);
                            else
                                logger.Info(obj as string);
                        }
                        else
                        {
                            logger.Info(obj);
                            if (args != null && args.Length > 0)
                            {
                                foreach (object item in args)
                                {
                                    logger.Info(item);
                                }
                            }
                        }
                    }
                }
                else if (level == LogLevel.Warn)
                {
                    if (logger.IsWarnEnabled)
                    {
                        logger.Warn(obj);
                        if (args != null && args.Length > 0)
                        {
                            foreach (object item in args)
                            {
                                if (item is Exception)
                                    logger.Warn("Exception occured", item as Exception);
                                else
                                    logger.Warn(item);
                            }
                        }
                    }
                }
                else if (level == LogLevel.Error)
                {
                    if (logger.IsErrorEnabled)
                    {
                        logger.Error(obj);
                        if (args != null && args.Length > 0)
                        {
                            foreach (object item in args)
                            {
                                if (item is Exception)
                                    logger.Error("Exception occured", item as Exception);
                                else
                                    logger.Error(item);
                            }
                        }
                    }
                }
                else if (level == LogLevel.Fatal)
                {
                    if (logger.IsFatalEnabled)
                    {
                        logger.Fatal(obj);
                        if (args != null && args.Length > 0)
                        {
                            foreach (object item in args)
                            {
                                if (item is Exception)
                                    logger.Fatal("Exception occured", item as Exception);
                                else
                                    logger.Fatal(item);
                            }
                        }
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// 创建新日志记录器
        /// </summary>
        /// <param name="existName">系统已存在的日志记录器名称，用作创建新日志记录器的模板。</param>
        /// <param name="newName">新创建的日志记录器名称</param>
        /// <param name="newFile">新创建日志记录器绑定的写入文件路径</param>
        /// <returns>创建是否成功</returns>
        /// <remarks>创建成功后存在于默认容器中且不会被删除</remarks>
        public static bool CreateLogger(this string existName, string newName, string newFile)
        {
            return CreateLogger(string.Empty, existName, newName, newFile);
        }

        /// <summary>
        /// 创建新日志记录器
        /// </summary>
        /// <param name="repositoryName">新日志记录器所属容器名称</param>
        /// <param name="existName">系统已存在的日志记录器名称，用作创建新日志记录器的模板。</param>
        /// <param name="newName">新创建的日志记录器名称</param>
        /// <param name="newFile">新创建日志记录器绑定的写入文件路径</param>
        /// <returns>创建是否成功</returns>
        /// <remarks>创建成功后存在于指定容器中且可以被删除</remarks>
        public static bool CreateLogger(this string repositoryName, string existName, string newName, string newFile)
        {
            ILog logger = LogManager.Exists(existName);
            if (logger == null)
                return false;
            else
                return logger.CreateLogger(newName, newFile, repositoryName) == null ? false : true;
        }

        /// <summary>
        /// 删除指定的日志记录器
        /// </summary>
        /// <param name="repository">日志记录器所属容器名称</param>
        /// <param name="names">日志记录器名称数组，为空则删除指定容器。</param>
        public static void RemoveLogger(this string repository, params string[] names)
        {
            if (log4net.Core.LoggerManager.RepositorySelector.ExistsRepository(repository))
            {
                lock (ConfigPath)
                {
                    if (log4net.Core.LoggerManager.RepositorySelector.ExistsRepository(repository))
                    {
                        log4net.Repository.ILoggerRepository rep = LogManager.GetRepository(repository);
                        if (names == null || names.Length == 0)
                            (rep as log4net.Repository.Hierarchy.Hierarchy).Clear();
                        else
                        {
                            foreach (string name in names)
                            {
                                ILog logger = LogManager.Exists(repository, name);
                                if (logger != null)
                                    (logger.Logger as log4net.Core.IAppenderAttachable).RemoveAllAppenders();
                            }
                            bool flag = true;
                            foreach (ILog item in LogManager.GetCurrentLoggers(repository))
                            {
                                if ((item.Logger as log4net.Core.IAppenderAttachable).Appenders.Count > 0)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                                (rep as log4net.Repository.Hierarchy.Hierarchy).Clear();
                        }
                    }
                }
            }
        }
    }
}