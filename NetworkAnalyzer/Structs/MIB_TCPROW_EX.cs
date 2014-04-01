using System;



namespace NetworkAnalyzer
{
    struct MIB_TCP_ROW_EX
    {
        public TcpState dwState;

        public UInt32   dwLocalAddr;
        public int      dwLocalPort;

        public UInt32   dwRemoteAddr;
        public int      dwRemotePort;

        public int      dwProcessId;
    }
}
