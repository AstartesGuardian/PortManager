using System;
using System.Net;

namespace NetworkAnalyzer
{
    public class UdpEntry : Entry
    {
        public UdpEntry(UInt32 localAddress, int localPort, int processID)
        {
            this.ProcessID = processID;
            this.m_LocalEndPoint = new IPEndPoint(localAddress, localPort);
        }

        #region Equal
        private static bool CheckSimilarities(UdpEntry entry1, UdpEntry entry2)
        {
            if (Object.ReferenceEquals(entry1, null)) // entry1 = null
                return Object.ReferenceEquals(entry2, null);
            else // entry1 != null
                if (Object.ReferenceEquals(entry2, null)) // entry2 = null
                    return false;

            // entry1 != null & entry2 != null

            return entry1.ProcessID == entry2.ProcessID &&
                entry1.LocalEndPoint.Address.Equals(entry2.LocalEndPoint.Address) &&
                entry1.LocalEndPoint.Port == entry2.LocalEndPoint.Port;
        }

        public bool Equals(UdpEntry o)
        {
            return CheckSimilarities(this, o);
        }
        public override bool Equals(object obj)
        {
            if (obj is UdpEntry)
                return CheckSimilarities(this, obj as UdpEntry);

            return false;
        }
        public override int GetHashCode()
        {
            return ProcessID * 3 + LocalEndPoint.GetHashCode() * 2;
        }

        public static bool operator ==(UdpEntry entry1, UdpEntry entry2)
        {
            return CheckSimilarities(entry1, entry2);
        }
        public static bool operator !=(UdpEntry entry1, UdpEntry entry2)
        {
            return !CheckSimilarities(entry1, entry2);
        }
        #endregion

        public override string ToString()
        {
            return LocalEndPoint.Port + " : " + ProcessName;
        }
    }
}
