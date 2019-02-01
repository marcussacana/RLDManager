using RLDManager;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        private void findKeyToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Title = "Select a Script...";
            fd.Filter = "All ExHIBIT Scripts File|*.rld";
            if (fd.ShowDialog() == DialogResult.OK) {
                MessageBox.Show("Please note, this is a brute force process,\nthe program will freeze for a time with a high CPU Usage\n(Maybe 2~3 hours is required.)", "RMGUI",MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                byte[] Script = System.IO.File.ReadAllBytes(fd.FileName);

                bool Failed = true;
                if (DoubleThreadBruteForce(Script)) {
                    try {
                        uint Key = this.Key.Last();
                        if (Key != 0) {
                            Failed = false;
                            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "RLDKeys.txt", "0x" + Key.ToString("X8"));
                            MessageBox.Show("The Key is: 0x" + Key.ToString("X8") + "\nKey Saved, Try open the script now.", "RMGui", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    } catch {

                    }
                } 

                if (Failed)
                    MessageBox.Show("Failed to Catch the key", "RMGui", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        uint[] Key = new uint[0];
        private bool DoubleThreadBruteForce(byte[] Script) {
            Thread T1 = new Thread(() => {
                uint[] Key = new uint[0];
                bool Sucess = RLD.FindKey(Script, out Key);
                if (Sucess)
                    this.Key = Key;
            });
            
            T1.Start();

            uint Dots = 2;
            while (T1.IsAlive) {
                Application.DoEvents();
                Thread.Sleep(400);
                Dots++;
                if (Dots == 3)
                    Dots = 0;
                switch (Dots) {
                    case 0:
                        Text = "Finding Key.   |" + RLD.FindProgress;
                        break;
                    case 1:
                        Text = "Finding Key..  |" + RLD.FindProgress;
                        break;
                    default:
                        Text = "Finding Key... |" + RLD.FindProgress;
                        break;
                }

            }

            if (Key.Length == 0)
                return false;
            return true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            Environment.Exit(0);
        }
    }
}
