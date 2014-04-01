using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NetworkAnalyzer;

namespace NetworkAnalyzer_Dev
{
    public partial class CPortList : UserControl
    {
        public CPortList()
        {
            InitializeComponent();

            pw = new PortWatcher();
            pw.NewEntry += new PortWatcher.PortWatcherEntriesEventHandler(pw_NewEntry);
            pw.RemovedEntry += new PortWatcher.PortWatcherEntriesEventHandler(pw_RemovedEntry);
            pw.Interval = 3000;
        }

        private PortWatcher pw;

        private void pw_NewEntry(PortWatcher pw, Entry[] entries)
        {
            try
            {
                Invoke(new PortWatcher.PortWatcherEntriesEventHandler(pw_NewEntryI), new object[] { pw, entries });
            }
            catch
            {
                pw.Stop();
            }
        }
        private void pw_RemovedEntry(PortWatcher pw, Entry[] entries)
        {
            try
            {
                Invoke(new PortWatcher.PortWatcherEntriesEventHandler(pw_RemovedEntryI), new object[] { pw, entries });
            }
            catch
            {
                pw.Stop();
            }
        }

        private void pw_NewEntryI(PortWatcher pw, Entry[] entries)
        {
            ListViewItem lvi;
            TcpEntry tcp_entry;

            foreach (Entry entry in entries)
            {
                lvi = new ListViewItem();

                lvi.SubItems.AddRange(new string[]
                    {
                        entry.ProcessName,
                        entry.LocalEndPoint.Address.ToString(),
                        entry.LocalEndPoint.Port.ToString()
                    });

                tcp_entry = entry as TcpEntry;
                if (tcp_entry != null) // TCP
                {
                    lvi.Text = "TCP";

                    lvi.SubItems.AddRange(new string[]
                    {
                        tcp_entry.RemoteEndPoint.Address.ToString(),
                        tcp_entry.RemoteEndPoint.Port.ToString()
                    });

                    lvi.BackColor = Color.LightSkyBlue;
                }
                else // UDP
                {
                    lvi.Text = "UDP";

                    lvi.BackColor = Color.LightSalmon;
                }

                lvi.Name = entry.ProcessName;
                lvi.Tag = entry;

                ListViewItem[] lvis = listView1.Items.Find(entry.ProcessName, false);
                if (lvis.Count() == 0)
                    listView1.Items.Add(lvi);
                else
                    listView1.Items.Insert(lvis[0].Index, lvi);
            }
        }


        private void pw_RemovedEntryI(PortWatcher pw, Entry[] entries)
        {
            foreach(Entry entry in entries)
                foreach (ListViewItem lvi in listView1.Items)
                {
                    dynamic lviEntry;

                    if(lvi.Tag is TcpEntry)
                        lviEntry = (TcpEntry)lvi.Tag;
                    else
                        lviEntry = (UdpEntry)lvi.Tag;

                    if (!(lviEntry is TcpEntry && entry is UdpEntry ||
                        lviEntry is UdpEntry && entry is TcpEntry))
                    {
                        if ((entry is TcpEntry ? (TcpEntry)entry == lviEntry : (UdpEntry)entry == lviEntry))
                        {
                            listView1.Items.Remove(lvi);
                            break;
                        }
                    }
                }
        }


        public void Start()
        {
            pw.Start();
        }

        public void Stop()
        {
            pw.Stop();
        }
    }
}
