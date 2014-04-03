using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using System.Diagnostics;

namespace NetworkAnalyzer
{
    public class PortWatcher
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

            manager = new PortManager();

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

        private PortManager manager;


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


        private bool m_IsRunning;
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
                Invoke_Started();
                do
                {
                    mx_runtime.WaitOne();
                    mx_runtime.ReleaseMutex();

                    lock (manager)
                    {
                        Runtime_main();
                    }
                    
                    System.Threading.Thread.Sleep(Interval);
                } while (th_runtime.ThreadState != System.Threading.ThreadState.StopRequested);
            }
            catch (ThreadInterruptedException)
            {
                m_IsRunning = false;
                Invoke_Stopped();
            }
        }

        private void Runtime_main()
        {
            manager.Refresh();

            // Removed entries
            TcpEntry[] r_tcp_entries = (from e in last_entries
                                        where e is TcpEntry && (from el in manager.TCPTable where el.Equals(e) select el).Count() == 0
                                        select (TcpEntry)e).ToArray();

            UdpEntry[] r_udp_entries = (from e in last_entries
                                        where e is UdpEntry && (from el in manager.UDPTable where el.Equals(e) select el).Count() == 0
                                        select (UdpEntry)e).ToArray();

            Entry[] all_removed_entries = ((Entry[])r_udp_entries).Concat((Entry[])r_tcp_entries).ToArray();

            // New entries
            TcpEntry[] tcp_entries = (from e in manager.TCPTable
                                      where (from el in last_entries where el.Equals(e) select el).Count() == 0
                                      select e).ToArray();

            UdpEntry[] udp_entries = (from e in manager.UDPTable
                                      where (from el in last_entries where el.Equals(e) select el).Count() == 0
                                      select e).ToArray();

            Entry[] all_new_entries = ((Entry[])udp_entries).Concat((Entry[])tcp_entries).ToArray();

            last_entries = manager.Table.ToArray();

            Invoke_NewEntryEvent(all_new_entries);
            Invoke_NewTCPEntryEvent(tcp_entries);
            Invoke_NewUDPEntryEvent(udp_entries);

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
    }
}
