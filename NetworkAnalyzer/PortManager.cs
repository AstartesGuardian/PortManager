using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace NetworkAnalyzer
{
    public partial class PortManager
    {
        #region Imports

        #region General
        /// <summary>
        /// Get Heap of current process
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern IntPtr GetProcessHeap();

        /// <summary>
        /// Free memory
        /// </summary>
        /// <param name="hHeap"></param>
        /// <param name="dwFlags"></param>
        /// <param name="lpMem"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int HeapFree(IntPtr hHeap, int dwFlags, IntPtr lpMem);
        #endregion

        #region XP
        /// <summary>
        /// Table TCP [XP]
        /// </summary>
        /// <param name="ppTCPTable"></param>
        /// <param name="bOrder"></param>
        /// <param name="hHeap"></param>
        /// <param name="dwFlags"></param>
        /// <param name="dwFamily"></param>
        /// <returns></returns>
        [DllImport("IPHLPAPI")]
        private static extern Int32 AllocateAndGetTcpExTableFromStack(ref IntPtr ppTCPTable, [MarshalAs(UnmanagedType.Bool)] bool bOrder, IntPtr hHeap, IntPtr dwFlags, Int32 dwFamily);
        
        /// <summary>
        /// Table UDP [XP]
        /// </summary>
        /// <param name="ppUDPTable"></param>
        /// <param name="bOrder"></param>
        /// <param name="hHeap"></param>
        /// <param name="dwFlags"></param>
        /// <param name="dwFamily"></param>
        /// <returns></returns>
        [DllImport("IPHLPAPI")]
        private static extern Int32 AllocateAndGetUdpExTableFromStack(ref IntPtr ppUDPTable, [MarshalAs(UnmanagedType.Bool)] bool bOrder, IntPtr hHeap, IntPtr dwFlags, Int32 dwFamily);
        #endregion

        #region NT
        /// <summary>
        /// Table TCP [NT]
        /// </summary>
        /// <param name="pTcpTable"></param>
        /// <param name="dwOutBufLen"></param>
        /// <param name="bOrder"></param>
        /// <param name="dwFamily"></param>
        /// <param name="dwClass"></param>
        /// <param name="dwReserved"></param>
        /// <returns></returns>
        [DllImport("IPHLPAPI")]
        private static extern int GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool bOrder, int dwFamily, TCP_TABLE_CLASS dwClass, int dwReserved);
        /// <summary>
        /// Table UDP [NT]
        /// </summary>
        /// <param name="pUdpTable"></param>
        /// <param name="dwOutBufLen"></param>
        /// <param name="bOrder"></param>
        /// <param name="dwFamily"></param>
        /// <param name="dwClass"></param>
        /// <param name="dwReserved"></param>
        /// <returns></returns>
        [DllImport("IPHLPAPI")]
        private static extern int GetExtendedUdpTable(IntPtr pUdpTable, ref int dwOutBufLen, bool bOrder, int dwFamily, UDP_TABLE_CLASS dwClass, int dwReserved);
        #endregion

        #endregion

        public PortManager()
        {
            this.loaded = false;
        }

        #region First Loading
        /// <summary>
        /// Indicate if the loading has already be made successfully
        /// </summary>
        protected bool loaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Refresh for the first time if it has not already be made
        /// </summary>
        protected void RefreshIfNotAlreadyDone()
        {
            if (!this.loaded)
                Refresh();
        }
        #endregion

        #region Constant
        /// <summary>
        /// IP Version 4
        /// </summary>
        private const Int32 FAMILY_V4 = 2;
        #endregion

        #region Lists

        #region TCP
        /// <summary>
        /// Table TCP
        /// </summary>
        private List<TcpEntry> m_TcpTable = new List<TcpEntry>();

        /// <summary>
        /// Table TCP
        /// </summary>
        public IEnumerable<TcpEntry> TCPTable
        {
            get
            {
                RefreshIfNotAlreadyDone();
                return m_TcpTable;
            }
        }
        #endregion

        #region UDP
        /// <summary>
        /// Table UDP
        /// </summary>
        private List<UdpEntry> m_UdpTable = new List<UdpEntry>();

        /// <summary>
        /// Table UDP
        /// </summary>
        public IEnumerable<UdpEntry> UDPTable
        {
            get
            {
                RefreshIfNotAlreadyDone(); return m_UdpTable;
            }
        }
        #endregion

        /// <summary>
        /// TCP & UDP Table
        /// </summary>
        public IEnumerable<Entry> Table
        {
            get
            {
                return (
                            from entry in TCPTable
                            select (Entry)entry
                        )
                        .Union
                        (
                            from entry in UDPTable
                            select (Entry)entry
                        );
            }
        }

        #endregion

        #region Manager

        /// <summary>
        /// Refresh the lists of opened ports
        /// </summary>
        public void Refresh()
        {
            this.m_TcpTable.Clear();
            this.m_UdpTable.Clear();

            if (IsWindowsNT())
            {
                getOpenedPorts_NT();
            }
            else
            {
                getOpenedPorts_XP();
            }

            this.loaded = true;
        }

        private void getOpenedPorts_NT()
        {
            this.m_TcpTable = getOpenedPorts_NT_Get<TcpEntry>();
            this.m_UdpTable = getOpenedPorts_NT_Get<UdpEntry>();
        }

        private void getOpenedPorts_XP()
        {
            this.m_TcpTable = getOpenedPorts_XP_Get<TcpEntry>();
            this.m_UdpTable = getOpenedPorts_XP_Get<UdpEntry>();
        }


        protected bool IsWindowsNT()
        {
            return ((Environment.OSVersion.Platform == PlatformID.Win32NT) &&
                (Environment.OSVersion.Version.Major == 6));
        }

        // T : TcpEntry / UdpEntry
        private List<T> getOpenedPorts_XP_Get<T>()
        {
            IntPtr ptrTable;
            int ret;
            dynamic table = new List<T>();
            Type E;
            if (typeof(T) == typeof(TcpEntry))
                E = typeof(MIB_TCP_ROW_EX);
            else
                E = typeof(MIB_UDP_ROW_EX);

            ptrTable = IntPtr.Zero;
            if (typeof(T) == typeof(TcpEntry))
                ret = AllocateAndGetTcpExTableFromStack(ref ptrTable, true, GetProcessHeap(), IntPtr.Zero, FAMILY_V4);
            else
                ret = AllocateAndGetUdpExTableFromStack(ref ptrTable, true, GetProcessHeap(), IntPtr.Zero, FAMILY_V4);

            if (ret == 0 && ptrTable != IntPtr.Zero)
            {
                int dwNbEntries = Marshal.ReadInt32(ptrTable);
                IntPtr ptr = new IntPtr(ptrTable.ToInt32() + 4);

                for (int i = 0; i <= dwNbEntries - 1; i++) // For each
                {
                    dynamic entry = Convert.ChangeType(Marshal.PtrToStructure(ptr, E), E);

                    if (typeof(T) == typeof(TcpEntry))
                        table.Add(new TcpEntry(entry.dwState, entry.dwRemoteAddr, entry.dwRemotePort, entry.dwLocalAddr, entry.dwLocalPort, entry.dwProcessId, this));
                    else
                        table.Add(new UdpEntry(entry.dwLocalAddr, entry.dwLocalPort, entry.dwProcessId));

                    ptr = new IntPtr(ptr.ToInt32() + Marshal.SizeOf(E));
                }

                // Free memory
                HeapFree(GetProcessHeap(), 0, ptrTable);
            }
            else
            {
                throw new System.ComponentModel.Win32Exception(ret);
            }

            return table;
        }


        // T : TcpEntry / UdpEntry
        private List<T> getOpenedPorts_NT_Get<T>()
            where T : Entry
        {
            IntPtr ptrTable;
            int outBufLen;
            int ret;
            dynamic table = new List<T>();
            Type E;
            if (typeof(T) == typeof(TcpEntry))
                E = typeof(MIB_TCP_ROW_EX);
            else
                E = typeof(MIB_UDP_ROW_EX);

            ptrTable = IntPtr.Zero;
            outBufLen = 0;
            if (typeof(T) == typeof(TcpEntry))
                ret = GetExtendedTcpTable(IntPtr.Zero, ref outBufLen, true, FAMILY_V4, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            else
                ret = GetExtendedUdpTable(IntPtr.Zero, ref outBufLen, true, FAMILY_V4, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);

            try
            {
                ptrTable = Marshal.AllocHGlobal(outBufLen);
                if (typeof(T) == typeof(TcpEntry))
                    ret = GetExtendedTcpTable(ptrTable, ref outBufLen, true, FAMILY_V4, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
                else
                    ret = GetExtendedUdpTable(ptrTable, ref outBufLen, true, FAMILY_V4, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);

                if (ret == 0) // Success
                {
                    int dwNbEntries = Marshal.ReadInt32(ptrTable);
                    IntPtr ptr = new IntPtr(ptrTable.ToInt32() + 4);

                    for (int i = 0; i <= dwNbEntries - 1; i++) // For each entry
                    {
                        // Get the values / content
                        dynamic entry = Convert.ChangeType(Marshal.PtrToStructure(ptr, E), E);
                        // Store it in an Entry class and in the list
                        if(typeof(T) == typeof(TcpEntry))
                            table.Add(new TcpEntry(entry.dwState, entry.dwRemoteAddr, entry.dwRemotePort, entry.dwLocalAddr, entry.dwLocalPort, entry.dwProcessId, this));
                        if (typeof(T) == typeof(UdpEntry))
                            table.Add(new UdpEntry(entry.dwLocalAddr, entry.dwLocalPort, entry.dwProcessId));

                        // Next entry
                        ptr = new IntPtr(ptr.ToInt32() + Marshal.SizeOf(E));
                    }
                }
                else
                {
                    throw new System.ComponentModel.Win32Exception(ret);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(ptrTable);
            }

            return table;
        }

        #endregion
    }
}
