using System;
using System.Text;
using System.Net.Sockets;

namespace CSharpLib.Common.NoSql.Memcached
{
    /// <summary>
    /// 
    /// </summary>
    public class NetworkStreamIgnoreSeek : NetworkStream
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="access"></param>
        /// <param name="ownsSocket"></param>
        public NetworkStreamIgnoreSeek(Socket socket, System.IO.FileAccess access, bool ownsSocket)
            : base(socket, access, ownsSocket) 
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="access"></param>
        public NetworkStreamIgnoreSeek(Socket socket, System.IO.FileAccess access)
            : base(socket, access) 
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="ownsSocket"></param>
        public NetworkStreamIgnoreSeek(Socket socket, bool ownsSocket)
            : base(socket, ownsSocket) 
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        public NetworkStreamIgnoreSeek(Socket socket)
            : base(socket) 
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            return 0;
        }
    }
}