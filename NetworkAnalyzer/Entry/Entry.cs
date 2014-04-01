using System.Diagnostics;
using System.Net;

namespace NetworkAnalyzer
{
    public class Entry
    {
        protected Entry()
        { }

        #region LocalEndPoint
        protected IPEndPoint m_LocalEndPoint;

        /// <summary>
        /// Local IP Address and port
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return m_LocalEndPoint; }
        }
        #endregion

        #region ProcessID
        private int m_ProcessID; // Private because of "set" in "int ProcessID"

        /// <summary>
        /// Get the PID of the process
        /// </summary>
        public int ProcessID
        {
            protected set
            {
                m_ProcessID = value;

                try
                {
                    m_Process = Process.GetProcessById(m_ProcessID);
                }
                catch
                { }
            }
            get
            {
                return m_ProcessID;
            }
        }
        #endregion

        #region Process
        protected Process m_Process;

        /// <summary>
        /// Get the process associated
        /// </summary>
        public Process Process
        {
            get { return m_Process; }
        }
        #endregion

        #region ProcessName
        protected string m_ProcessName = null;

        /// <summary>
        /// Name of the processus
        /// </summary>
        public string ProcessName
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_ProcessName))
                {
                    if (m_Process != null)
                        this.m_ProcessName = m_Process.ProcessName;
                }
                return this.m_ProcessName;
            }
        }
        #endregion
    }
}
