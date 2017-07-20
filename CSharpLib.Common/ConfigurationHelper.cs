using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Configuration;

namespace CSharpLib.Common
{
    /// <summary>
    /// 配置文件信息帮助类
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// 获取web.config中指定节点的appSettings值
        /// </summary>
        /// <param name="key">配置节点KEY</param>
        /// <returns>web.config中的指定节点的appSettings值</returns>
        public static string GetAppSetting(this string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 从指定路径的XML文件中读取指定节点的appSettings值。要求该XML文件结构和WEB.CONFIG(APP.CONFIG)结构一致
        /// </summary>
        /// <param name="key">配置节点KEY</param>
        /// <param name="xmlFilePath">XML文件路径</param>
        /// <returns>XML文件中的appSettings节点值</returns>
        public static string GetAppSetting(this string key, string xmlFilePath)
        {
            XmlDocument xmlDoc = xmlFilePath.LoadToXml();
            XmlNodeList settingsNodes = xmlDoc["Config"]["WebConfig"]["configuration"]["appSettings"].ChildNodes;
            foreach (XmlNode node in settingsNodes)
            {
                if (node.NodeType == XmlNodeType.Element && node.Name.Equals("add") && node.Attributes["key"].Value.Equals(key))
                {
                    return node.Attributes["value"].Value;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取web.config中指定节点的ConnectionStrings值
        /// </summary>
        /// <param name="name">配置节点Name</param>
        /// <returns>web.config中指定节点的ConnectionStrings值</returns>
        public static string GetConnectionString(this string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        /// <summary>
        /// 从指定路径的XML文件中读取指定节点的connectionStrings值。要求该XML文件结构和WEB.CONFIG(APP.CONFIG)结构一致
        /// </summary>
        /// <param name="name">配置节点Name</param>
        /// <param name="xmlFilePath">XML文件路径</param>
        /// <returns>XML文件中的connectionStrings节点值</returns>
        public static string GetConnectionString(this string name, string xmlFilePath)
        {
            XmlNode node = name.GetXmlConnectionStringNode(xmlFilePath);
            if (null != node)
            {
                return node.Attributes["connectionString"].Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 从指定路径的XML文件中读取指定节点的ConnectionStringSetting对象。要求该XML文件结构和WEB.CONFIG(APP.CONFIG)结构一致
        /// </summary>
        /// <param name="name">配置节点Name</param>
        /// <param name="xmlFilePath">XML文件路径</param>
        /// <returns>XML文件中读取指定节点的ConnectionStringSetting对象</returns>
        public static ConnectionStringSettings GetConnectionStringSetting(this string name, string xmlFilePath)
        {
            XmlNode node = name.GetXmlConnectionStringNode(xmlFilePath);
            if (null != node)
            {
                return new ConnectionStringSettings
                {
                    Name = name,
                    LockItem = node.Attributes["lockItem"].Value.Parse<bool>(),
                    ProviderName = node.Attributes["providerName"].Value,
                    ConnectionString = node.Attributes["connectionString"].Value
                };
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="xmlFilePath"></param>
        /// <returns></returns>
        private static XmlNode GetXmlConnectionStringNode(this string name, string xmlFilePath)
        {
            XmlDocument xmlDoc = xmlFilePath.LoadToXml();
            XmlNodeList settingsNodes = xmlDoc["Config"]["WebConfig"]["configuration"]["connectionStrings"].ChildNodes;
            foreach (XmlNode node in settingsNodes)
            {
                if (node.NodeType == XmlNodeType.Element && node.Name.Equals("add") && node.Attributes["name"].Value.Equals(name))
                {
                    return node;
                }
            }
            return null;
        }
    }
}