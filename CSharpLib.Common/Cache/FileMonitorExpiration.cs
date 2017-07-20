using System;

namespace CSharpLib.Common.Cache
{
    /// <summary>
    /// 缓存过期策略--监控指定路径的文件，如果文件发生变化，则缓存过期
    /// </summary>
    public class FileMonitorExpiration : IExpiration
    {
        /// <summary>
        /// 监控的文件路径
        /// </summary>
        public string FileFullName { get; private set; }

        /// <summary>
        /// 指示监控文件变化为过期条件的缓存过期策略
        /// </summary>
        /// <param name="fileFullName">监控的文件路径（当前进程可有访问权限的文件全物理路径）</param>
        public FileMonitorExpiration(string fileFullName)
        {
            this.FileFullName = fileFullName;
        }
    }
}