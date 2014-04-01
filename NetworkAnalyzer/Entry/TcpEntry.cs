using System;
using System.Runtime.InteropServices;
using System.Net;

using System.Linq;

namespace NetworkAnalyzer
{
    public class TcpEntry : Entry, IEquatable<TcpEntry>
    {
        #region Import

        /// <summary>
        /// Close TCP
        /// </summary>
        /// <param name="pTcpRow"></param>
        /// <returns></returns>
        [DllImport("IPHLPAPI")]
        private static extern int SetTcpEntry(ref MIB_TCP_ROW_EX pTcpRow);

        #endregion

        public TcpEntry(TcpState state, UInt32 remoteAddr, int remotePort, UInt32 localAddress, int localPort, int processID, PortManager manager)
        {
            this.m_State = state;
            this.m_RemoteEndPoint = new IPEndPoint(remoteAddr, remotePort);
            this.ProcessID = processID;
            this.m_LocalEndPoint = new IPEndPoint(localAddress, localPort);
            this.manager = manager;
        }

        protected PortManager manager;

        /// <summary>
        /// Indicate if the port is already used by the process
        /// It needs a Refresh() of the manager
        /// </summary>
        /// <returns>Return true if the port is already used</returns>
        public bool IsAlive()
        {
            var entries = (from f in manager.TCPTable
                           where
                           (
                               f.ProcessID == this.ProcessID &&
                               f.m_LocalEndPoint == this.m_LocalEndPoint &&
                               f.m_ProcessName == this.m_ProcessName &&
                               f.m_RemoteEndPoint == this.m_RemoteEndPoint
                           )
                           select f);

            return (entries.Count() != 0);
        }

        #region State
        private TcpState m_State;

        /// <summary>
        /// State of the connexion
        /// </summary>
        public TcpState State
        {
            get { return m_State; }
        }
        #endregion

        #region RemoteEndPoint
        private IPEndPoint m_RemoteEndPoint;

        /// <summary>
        /// Remote IP Address and port
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return m_RemoteEndPoint; }
        }
        #endregion

        #region Close

        /// <summary>
        /// Close the connexion [Forced]
        /// </summary>
        public bool Close()
        {
            MIB_TCP_ROW_EX entry = new MIB_TCP_ROW_EX();

            entry.dwLocalAddr = BitConverter.ToUInt32(this.LocalEndPoint.Address.GetAddressBytes(), 0);
            entry.dwLocalPort = this.LocalEndPoint.Port;
            entry.dwProcessId = this.ProcessID;
            entry.dwRemoteAddr = BitConverter.ToUInt32(this.RemoteEndPoint.Address.GetAddressBytes(), 0);
            entry.dwRemotePort = this.RemoteEndPoint.Port;

            // To delete
            entry.dwState = TcpState.DELETE_TCB;

            // Request closure
            int ret = SetTcpEntry(ref entry);

            return (ret == 0);
        }

        #endregion

        #region Equal
        public bool Equals(TcpEntry o)
        {
            return this.ProcessID == o.ProcessID &&
                this.LocalEndPoint.Address.Equals(o.LocalEndPoint.Address) &&
                this.LocalEndPoint.Port == o.LocalEndPoint.Port &&
                this.RemoteEndPoint.Address.Equals(o.RemoteEndPoint.Address) &&
                this.RemoteEndPoint.Port == o.RemoteEndPoint.Port;
        }
        public bool Equals(Entry o)
        {
            return this.ProcessID == o.ProcessID &&
                this.LocalEndPoint.Address.Equals(o.LocalEndPoint.Address) &&
                this.LocalEndPoint.Port == o.LocalEndPoint.Port;
        }
        public override bool Equals(object obj)
        {
            if (obj is TcpEntry)
            {
                TcpEntry o = obj as TcpEntry;
                return o.Equals(this);
            }
            if (obj is Entry)
            {
                TcpEntry o = obj as TcpEntry;
                return o.Equals(this);
            }

            return false;
        }
        public override int GetHashCode()
        {
            return ProcessID * 3 + LocalEndPoint.GetHashCode() * 2 + RemoteEndPoint.GetHashCode();
        }

        public static bool operator ==(TcpEntry entry, TcpEntry entry2)
        {
            return entry.Equals(entry2);
        }
        public static bool operator !=(TcpEntry entry, TcpEntry entry2)
        {
            return !(entry == entry2);
        }
        #endregion

        public override string ToString()
        {
            return LocalEndPoint.Port + " : " + ProcessName;
        }
    }
}
