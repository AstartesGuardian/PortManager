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

            IsRunning = false;

            th_runtime = new Thread(new ThreadStart(Runtime));
            manager = new PortManager();

            // Runtime
            all_entries = new Entry[0];
            tcp_entries = new TcpEntry[0];
            udp_entries = new UdpEntry[0];
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
            if (!IsRunning)
                th_runtime.Start();
        }


        public void Stop()
        {
            if (IsRunning)
                th_runtime.Interrupt();
        }


        public bool IsRunning // Boxed
        {
            get;
            private set;
        }

        #region Runtime

        private Entry[] all_entries;
        private TcpEntry[] tcp_entries;
        private UdpEntry[] udp_entries;

        private Thread th_runtime;
        private void Runtime()
        {
            try
            {
                Invoke_Started();
                IsRunning = true;
                do
                {
                    lock (manager)
                    {
                        manager.Refresh();

                        tcp_entries = (from e in manager.TCPTable
                                       where (from el in all_entries where el.Equals(e) select el).Count() == 0
                                       select e).ToArray();

                        udp_entries = (from e in manager.UDPTable
                                       where !udp_entries.Contains(e)
                                       select e).ToArray();

                        Entry[] all_new_entries = ((Entry[])udp_entries).Concat((Entry[])tcp_entries).ToArray();
                        all_entries = manager.Table.ToArray();

                        Invoke_NewEntryEvent(all_new_entries);
                        Invoke_NewTCPEntryEvent(tcp_entries);
                        Invoke_NewUDPEntryEvent(udp_entries);
                    }

                    System.Threading.Thread.Sleep(Interval); // State Sleep -> Out Point by "Thread.Interrupt()"
                } while (true);
            }
            catch (ThreadInterruptedException)
            {
                IsRunning = false;
                Invoke_Stopped();
            }
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
            if (NewEntry != null && entries != null && entries.Count() > 0)
                Invoke(NewEntry, entries);
        }
        protected void Invoke_NewTCPEntryEvent(TcpEntry[] entries)
        {
            if (NewTCPEntry != null && entries != null && entries.Count() > 0)
                Invoke(NewTCPEntry, entries);
        }
        protected void Invoke_NewUDPEntryEvent(UdpEntry[] entries)
        {
            if (NewUDPEntry != null && entries != null && entries.Count() > 0)
                Invoke(NewUDPEntry, entries);
        }
        #endregion

        #region Start / Stop
        public delegate void PortWatcherEventHandler(PortWatcher sender);

        public event PortWatcherEventHandler Started;
        public event PortWatcherEventHandler Stopped;

        protected void Invoke_Started()
        {
            if (Started != null)
                Invoke(Started);
        }
        protected void Invoke_Stopped()
        {
            if (Stopped != null)
                Invoke(Stopped);
        }
        #endregion

        #region Invoke

        protected void Invoke(Delegate eventToInvoke)
        {
            DirectInvoke(eventToInvoke, new object[] { this });
        }
        protected void Invoke(Delegate eventToInvoke, object param)
        {
            DirectInvoke(eventToInvoke, new object[] { this, param });
        }
        protected void DirectInvoke(Delegate eventToInvoke, object[] param)
        {
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
