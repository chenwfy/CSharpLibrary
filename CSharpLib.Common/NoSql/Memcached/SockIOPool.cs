using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace CSharpLib.Common.NoSql.Memcached
{
    /// <summary>
    /// 
    /// </summary>
    public enum HashingAlgorithm
    {
        Native = 0,
        OldCompatibleHash = 1,
        NewCompatibleHash = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public class SockIOPool
    {
        private static Hashtable Pools = new Hashtable();
        private MaintenanceThread _maintenanceThread;
        private bool _initialized;
        private int _maxCreate = 1;
        private Hashtable _createShift;

        private int _poolMultiplier = 4;
        private int _initConns = 3;
        private int _minConns = 3;
        private int _maxConns = 10;
        private long _maxIdle = 1000 * 60 * 3;
        private long _maxBusyTime = 1000 * 60 * 5;
        private long _maintThreadSleep = 1000 * 5;
        private int _socketTimeout = 1000 * 10;
        private int _socketConnectTimeout = 50;
        private bool _failover = true;
        private bool _nagle = true;
        private HashingAlgorithm _hashingAlgorithm = HashingAlgorithm.Native;

        private ArrayList _servers;
        private ArrayList _weights;
        private ArrayList _buckets;

        private Hashtable _hostDead;
        private Hashtable _hostDeadDuration;

        private Hashtable _availPool;

        private Hashtable _busyPool;

        protected SockIOPool() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static SockIOPool GetInstance(String poolName)
        {
            if (Pools.ContainsKey(poolName))
                return (SockIOPool)Pools[poolName];

            SockIOPool pool = new SockIOPool();
            Pools[poolName] = pool;

            return pool;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static SockIOPool GetInstance()
        {
            return GetInstance(GetLocalizedString("default instance"));
        }

        /// <summary>
        /// 
        /// </summary>
        public ArrayList Servers
        {
            get { return _servers; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servers"></param>
        public void SetServers(string[] servers)
        {
            SetServers(new ArrayList(servers));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servers"></param>
        public void SetServers(ArrayList servers)
        {
            _servers = servers;
        }

        /// <summary>
        /// 
        /// </summary>
        public ArrayList Weights
        {
            get { return _weights; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weights"></param>
        public void SetWeights(int[] weights)
        {
            SetWeights(new ArrayList(weights));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weights"></param>
        public void SetWeights(ArrayList weights)
        {
            _weights = weights;
        }

        /// <summary>
        /// 
        /// </summary>
        public int InitConnections
        {
            get { return _initConns; }
            set { _initConns = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MinConnections
        {
            get { return _minConns; }
            set { _minConns = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MaxConnections
        {
            get { return _maxConns; }
            set { _maxConns = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long MaxIdle
        {
            get { return _maxIdle; }
            set { _maxIdle = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long MaxBusy
        {
            get { return _maxBusyTime; }
            set { _maxBusyTime = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long MaintenanceSleep
        {
            get { return _maintThreadSleep; }
            set { _maintThreadSleep = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int SocketTimeout
        {
            get { return _socketTimeout; }
            set { _socketTimeout = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int SocketConnectTimeout
        {
            get { return _socketConnectTimeout; }
            set { _socketConnectTimeout = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Failover
        {
            get { return _failover; }
            set { _failover = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Nagle
        {
            get { return _nagle; }
            set { _nagle = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public HashingAlgorithm HashingAlgorithm
        {
            get { return _hashingAlgorithm; }
            set { _hashingAlgorithm = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static int OriginalHashingAlgorithm(string key)
        {
            int hash = 0;
            char[] cArr = key.ToCharArray();

            for (int i = 0; i < cArr.Length; ++i)
            {
                hash = (hash * 33) + cArr[i];
            }

            return hash;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static int NewHashingAlgorithm(string key)
        {
            CRCTool checksum = new CRCTool();
            checksum.Init(CRCTool.CRCCode.CRC32);
            int crc = (int)checksum.crctablefast(UTF8Encoding.UTF8.GetBytes(key));

            return (crc >> 16) & 0x7fff;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Initialize()
        {
            if (_initialized
                && _buckets != null
                && _availPool != null
                && _busyPool != null)
            {
                return;
            }

            _buckets = new ArrayList();
            _availPool = new Hashtable(_servers.Count * _initConns);
            _busyPool = new Hashtable(_servers.Count * _initConns);
            _hostDeadDuration = new Hashtable();
            _hostDead = new Hashtable();
            _createShift = new Hashtable();
            _maxCreate = (_poolMultiplier > _minConns) ? _minConns : _minConns / _poolMultiplier;		// only create up to maxCreate connections at once

            if (_servers == null || _servers.Count <= 0)
            {
                throw new ArgumentException(GetLocalizedString("initialize with no servers"));
            }

            for (int i = 0; i < _servers.Count; i++)
            {
                if (_weights != null && _weights.Count > i)
                {
                    for (int k = 0; k < ((int)_weights[i]); k++)
                    {
                        _buckets.Add(_servers[i]);
                    }
                }
                else
                {
                    _buckets.Add(_servers[i]);
                }

                for (int j = 0; j < _initConns; j++)
                {
                    SockIO socket = CreateSocket((string)_servers[i]);
                    if (socket == null)
                    {
                        break;
                    }

                    AddSocketToPool(_availPool, (string)_servers[i], socket);
                }
            }

            _initialized = true;

            if (_maintThreadSleep > 0)
                this.StartMaintenanceThread();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Initialized
        {
            get { return _initialized; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        protected SockIO CreateSocket(string host)
        {
            SockIO socket = null;
            if (_failover && _hostDead.ContainsKey(host) && _hostDeadDuration.ContainsKey(host))
            {

                DateTime store = (DateTime)_hostDead[host];
                long expire = ((long)_hostDeadDuration[host]);

                if ((store.AddMilliseconds(expire)) > DateTime.Now)
                    return null;
            }

            try
            {
                socket = new SockIO(this, host, _socketTimeout, _socketConnectTimeout, _nagle);

                if (!socket.IsConnected)
                {
                    try
                    {
                        socket.TrueClose();
                    }
                    catch (SocketException ex)
                    {
                        socket = null;
                    }
                }
            }
            catch (SocketException ex)
            {
                socket = null;
            }
            catch (ArgumentException ex)
            {
                socket = null;
            }
            catch (IOException ex)
            {
                socket = null;
            }

            if (socket == null)
            {
                DateTime now = DateTime.Now;
                _hostDead[host] = now;
                long expire = (_hostDeadDuration.ContainsKey(host)) ? (((long)_hostDeadDuration[host]) * 2) : 100;
                _hostDeadDuration[host] = expire;
                ClearHostFromPool(_availPool, host);
            }
            else
            {
                _hostDead.Remove(host);
                _hostDeadDuration.Remove(host);
                if (_buckets.BinarySearch(host) < 0)
                    _buckets.Add(host);
            }

            return socket;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SockIO GetSock(string key)
        {
            return GetSock(key, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public SockIO GetSock(string key, object hashCode)
        {
            string hashCodeString = "<null>";
            if (hashCode != null)
                hashCodeString = hashCode.ToString();


            if (key == null || key.Length == 0)
            {
                return null;
            }

            if (!_initialized)
            {
                return null;
            }

            if (_buckets.Count == 0)
                return null;

            if (_buckets.Count == 1)
                return GetConnection((string)_buckets[0]);

            int tries = 0;

            int hv;
            if (hashCode != null)
            {
                hv = (int)hashCode;
            }
            else
            {

                switch (_hashingAlgorithm)
                {
                    case HashingAlgorithm.Native:
                        hv = key.GetHashCode();
                        break;

                    case HashingAlgorithm.OldCompatibleHash:
                        hv = OriginalHashingAlgorithm(key);
                        break;

                    case HashingAlgorithm.NewCompatibleHash:
                        hv = NewHashingAlgorithm(key);
                        break;

                    default:
                        hv = key.GetHashCode();
                        _hashingAlgorithm = HashingAlgorithm.Native;
                        break;
                }
            }

            while (tries++ <= _buckets.Count)
            {
                int bucket = hv % _buckets.Count;
                if (bucket < 0)
                    bucket += _buckets.Count;

                SockIO sock = GetConnection((string)_buckets[bucket]);

                if (sock != null)
                    return sock;

                if (!_failover)
                    return null;

                switch (_hashingAlgorithm)
                {
                    case HashingAlgorithm.Native:
                        hv += ((string)("" + tries + key)).GetHashCode();
                        break;

                    case HashingAlgorithm.OldCompatibleHash:
                        hv += OriginalHashingAlgorithm("" + tries + key);
                        break;

                    case HashingAlgorithm.NewCompatibleHash:
                        hv += NewHashingAlgorithm("" + tries + key);
                        break;

                    default:
                        hv += ((string)("" + tries + key)).GetHashCode();
                        _hashingAlgorithm = HashingAlgorithm.Native;
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public SockIO GetConnection(string host)
        {
            if (!_initialized)
            {
                return null;
            }

            if (host == null)
                return null;

            if (_availPool != null && !(_availPool.Count == 0))
            {
                Hashtable aSockets = (Hashtable)_availPool[host];

                if (aSockets != null && !(aSockets.Count == 0))
                {
                    foreach (SockIO socket in new IteratorIsolateCollection(aSockets.Keys))
                    {
                        if (socket.IsConnected)
                        {
                            aSockets.Remove(socket);
                            AddSocketToPool(_busyPool, host, socket);
                            return socket;
                        }
                        else
                        {
                            aSockets.Remove(socket);
                        }

                    }
                }
            }

            object cShift = _createShift[host];
            int shift = (cShift != null) ? (int)cShift : 0;

            int create = 1 << shift;
            if (create >= _maxCreate)
            {
                create = _maxCreate;
            }
            else
            {
                shift++;
            }

            _createShift[host] = shift;

            for (int i = create; i > 0; i--)
            {
                SockIO socket = CreateSocket(host);
                if (socket == null)
                    break;

                if (i == 1)
                {
                    AddSocketToPool(_busyPool, host, socket);
                    return socket;
                }
                else
                {
                    AddSocketToPool(_availPool, host, socket);
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="host"></param>
        /// <param name="socket"></param>
        protected static void AddSocketToPool(Hashtable pool, string host, SockIO socket)
        {
            if (pool == null)
                return;

            Hashtable sockets;
            if (host != null && host.Length != 0 && pool.ContainsKey(host))
            {
                sockets = (Hashtable)pool[host];
                if (sockets != null)
                {
                    sockets[socket] = DateTime.Now;

                    return;
                }
            }

            sockets = new Hashtable();
            sockets[socket] = DateTime.Now;
            pool[host] = sockets;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="host"></param>
        /// <param name="socket"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected static void RemoveSocketFromPool(Hashtable pool, string host, SockIO socket)
        {
            if (host != null && host.Length == 0 || pool == null)
                return;

            if (pool.ContainsKey(host))
            {
                Hashtable sockets = (Hashtable)pool[host];
                if (sockets != null)
                {
                    sockets.Remove(socket);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="host"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected static void ClearHostFromPool(Hashtable pool, string host)
        {
            if (pool == null || host != null && host.Length == 0)
                return;

            if (pool.ContainsKey(host))
            {
                Hashtable sockets = (Hashtable)pool[host];

                if (sockets != null && sockets.Count > 0)
                {
                    foreach (SockIO socket in new IteratorIsolateCollection(sockets.Keys))
                    {
                        try
                        {
                            socket.TrueClose();
                        }
                        catch (IOException ioe)
                        {
                        }
                        sockets.Remove(socket);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="addToAvail"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CheckIn(SockIO socket, bool addToAvail)
        {
            if (socket == null)
                return;

            string host = socket.Host;
            RemoveSocketFromPool(_busyPool, host, socket);

            // add to avail pool
            if (addToAvail && socket.IsConnected)
            {
                AddSocketToPool(_availPool, host, socket);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CheckIn(SockIO socket)
        {
            CheckIn(socket, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pool"></param>
        protected static void ClosePool(Hashtable pool)
        {
            if (pool == null)
                return;

            foreach (string host in pool.Keys)
            {
                Hashtable sockets = (Hashtable)pool[host];

                foreach (SockIO socket in new IteratorIsolateCollection(sockets.Keys))
                {
                    try
                    {
                        socket.TrueClose();
                    }
                    catch (IOException ioe)
                    {
                    }

                    sockets.Remove(socket);
                }
            }
        }

        /// <summary>
        /// Shuts down the pool.
        /// 
        /// Cleanly closes all sockets.
        /// Stops the maint thread.
        /// Nulls out all internal maps
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Shutdown()
        {
            if (_maintenanceThread != null && _maintenanceThread.IsRunning)
                StopMaintenanceThread();

            ClosePool(_availPool);
            ClosePool(_busyPool);
            _availPool = null;
            _busyPool = null;
            _buckets = null;
            _hostDeadDuration = null;
            _hostDead = null;
            _initialized = false;
        }

        /// <summary>
        /// Starts the maintenance thread.
        /// 
        /// This thread will manage the size of the active pool
        /// as well as move any closed, but not checked in sockets
        /// back to the available pool.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void StartMaintenanceThread()
        {
            if (_maintenanceThread != null)
            {

                if (_maintenanceThread.IsRunning)
                {
                }
                else
                {
                    _maintenanceThread.Start();
                }
            }
            else
            {
                _maintenanceThread = new MaintenanceThread(this);
                _maintenanceThread.Interval = _maintThreadSleep;
                _maintenanceThread.Start();
            }
        }

        /// <summary>
        /// Stops the maintenance thread.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void StopMaintenanceThread()
        {
            if (_maintenanceThread != null && _maintenanceThread.IsRunning)
                _maintenanceThread.StopThread();
        }

        /// <summary>
        /// Runs self maintenance on all internal pools.
        /// 
        /// This is typically called by the maintenance thread to manage pool size. 
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void SelfMaintain()
        {
            foreach (string host in new IteratorIsolateCollection(_availPool.Keys))
            {
                Hashtable sockets = (Hashtable)_availPool[host];

                if (sockets.Count < _minConns)
                {
                    // need to create new sockets
                    int need = _minConns - sockets.Count;

                    for (int j = 0; j < need; j++)
                    {
                        SockIO socket = CreateSocket(host);

                        if (socket == null)
                            break;

                        AddSocketToPool(_availPool, host, socket);
                    }
                }
                else if (sockets.Count > _maxConns)
                {
                    // need to close down some sockets
                    int diff = sockets.Count - _maxConns;
                    int needToClose = (diff <= _poolMultiplier)
                        ? diff
                        : (diff) / _poolMultiplier;


                    foreach (SockIO socket in new IteratorIsolateCollection(sockets.Keys))
                    {
                        if (needToClose <= 0)
                            break;

                        // remove stale entries
                        DateTime expire = (DateTime)sockets[socket];

                        // if past idle time
                        // then close socket
                        // and remove from pool
                        if ((expire.AddMilliseconds(_maxIdle)) < DateTime.Now)
                        {
                            try
                            {
                                socket.TrueClose();
                            }
                            catch (IOException ioe)
                            {
                            }

                            sockets.Remove(socket);
                            needToClose--;
                        }
                    }
                }

                // reset the shift value for creating new SockIO objects
                _createShift[host] = 0;
            }

            // go through busy sockets and destroy sockets
            // as needed to maintain pool settings
            foreach (string host in _busyPool.Keys)
            {
                Hashtable sockets = (Hashtable)_busyPool[host];
                // loop through all connections and check to see if we have any hung connections
                foreach (SockIO socket in new IteratorIsolateCollection(sockets.Keys))
                {
                    // remove stale entries
                    DateTime hungTime = (DateTime)sockets[socket];

                    // if past max busy time
                    // then close socket
                    // and remove from pool
                    if ((hungTime.AddMilliseconds(_maxBusyTime)) < DateTime.Now)
                    {
                        try
                        {
                            socket.TrueClose();
                        }
                        catch (IOException ioe)
                        {
                        }

                        sockets.Remove(socket);
                    }
                }
            }
        }

        /// <summary>
        /// Class which extends thread and handles maintenance of the pool.
        /// </summary>
        private class MaintenanceThread
        {
            private MaintenanceThread() { }

            private Thread _thread;

            private SockIOPool _pool;
            private long _interval = 1000 * 3; // every 3 seconds
            private bool _stopThread;

            public MaintenanceThread(SockIOPool pool)
            {
                _thread = new Thread(new ThreadStart(Maintain));
                _pool = pool;
            }

            public long Interval
            {
                get { return _interval; }
                set { _interval = value; }
            }

            public bool IsRunning
            {
                get { return _thread.IsAlive; }
            }

            /// <summary>
            /// Sets stop variable and interups and wait
            /// </summary>
            public void StopThread()
            {
                _stopThread = true;
                _thread.Interrupt();
            }

            /// <summary>
            /// The logic of the thread.
            /// </summary>
            private void Maintain()
            {
                while (!_stopThread)
                {
                    try
                    {
                        Thread.Sleep((int)_interval);

                        // if pool is initialized, then
                        // run the maintenance method on itself
                        if (_pool.Initialized)
                            _pool.SelfMaintain();

                    }
                    catch (ThreadInterruptedException) { } //MaxM: When SockIOPool.getInstance().shutDown() is called, this exception will be raised and can be safely ignored.
                    catch (Exception ex)
                    {
                    }
                }
            }

            /// <summary>
            /// Start the thread
            /// </summary>
            public void Start()
            {
                _stopThread = false;
                _thread.Start();
            }
        }

        private static ResourceManager _resourceManager = new ResourceManager("Memcached.ClientLibrary.StringMessages", typeof(SockIOPool).Assembly);
        private static string GetLocalizedString(string key)
        {
            return _resourceManager.GetString(key);
        }
    }
}