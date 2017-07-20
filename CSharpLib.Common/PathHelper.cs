using System;
using System.IO;
using System.Web;

namespace CSharpLib.Common
{
    /// <summary>
    /// 文件路径帮助类
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// 返回文件虚拟路径所对应的物理路径
        /// 适用于WEBFORM项目
        /// </summary>
        /// <param name="virtualPath">文件虚拟路径</param>
        /// <returns>文件物理路径</returns>
        public static string MapPath(this string virtualPath)
        {
            return virtualPath.MapPath(HttpContext.Current);
        }

        /// <summary>
        /// 返回文件虚拟路径所对应的物理路径
        /// 适用于WEBFORM项目
        /// </summary>
        /// <param name="virtualPath">文件虚拟路径</param>
        /// <param name="context">当前HTTP上下文应答对象</param>
        /// <returns>文件物理路径</returns>
        public static string MapPath(this string virtualPath, HttpContext context)
        {
            if (context != null)
                return context.Server.MapPath(virtualPath);
            return System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);
        }

        /// <summary>
        /// 获取应用所在目录
        /// </summary>
        /// <returns>应用所在目录</returns>
        public static string AppBaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 获取当前应用进程目录
        /// </summary>
        /// <returns>当前应用进程目录</returns>
        public static string CurrentProcessDirectory()
        {
            return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        }
    }
}