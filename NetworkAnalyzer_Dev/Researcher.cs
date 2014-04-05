using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Text.RegularExpressions;
using System.Net;

namespace NetworkAnalyzer_Dev
{
    public partial class Researcher : UserControl
    {
        public Researcher()
        {
            InitializeComponent();

            const string numIp = @"([01]?\d\d?|2[0-4]\d|25[0-5])";
            regex_ip_port = new Regex(@"^(\s*)(?<type>([Uu][Dd][Pp])|([Tt][Cc][Pp]))?(\s*)(?<dist>[LlRrDd])?(\s*)(?<ip>(?<ip1>" + numIp + @")\.(?<ip2>" + numIp + @")\.(?<ip3>" + numIp + @")\.(?<ip4>" + numIp + @"))?(\s*)(:)?(\s*)(?<port>[0-9]*)?(\s*)$");

            regex_name = new Regex(@"^(\s*)[Nn][Aa][Mm][Ee](\s*)=(\s*)(?<name>.+)$");

            _CurrentFilter = Filter.AllValidFilter;

            this.BackColor = color;
        }

        private readonly Color color = Color.FromArgb(0, 186, 228);

        private readonly Regex regex_ip_port;
        private readonly Regex regex_name;

        public event EventHandler Refresh;
        private void Invoke_Refresh()
        {
            if (Refresh != null)
                Refresh(this, new EventArgs());
        }

        private Filter _CurrentFilter;
        public Filter CurrentFilter
        {
            get { return _CurrentFilter; }
            protected set
            {
                _CurrentFilter = value;
                Invoke_Refresh();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string text = textBox1.Text.Trim();

            if (text.Length == 0)
            {
                CurrentFilter = Filter.AllValidFilter;
                this.BackColor = color;
            }
            else
            {
                Filter f = getFilter(text);
                if (f != null)
                {
                    CurrentFilter = f;

                    this.BackColor = color;
                }
                else
                    this.BackColor = Color.Salmon;
            }
        }

        private Filter getFilter(string str)
        {
            Match m = regex_ip_port.Match(str);
            if(m.Success)
            {
                string type = m.Groups["type"].Value;
                string dist = m.Groups["dist"].Value;
                string ip = m.Groups["ip"].Value;
                string port = m.Groups["port"].Value;


                Nullable<int> null_port = null;
                IPAddress ipaddress = null;
                Filter.Distance edist = Filter.Distance.None;
                bool isUDP = type.Length > 0 ? type.ToLower() == "udp" : false;
                bool isTCP = type.Length > 0 ? type.ToLower() == "tcp" : false;

                if(ip.Length > 0)
                {
                    ipaddress = new IPAddress(new byte[]
                    {
                        byte.Parse(m.Groups["ip1"].Value),
                        byte.Parse(m.Groups["ip2"].Value),
                        byte.Parse(m.Groups["ip3"].Value),
                        byte.Parse(m.Groups["ip4"].Value)
                    });
                }

                if (dist.Length > 0)
                {
                    switch(dist.ToLower())
                    {
                        case "l":
                            edist = Filter.Distance.Local;
                            break;

                        case "d":
                        case "r":
                            edist = Filter.Distance.Remote;
                            break;
                    }
                }

                if(port.Length > 0)
                {
                    null_port = new Nullable<int>(int.Parse(port));
                }

                return new Filter(isUDP, isTCP,
                    null_port,
                    ipaddress,
                    edist);
            }
            else
            {
                m = regex_name.Match(str);

                if(m.Success)
                {
                    return new Filter(m.Groups["name"].Value);
                }
            }

            return null;
        }

        private void Researcher_BackColorChanged(object sender, EventArgs e)
        {
            textBox1.BackColor = this.BackColor;
        }

        private void Researcher_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
                textBox1.Focus();
        }
    }
}
