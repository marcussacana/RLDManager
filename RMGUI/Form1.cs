using RLDManager;
using System;
using System.Linq;
using System.Windows.Forms;

namespace RMGUI {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        RLD rld;
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Title = "Select a Script...";
            fd.Filter = "All ExHIBIT Scripts File|*.rld";
            if (fd.ShowDialog() == DialogResult.OK) {
                rld = new RLD(System.IO.File.ReadAllBytes(fd.FileName));
                string[] STR = rld.Import();
                Strs.Items.Clear();
                foreach (string Dialog in STR)
                    Strs.Items.Add(Dialog);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog fd = new SaveFileDialog();
            fd.Title = "Save As...";
            fd.Filter = "All ExHIBIT Scripts File|*.rld";
            if (fd.ShowDialog() == DialogResult.OK) {
                string[] STR = new string[Strs.Items.Count];
                for (int i = 0; i < STR.Length; i++) {
                    STR[i] = Strs.Items[i].ToString();
                }
                System.IO.File.WriteAllBytes(fd.FileName, rld.Export(STR));
                MessageBox.Show("File Saved.");
            }
        }

        private void Strs_SelectedIndexChanged(object sender, EventArgs e) {
            if (Strs.SelectedIndex < 0)
                return;
            Dialog.Text = Strs.Items[Strs.SelectedIndex].ToString();
            Text = "ID: " + Strs.SelectedIndex;
        }

        private void Dialog_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r' || e.KeyChar == '\n') {
                Strs.Items[Strs.SelectedIndex] = Dialog.Text;
                Strs.SelectedIndex++;
            }
        }
    }
}