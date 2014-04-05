using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetworkAnalyzer;
using System.Net;

namespace NetworkAnalyzer_Dev
{
    public class Filter
    {
        public enum Distance
        {
            None,
            Local,
            Remote
        }


        private Filter()
        {
            this.AllValid = true;
        }
        public Filter(bool UDP, bool TCP,
            Nullable<int> Port, IPAddress IP,
            Distance distance)
        {
            this.Name = null;

            this.udp = UDP;
            this.tcp = TCP;

            this.port = Port;
            this.ip = IP;

            this.distance = distance;

            this.AllValid = false;
        }
        public Filter(string Name)
        {
            this.Name = Name.ToLower();

            this.udp = false;
            this.tcp = false;

            this.port = null;
            this.ip = null;

            this.distance = Distance.None;

            this.AllValid = false;
        }

        private static Filter _AllValidFilter = null;
        public static Filter AllValidFilter
        {
            get
            {
                if (_AllValidFilter == null)
                    _AllValidFilter = new Filter();
                return _AllValidFilter;
            }
        }

        private readonly bool AllValid;

        private readonly string Name;

        private readonly bool udp;
        private readonly bool tcp;

        private readonly Distance distance;

        private readonly Nullable<int> port;
        private readonly IPAddress ip;

        public bool isMatching(Entry entry)
        {
            if (AllValid)
                return true;

            if (Name != null)
            {
                return entry.ProcessName.ToLower().StartsWith(Name);
            }

            if (tcp && !(entry is TcpEntry))
                return false;

            if (udp && !(entry is UdpEntry))
                return false;

            TcpEntry tcp_entry = entry as TcpEntry;

            if (distance == Distance.None)
            {
                if (port != null && entry.LocalEndPoint.Port != port.Value &&
                    (tcp_entry == null || tcp_entry.RemoteEndPoint.Port != port.Value))
                    return false;
                
                if (ip != null && !entry.LocalEndPoint.Address.Equals(ip) &&
                    (tcp_entry == null || !tcp_entry.RemoteEndPoint.Address.Equals(ip)))
                    return false;
            }
            else
                if (distance == Distance.Local)
                {
                    if (port != null && entry.LocalEndPoint.Port != port.Value)
                        return false;

                    if (ip != null && !entry.LocalEndPoint.Address.Equals(ip))
                        return false;
                }
                else
                    if (distance == Distance.Remote)
                    {
                        if (port != null &&
                            (tcp_entry == null || tcp_entry.RemoteEndPoint.Port != port.Value))
                            return false;

                        if (ip != null &&
                            (tcp_entry == null || !tcp_entry.RemoteEndPoint.Address.Equals(ip)))
                            return false;
                    }

            return true;
        }
    }
}
