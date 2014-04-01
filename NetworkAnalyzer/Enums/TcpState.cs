



namespace NetworkAnalyzer
{
    public enum TcpState : int
    {
        CLOSE_WAIT  = 8,
        CLOSED      = 1,
        CLOSING     = 9,
        DELETE_TCB  = 12,
        ESTAB       = 5,
        FIN_WAIT1   = 6,
        FIN_WAIT2   = 7,
        LAST_ACK    = 10,
        LISTEN      = 2,
        SYN_RCVD    = 4,
        SYN_SENT    = 3,
        TIME_WAIT   = 11
    }
}
