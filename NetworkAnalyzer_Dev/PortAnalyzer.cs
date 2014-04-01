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
    public partial class PortAnalyzer : Form
    {
        public PortAnalyzer()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cProcessList1.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cProcessList1.Stop();
        }
    }
}
