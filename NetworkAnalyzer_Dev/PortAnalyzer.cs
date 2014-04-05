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

        private void cb_analyzer_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_analyzer.Checked)
                cProcessList1.Start();
            else
                cProcessList1.Stop();
        }
    }
}
