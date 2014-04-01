using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NetworkAnalyzer;

using System.Net;
using System.Net.Sockets;

namespace NetworkAnalyzer_Dev
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            pw = new PortWatcher();
            //pw.Invoker = this;
            pw.NewTCPEntry += new PortWatcher.PortWatcherTCPEntriesEventHandler(pw_NewEntry);
        }

        TcpListener tcp;
        //PortManager op;
        PortWatcher pw;
        private void pw_NewEntry(PortWatcher pw, TcpEntry[] entries)
        {
            Invoke(new PortWatcher.PortWatcherTCPEntriesEventHandler(a), new object[] { pw, entries });
        }

        private void a(PortWatcher pw, TcpEntry[] entries)
        {
            listBox1.Items.Clear();
            listBox1.Items.AddRange(entries);
        }

        const int destPort = 1700;

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();

            pw.Start();
            /*

            op = new PortManager();
            
            Text = (from f in op.TCPTable orderby f.LocalEndPoint.Port select f).Count().ToString();
            foreach (TcpEntry entry in from f in op.TCPTable orderby f.LocalEndPoint.Port select f)
                listBox1.Items.Add(entry);
            foreach (TcpEntry entry in from f in op.TCPTable orderby f.RemoteEndPoint.Port select f)
                listBox2.Items.Add(entry.RemoteEndPoint.Port + " " + entry.ProcessName);*/
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {/*
            TcpClient c = new TcpClient("localHost", destPort);
            NetworkStream s = c.GetStream();
            System.Threading.Thread.Sleep(30000);
            s.WriteByte(10);
            s.Close();
            c.Close();*/
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int port = int.Parse(textBox1.Text);
            /*
            TcpEntry entry = (from f in op.TCPTable where f.LocalEndPoint.Port == port select f).First();

            Text = entry.IsAlive().ToString() + " : ";
            if (entry != null)
            {
                entry.Close();
                op.Refresh();
                Text += entry.IsAlive().ToString();
            }*/
        }
    }
}
