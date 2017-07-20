using System;

namespace CSharpLib.Common
{
    /// <summary>
    /// 基本类型格式转换帮助类
    /// </summary>
    public static class ParseHelper
    {
        /// <summary>
        /// 基本类型格式转换
        /// </summary>
        /// <typeparam name="T">要转换成的基本类型</typeparam>
        /// <param name="value">要转换的初始值</param>
        /// <returns>转换后的类型值</returns>
        /// <exception cref="NotSupportedException">参数T不受支持</exception>
        public static T Parse<T>(this string value)
        {
            Type t = typeof(T);

            if (t.Equals(typeof(string)))
            {
                return (T)(value as object);
            }
            else if (t.Equals(typeof(int)) || t.Equals(typeof(int?)))
            {
                int val;
                if (int.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(long)) || t.Equals(typeof(long?)))
            {
                long val;
                if (long.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(double)) || t.Equals(typeof(double?)))
            {
                double val;
                if (double.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(DateTime)) || t.Equals(typeof(DateTime?)))
            {
                DateTime val;
                if (DateTime.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(bool)) || t.Equals(typeof(bool?)))
            {
                bool val;
                if (bool.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(short)) || t.Equals(typeof(short?)))
            {
                short val;
                if (short.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(byte)) || t.Equals(typeof(byte?)))
            {
                byte val;
                if (byte.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(char)) || t.Equals(typeof(char?)))
            {
                char val;
                if (char.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }

            else if (t.Equals(typeof(float)) || t.Equals(typeof(float?)))
            {
                float val;
                if (float.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }

            else if (t.Equals(typeof(decimal)) || t.Equals(typeof(decimal?)))
            {
                decimal val;
                if (decimal.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(uint)) || t.Equals(typeof(uint?)))
            {
                uint val;
                if (uint.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(ulong)) || t.Equals(typeof(ulong?)))
            {
                ulong val;
                if (ulong.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(ushort)) || t.Equals(typeof(ushort?)))
            {
                ushort val;
                if (ushort.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else if (t.Equals(typeof(sbyte)) || t.Equals(typeof(sbyte?)))
            {
                sbyte val;
                if (sbyte.TryParse(value, out val))
                    return (T)(val as object);
                else
                    return default(T);
            }
            else
            {
                Type baseType = t.BaseType;
                if (baseType != null && baseType.Equals(typeof(Enum)))
                {
                    if (Enum.IsDefined(t, value))
                        return (T)Enum.Parse(t, value);
                    else
                        return default(T);
                }
                throw new NotSupportedException(t.ToString() + " not supported in STRING_PARSE_MODE。");
            }
        }
    }
}