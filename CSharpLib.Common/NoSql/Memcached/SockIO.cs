using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Threading;

namespace CSharpLib.Common.NoSql.Memcached
{
    /// <summary>
    /// 
    /// </summary>
    public class SockIO
    {
        private static int IdGenerator;
        private int _id;
        private DateTime _created;
        private SockIOPool _pool;
        private String _host;
        private Socket _socket;
        private Stream _networkStream;

        /// <summary>
        /// 
        /// </summary>
        private SockIO()
        {
            _id = Interlocked.Increment(ref IdGenerator);
            _created = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="timeout"></param>
        /// <param name="connectTimeout"></param>
        /// <param name="noDelay"></param>
        public SockIO(SockIOPool pool, String host, int port, int timeout, int connectTimeout, bool noDelay)
            : this()
        {
            if (host == null || host.Length == 0)
                throw new ArgumentNullException(GetLocalizedString("host"), GetLocalizedString("null host"));

            _pool = pool;

            if (connectTimeout > 0)
            {
                _socket = GetSocket(host, port, connectTimeout);
            }
            else
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(new IPEndPoint(IPAddress.Parse(host), port));
            }

            _networkStream = new BufferedStream(new NetworkStreamIgnoreSeek(_socket));

            _host = host + ":" + port;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="host"></param>
        /// <param name="timeout"></param>
        /// <param name="connectTimeout"></param>
        /// <param name="noDelay"></param>
        public SockIO(SockIOPool pool, String host, int timeout, int connectTimeout, bool noDelay)
            : this()
        {
            if (host == null || host.Length == 0)
                throw new ArgumentNullException(GetLocalizedString("host"), GetLocalizedString("null host"));

            _pool = pool;

            String[] ip = host.Split(':');

            if (connectTimeout > 0)
            {
                _socket = GetSocket(ip[0], int.Parse(ip[1], new System.Globalization.NumberFormatInfo()), connectTimeout);
            }
            else
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(new IPEndPoint(IPAddress.Parse(ip[0]), int.Parse(ip[1], new System.Globalization.NumberFormatInfo())));
            }

            _networkStream = new BufferedStream(new NetworkStreamIgnoreSeek(_socket));
            _host = host;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected static Socket GetSocket(String host, int port, int timeout)
        {
            ConnectThread thread = new ConnectThread(host, port);
            thread.Start();

            int timer = 0;
            int sleep = 25;

            while (timer < timeout)
            {
                if (thread.IsConnected)
                    return thread.Socket;

                if (thread.IsError)
                    throw new IOException();

                try
                {
                    Thread.Sleep(sleep);
                }
                catch (ThreadInterruptedException) { }

                timer += sleep;
            }

            throw new IOException(GetLocalizedString("connect timeout").Replace("$$timeout$$", timeout.ToString(new System.Globalization.NumberFormatInfo())));
        }

        /// <summary>
        /// 
        /// </summary>
        public string Host
        {
            get { return _host; }
        }

        /// <summary>
        /// 
        /// </summary>
        public void TrueClose()
        {
            bool err = false;
            StringBuilder errMsg = new StringBuilder();

            if (_socket == null || _networkStream == null)
            {
                err = true;
                errMsg.Append(GetLocalizedString("socket already closed"));
            }

            if (_socket != null)
            {
                try
                {
                    _socket.Close();
                }
                catch (IOException ioe)
                {
                    errMsg.Append(GetLocalizedString("error closing socket").Replace("$$ToString$$", ToString()).Replace("$$Host$$", Host) + System.Environment.NewLine);
                    errMsg.Append(ioe.ToString());
                    err = true;
                }
                catch (SocketException soe)
                {
                    errMsg.Append(GetLocalizedString("error closing socket").Replace("$$ToString$$", ToString()).Replace("$$Host$$", Host) + System.Environment.NewLine);
                    errMsg.Append(soe.ToString());
                    err = true;
                }
            }

            if (_socket != null)
                _pool.CheckIn(this, false);

            _networkStream = null;
            _socket = null;

            if (err)
                throw new IOException(errMsg.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            _pool.CheckIn(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected
        {
            get { return _socket != null && _socket.Connected; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            if (_socket == null || !_socket.Connected)
            {
                throw new IOException(GetLocalizedString("read closed socket"));
            }

            byte[] b = new byte[1];
            MemoryStream memoryStream = new MemoryStream();
            bool eol = false;

            while (_networkStream.Read(b, 0, 1) != -1)
            {

                if (b[0] == 13)
                {
                    eol = true;

                }
                else
                {
                    if (eol)
                    {
                        if (b[0] == 10)
                            break;

                        eol = false;
                    }
                }

                memoryStream.Write(b, 0, 1);
            }

            if (memoryStream == null || memoryStream.Length <= 0)
            {
                throw new IOException(GetLocalizedString("closing dead stream"));
            }

            string temp = UTF8Encoding.UTF8.GetString(memoryStream.GetBuffer()).TrimEnd('\0', '\r', '\n');
            return temp;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearEndOfLine()
        {
            if (_socket == null || !_socket.Connected)
            {
                throw new IOException(GetLocalizedString("read closed socket"));
            }

            byte[] b = new byte[1];
            bool eol = false;
            while (_networkStream.Read(b, 0, 1) != -1)
            {

                if (b[0] == 13)
                {
                    eol = true;
                    continue;
                }

                if (eol)
                {
                    if (b[0] == 10)
                        break;

                    eol = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public void Read(byte[] bytes)
        {
            if (_socket == null || !_socket.Connected)
            {
                throw new IOException(GetLocalizedString("read closed socket"));
            }

            if (bytes == null)
                return;

            int count = 0;
            while (count < bytes.Length)
            {
                int cnt = _networkStream.Read(bytes, count, (bytes.Length - count));
                count += cnt;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Flush()
        {
            if (_socket == null || !_socket.Connected)
            {
                throw new IOException(GetLocalizedString("write closed socket"));
            }
            _networkStream.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public void Write(byte[] bytes)
        {
            Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Write(byte[] bytes, int offset, int count)
        {
            if (_socket == null || !_socket.Connected)
            {
                throw new IOException(GetLocalizedString("write closed socket"));
            }
            if (bytes != null)
                _networkStream.Write(bytes, offset, count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_socket == null)
                return "";
            return _id.ToString(new System.Globalization.NumberFormatInfo());
        }

        /// <summary>
        /// 
        /// </summary>
        private class ConnectThread
        {
            Thread _thread;
            private Socket _socket;
            private String _host;
            private int _port;
            bool _error;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="host"></param>
            /// <param name="port"></param>
            public ConnectThread(string host, int port)
            {
                _host = host;
                _port = port;

                _thread = new Thread(new ThreadStart(Connect));
                _thread.IsBackground = true;
            }

            /// <summary>
            /// 
            /// </summary>
            private void Connect()
            {
                try
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Connect(new IPEndPoint(IPAddress.Parse(_host), _port));
                }
                catch (IOException)
                {
                    _error = true;
                }
                catch (SocketException ex)
                {
                    _error = true;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                _thread.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public bool IsConnected
            {
                get { return _socket != null && _socket.Connected; }
            }

            /// <summary>
            /// 
            /// </summary>
            public bool IsError
            {
                get { return _error; }
            }

            /// <summary>
            /// 
            /// </summary>
            public Socket Socket
            {
                get { return _socket; }
            }
        }

        private static ResourceManager _resourceManager = new ResourceManager("Memcached.ClientLibrary.StringMessages", typeof(SockIO).Assembly);
        private static string GetLocalizedString(string key)
        {
            return _resourceManager.GetString(key);
        }
    }
}