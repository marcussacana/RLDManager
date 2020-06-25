namespace RMGUI {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.Strs = new System.Windows.Forms.ListBox();
            this.Dialog = new System.Windows.Forms.TextBox();
            this.MS = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MS.SuspendLayout();
            this.SuspendLayout();
            // 
            // Strs
            // 
            this.Strs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Strs.FormattingEnabled = true;
            this.Strs.Location = new System.Drawing.Point(9, 10);
            this.Strs.Margin = new System.Windows.Forms.Padding(2);
            this.Strs.Name = "Strs";
            this.Strs.Size = new System.Drawing.Size(447, 316);
            this.Strs.TabIndex = 0;
            this.Strs.SelectedIndexChanged += new System.EventHandler(this.Strs_SelectedIndexChanged);
            // 
            // Dialog
            // 
            this.Dialog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Dialog.Location = new System.Drawing.Point(9, 339);
            this.Dialog.Margin = new System.Windows.Forms.Padding(2);
            this.Dialog.Name = "Dialog";
            this.Dialog.Size = new System.Drawing.Size(447, 20);
            this.Dialog.TabIndex = 1;
            this.Dialog.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Dialog_KeyPress);
            // 
            // MS
            // 
            this.MS.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MS.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.findKeyToolStripMenuItem,
            this.cryptToolStripMenuItem});
            this.MS.Name = "MS";
            this.MS.Size = new System.Drawing.Size(181, 114);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // findKeyToolStripMenuItem
            // 
            this.findKeyToolStripMenuItem.Name = "findKeyToolStripMenuItem";
            this.findKeyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.findKeyToolStripMenuItem.Text = "Find Key";
            this.findKeyToolStripMenuItem.Click += new System.EventHandler(this.findKeyToolStripMenuItem_Click);
            // 
            // cryptToolStripMenuItem
            // 
            this.cryptToolStripMenuItem.Name = "cryptToolStripMenuItem";
            this.cryptToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.cryptToolStripMenuItem.Text = "Crypt";
            this.cryptToolStripMenuItem.Click += new System.EventHandler(this.cryptToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 366);
            this.ContextMenuStrip = this.MS;
            this.Controls.Add(this.Dialog);
            this.Controls.Add(this.Strs);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "RLD Manager GUI";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.MS.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox Strs;
        private System.Windows.Forms.TextBox Dialog;
        private System.Windows.Forms.ContextMenuStrip MS;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cryptToolStripMenuItem;
    }
}

