using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using System.Diagnostics;

namespace NetworkAnalyzer
{
    public class PortWatcher : IDisposable
    {
        #region Default Parameters
        private static class DEFAULT
        {
            public const int DefaultInterval = 10000;
        }
        #endregion


        #region Constructors
        /// <summary>
        /// Create a watcher of port which watch all ports
        /// </summary>
        public PortWatcher()
        {
            watchedPorts = null;
            Init();
        }

        /// <summary>
        /// Create a watcher of port which watch a port
        /// </summary>
        /// <param name="port">Port to watch</param>
        public PortWatcher(int port)
        {
            watchedPorts = new int[] { port };
            Init();
        }

        /// <summary>
        /// Create a watcher of port which watch some ports
        /// </summary>
        /// <param name="ports">Ports to watch</param>
        public PortWatcher(int[] ports)
        {
            watchedPorts = ports;
            Init();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Init()
        {
            Interval = DEFAULT.DefaultInterval;

            Manager = new PortManager();

            // Runtime
            m_IsRunning = false;
            th_runtime = new Thread(new ThreadStart(Runtime));
            mx_runtime = new Mutex(false);
            last_entries = new Entry[0];
        }
        #endregion

        /// <summary>
        /// List of ports to watch
        /// Null value equals watch all
        /// </summary>
        private readonly int[] watchedPorts;

        public PortManager Manager
        {
            get;
            private set;
        }


        public int Interval // Boxed
        {
            get;
            set;
        }


        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            if (th_runtime.ThreadState != System.Threading.ThreadState.Unstarted)
                th_runtime.Interrupt();
        }

        public void Pause()
        {
            IsRunning = false;

            
        }

        public void Resume()
        {
            IsRunning = true;
        }


        private volatile bool m_IsRunning;
        public bool IsRunning // Boxed
        {
            get
            {
                return m_IsRunning;
            }
            private set
            {
                if (m_IsRunning != value)
                {
                    if (value)
                        if (th_runtime.ThreadState == System.Threading.ThreadState.Unstarted) // Start for first time
                            th_runtime.Start();
                        else // Restart
                            mx_runtime.ReleaseMutex();
                    else
                        mx_runtime.WaitOne();

                    m_IsRunning = value;
                }
            }
        }

        #region Runtime

        private Entry[] last_entries;

        private Mutex mx_runtime;
        private Thread th_runtime;
        private void Runtime()
        {
            try
            {
                // Start event
                Invoke_Started();

                do // Runtime
                {
                    mx_runtime.WaitOne();
                    mx_runtime.ReleaseMutex();

                    Runtime_main();
                    
                    System.Threading.Thread.Sleep(Interval);
                } while (th_runtime.ThreadState != System.Threading.ThreadState.StopRequested);
            }
            catch (ThreadInterruptedException)
            {
                m_IsRunning = false;
                // Stop event
                Invoke_Stopped();
            }
        }

        private void Runtime_main()
        {
            Entry[] entries;
            TcpEntry[] tcp_entries;
            UdpEntry[] udp_entries;

            lock (Manager)
            {
                Manager.Refresh();

                entries = Manager.Table.ToArray();
                tcp_entries = Manager.TCPTable.ToArray();
                udp_entries = Manager.UDPTable.ToArray();
            }

            // Removed entries
            TcpEntry[] r_tcp_entries = last_entries.Where(e => e is TcpEntry && !tcp_entries.Contains(e)).Cast<TcpEntry>().ToArray();
            UdpEntry[] r_udp_entries = last_entries.Where(e => e is UdpEntry && !udp_entries.Contains(e)).Cast<UdpEntry>().ToArray();

            Entry[] all_removed_entries = ((Entry[])r_udp_entries).Concat((Entry[])r_tcp_entries).ToArray();

            // New entries
            TcpEntry[] n_tcp_entries = tcp_entries.Where(e => !last_entries.Contains(e)).ToArray();
            UdpEntry[] n_udp_entries = udp_entries.Where(e => !last_entries.Contains(e)).ToArray();

            Entry[] all_new_entries = ((Entry[])n_udp_entries).Concat((Entry[])n_tcp_entries).ToArray();

            last_entries = entries;

            Invoke_NewEntryEvent(all_new_entries);
            Invoke_NewTCPEntryEvent(n_tcp_entries);
            Invoke_NewUDPEntryEvent(n_udp_entries);

            Invoke_RemovedEntryEvent(all_removed_entries);
            Invoke_RemovedTCPEntryEvent(r_tcp_entries);
            Invoke_RemovedUDPEntryEvent(r_udp_entries);
        }

        #endregion

        #region Events

        #region New Entries
        public delegate void PortWatcherTCPEntriesEventHandler(PortWatcher sender, TcpEntry[] entries);
        public delegate void PortWatcherUDPEntriesEventHandler(PortWatcher sender, UdpEntry[] entries);
        public delegate void PortWatcherEntriesEventHandler(PortWatcher sender, Entry[] entries);

        public event PortWatcherEntriesEventHandler     NewEntry;
        public event PortWatcherTCPEntriesEventHandler  NewTCPEntry;
        public event PortWatcherUDPEntriesEventHandler  NewUDPEntry;

        protected void Invoke_NewEntryEvent(Entry[] entries)
        {
            Invoke(NewEntry, entries);
        }
        protected void Invoke_NewTCPEntryEvent(TcpEntry[] entries)
        {
            Invoke(NewTCPEntry, entries);
        }
        protected void Invoke_NewUDPEntryEvent(UdpEntry[] entries)
        {
            Invoke(NewUDPEntry, entries);
        }
        #endregion

        #region Removed Entries
        public event PortWatcherEntriesEventHandler RemovedEntry;
        public event PortWatcherTCPEntriesEventHandler RemovedTCPEntry;
        public event PortWatcherUDPEntriesEventHandler RemovedUDPEntry;

        protected void Invoke_RemovedEntryEvent(Entry[] entries)
        {
            Invoke(RemovedEntry, entries);
        }
        protected void Invoke_RemovedTCPEntryEvent(TcpEntry[] entries)
        {
            Invoke(RemovedTCPEntry, entries);
        }
        protected void Invoke_RemovedUDPEntryEvent(UdpEntry[] entries)
        {
            Invoke(RemovedUDPEntry, entries);
        }
        #endregion

        #region Start / Stop
        public delegate void PortWatcherEventHandler(PortWatcher sender);

        public event PortWatcherEventHandler Started;
        public event PortWatcherEventHandler Stopped;

        protected void Invoke_Started()
        {
            Invoke(Started);
        }
        protected void Invoke_Stopped()
        {
            Invoke(Stopped);
        }
        #endregion

        #region Invoke

        protected void Invoke(Delegate eventToInvoke)
        {
            DirectInvoke(eventToInvoke, new object[] { this });
        }
        protected void Invoke(Delegate eventToInvoke, Entry[] entries)
        {
            if (entries != null && entries.Count() > 0)
                DirectInvoke(eventToInvoke, new object[] { this, entries });
        }
        protected void DirectInvoke(Delegate eventToInvoke, object[] param)
        {
            if (eventToInvoke == null)
                return;

            try
            {
                eventToInvoke.DynamicInvoke(param);
            }
            catch
            {
                this.Stop();
            }
        }

        #endregion

        #endregion

        #region Dispose
        void IDisposable.Dispose()
        {
            Stop();
        }
        #endregion
    }
}
