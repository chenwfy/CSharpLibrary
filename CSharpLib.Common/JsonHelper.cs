using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CSharpLib.Common
{
    /// <summary>
    /// JOSN序列化与反序列化帮助类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 将对象序列为JSON格式字串
        /// </summary>
        /// <param name="value">待序列化的对象</param>
        /// <returns>JSON格式字串</returns>
        public static string JsonSerialize(this object value)
        {
            if (null == value)
                return string.Empty;

            return ReplaceJsonDate(JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// 将JSON字串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象数据类型</typeparam>
        /// <param name="json">JSON字串</param>
        /// <returns>反序列化后的对象</returns>
        public static T JsonDeserialize<T>(this string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);

            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 替换JSON字串中的日期字串
        /// </summary>
        /// <param name="containDateTimeSource"></param>
        /// <returns></returns>
        private static string ReplaceJsonDate(string containDateTimeSource)
        {
            string pattern = @"\\/Date\((\d+)\+\d+\)\\/";
            MatchEvaluator matchEvaluator = new MatchEvaluator(JsonDateFormat);
            Regex reg = new Regex(pattern);
            string result = containDateTimeSource;
            result = reg.Replace(result, matchEvaluator);
            return result;
        }

        /// <summary>
        /// 格式化JSON格式时间字串
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private static string JsonDateFormat(Match match)
        {
            string dateTime = string.Empty;
            DateTime time = new DateTime(1970, 1, 1);
            time = time.AddMilliseconds(long.Parse(match.Groups[1].Value));
            time = time.ToLocalTime();
            dateTime = time.ToString("yyyy-MM-dd HH:mm:ss");
            return dateTime;
        }
    }
}