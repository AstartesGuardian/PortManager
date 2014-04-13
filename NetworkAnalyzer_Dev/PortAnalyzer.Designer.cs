namespace NetworkAnalyzer_Dev
{
    partial class PortAnalyzer
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortAnalyzer));
            this.cb_analyzer = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cProcessList1 = new NetworkAnalyzer_Dev.CPortList();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cb_analyzer
            // 
            this.cb_analyzer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_analyzer.AutoSize = true;
            this.cb_analyzer.BackColor = System.Drawing.Color.White;
            this.cb_analyzer.Location = new System.Drawing.Point(596, 5);
            this.cb_analyzer.Name = "cb_analyzer";
            this.cb_analyzer.Size = new System.Drawing.Size(63, 17);
            this.cb_analyzer.TabIndex = 8;
            this.cb_analyzer.Text = "Analyze";
            this.cb_analyzer.UseVisualStyleBackColor = false;
            this.cb_analyzer.CheckedChanged += new System.EventHandler(this.cb_analyzer_CheckedChanged);
            // 
            // cProcessList1
            // 
            this.cProcessList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cProcessList1.Location = new System.Drawing.Point(0, 0);
            this.cProcessList1.Name = "cProcessList1";
            this.cProcessList1.Size = new System.Drawing.Size(681, 367);
            this.cProcessList1.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(580, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(13, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "?";
            this.toolTip1.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
            // 
            // PortAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(681, 367);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_analyzer);
            this.Controls.Add(this.cProcessList1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PortAnalyzer";
            this.Text = "Port Analyzer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CPortList cProcessList1;
        private System.Windows.Forms.CheckBox cb_analyzer;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
    }
}

