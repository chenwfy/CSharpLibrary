using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CSharpLib.Common
{
    /// <summary>
    /// XML文件操作帮助类
    /// </summary>
    public static class XMLHelper
    {
        /// <summary>
        /// 将指定的XML文件加载为XmlDocument对象，如果文件不存在则返回NULL
        /// </summary>
        /// <param name="xmlFilePath">XML文件路径</param>
        /// <returns>XmlDocument对象</returns>
        public static XmlDocument LoadToXml(this string xmlFilePath)
        {
            if (!File.Exists(xmlFilePath))
                return null;
            
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);
            return xmlDoc;
        }

        /// <summary>
        /// 将指定的XML流加载为XmlDocument对象
        /// </summary>
        /// <param name="stream">输入流</param>
        /// <returns>XmlDocument对象</returns>
        public static XmlDocument LoadToXml(this Stream stream)
        {
            if (null == stream || stream.Length == 0)
                return null;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(stream);
            return xmlDoc;
        }

        /// <summary>
        /// 将对象序列化为XML格式字串
        /// </summary>
        /// <param name="value">待序列化的对象</param>
        /// <returns>XML格式字串</returns>
        public static string XmlSerializer(this object value)
        {
            string xmlString = string.Empty;
            XmlSerializer xmlSerializer = new XmlSerializer(value.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                TextWriter writer = new StreamWriter(ms, Encoding.UTF8);
                XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
                xmlns.Add(String.Empty, String.Empty);
                xmlSerializer.Serialize(writer, value, xmlns);
                ms.Position = 0;
                byte[] buffer = new byte[ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                xmlString = Encoding.UTF8.GetString(buffer);
            }
            return ReplaceXmlDate(xmlString);
        }

        /// <summary>
        /// 替换XML字串中的日期字串
        /// </summary>
        /// <param name="containDateTimeSource"></param>
        /// <returns></returns>
        private static string ReplaceXmlDate(string containDateTimeSource)
        {
            string pattern = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}";
            MatchEvaluator matchEvaluator = new MatchEvaluator(XmlDateFormat);
            Regex reg = new Regex(pattern);
            string result = containDateTimeSource;
            result = reg.Replace(result, matchEvaluator);
            return result;
        }

        /// <summary>
        /// 格式化XML格式时间字串
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private static string XmlDateFormat(Match match)
        {
            return Convert.ToDateTime(match.Value).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// XML字串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象数据类型</typeparam>
        /// <param name="xml">XML字串</param>
        /// <returns>对象</returns>
        public static T XmlDeserialize<T>(this string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            return (T)xs.Deserialize(new StringReader(xml));
        }
    }
}