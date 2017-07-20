namespace CSharpLib.Common
{
    /// <summary>
    /// Adler32辅助类
    /// </summary>
    public static class Adler32Helper
    {
        /// <summary>
        /// 
        /// </summary>
        static Adler32Helper()
        {
        }

        /// <summary>
        /// 获取指定数据的Adler32校验码
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static long Adler32CheckSum(this byte[] buffer)
        {
            return buffer.Adler32CheckSum(0, buffer.Length);
        }

        /// <summary>
        /// 获取指定数据的Adler32校验码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long Adler32CheckSum(this byte[] buffer, int offset, int length)
        {
            Adler32 adl = new Adler32();
            adl.Update(buffer, offset, length);
            return adl.Checksum;
        }
    }
}