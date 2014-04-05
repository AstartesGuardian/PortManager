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
            researcher = new Researcher();
            this.Controls.Add(researcher);

            InitializeComponent();

            this.Disposed += new EventHandler(CPortList_Disposed);

            pw = new PortWatcher();
            pw.NewEntry += new PortWatcher.PortWatcherEntriesEventHandler(pw_NewEntry);
            pw.RemovedEntry += new PortWatcher.PortWatcherEntriesEventHandler(pw_RemovedEntry);
            pw.Interval = 3000;


            researcher.Refresh += researcher_Refresh;

            researcher.Top = 50;
            researcher.Left = 0;
            researcher.Anchor = AnchorStyles.Left;
            researcher.Visible = false;
        }

        private void researcher_Refresh(object sender, EventArgs e)
        {
            Filter filter = ((Researcher)sender).CurrentFilter;

            listView1.Items.Clear();

            Entry[] entries;

            lock (pw.Manager)
                entries = pw.Manager.Table.ToArray();

            pw_NewEntryI(pw, entries);
        }

        protected Researcher researcher;

        protected PortWatcher pw;

        public PortManager Manager
        {
            get { return pw.Manager; }
        }

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

            Filter filter = researcher.CurrentFilter;

            foreach (Entry entry in entries)
                if (filter.isMatching(entry))
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
            Filter filter = researcher.CurrentFilter;

            foreach (Entry entry in entries)
                if (filter.isMatching(entry))
                    foreach (ListViewItem lvi in listView1.Items)
                    {
                        dynamic lviEntry;

                        if (lvi.Tag is TcpEntry)
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
            pw.Resume();
        }

        public void Stop()
        {
            pw.Pause();
        }

        private void CPortList_Disposed(object sender, EventArgs e)
        {
            try
            {
                pw.Stop();
            }
            catch
            { }
        }

        private void cms_port_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = listView1.SelectedIndices.Count == 0;
        }

        private void closePortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            TcpEntry tcp_entry = listView1.SelectedItems[0].Tag as TcpEntry;

            if (tcp_entry == null)
                return;

            tcp_entry.Close();
        }

        private void killProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            TcpEntry tcp_entry = listView1.SelectedItems[0].Tag as TcpEntry;

            if (tcp_entry == null || tcp_entry.Process == null)
                return;

            tcp_entry.Process.Kill();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            researcher.Hide();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            researcher.Show();
        }
    }
}
