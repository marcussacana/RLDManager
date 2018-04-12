using RLDManager;
using System;
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
                MessageBox.Show("Please note, this is a brute force process,\nthe program will freeze for a time with a high CPU Usage\n(Maybe 1~2 hours is required.)", "RMGUI",MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                byte[] Script = System.IO.File.ReadAllBytes(fd.FileName);

                System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "RLD KEY.txt", Key.ToString("X8"));
                if (DoubleThreadBruteForce(Script)) 
                    MessageBox.Show("The Key is: 0x" + Key.ToString("X8") + "\nKey Saved, you can open your rld script now.", "RMGui", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Failed to Catch the key", "RMGui", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        uint Key = 0;
        private bool DoubleThreadBruteForce(byte[] Script) {
            Thread T1 = new Thread(() => {
                uint Key = 0;
                bool Sucess = RLD.FindKey(Script, out Key, false);
                if (Sucess)
                    this.Key = Key;
                else
                    Key = uint.MaxValue;
            });
            Thread T2 = new Thread(() => {
                uint Key = 0;
                bool Sucess = RLD.FindKey(Script, out Key, true);
                if (Sucess)
                    this.Key = Key;
                else
                    Key = uint.MaxValue;
            });
            T1.Start();
            T2.Start();
            uint t = 400;
            uint Dots = 2;
            while (Key == 0) {
                Application.DoEvents();
                Thread.Sleep(100);
                t -= 100;
                if (t <= 0) {
                    t = 400;
                    Dots++;
                    if (Dots == 3)
                        Dots = 0;
                    switch (Dots) {
                        case 0:
                            Text = "Finding Key.";
                            break;
                        case 1:
                            Text = "Finding Key..";
                            break;
                        default:
                            Text = "Finding Key...";
                            break;
                    }
                }
            }
            if (T1.IsAlive)
                T1.Abort();
            if (T2.IsAlive)
                T2.Abort();
            if (Key == uint.MaxValue)
                return false;
            return true;
        }
    }
}
