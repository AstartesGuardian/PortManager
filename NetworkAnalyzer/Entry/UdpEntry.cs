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


        public override string ToString()
        {
            return LocalEndPoint.Port + " : " + ProcessName;
        }
    }
}
