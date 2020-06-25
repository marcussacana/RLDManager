using RLDManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RMGUI
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            foreach (var Pair in RLD.KnowedKeys)
            {
                comboBox1.Items.Add(Pair.Key);
            }
            comboBox1.Items.Add("Other...");
        }

        public uint Key;
        bool Other;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Other = comboBox1.SelectedItem.ToString() == "Other...";
            textBox1.Text = "";
            textBox1.Enabled = Other;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = DialogResult.OK;
            if (Other)
            {
                if (textBox1.Text.StartsWith("0x"))
                {
                    string Hex = textBox1.Text.Substring(2, textBox1.Text.Length - 2);
                    Key = Convert.ToUInt32(Hex, 16);
                }
                else
                {
                    if (!uint.TryParse(textBox1.Text, out Key))
                        DialogResult = DialogResult.Cancel;
                }
                return;
            }
            try
            {
                Key = RLD.KnowedKeys[comboBox1.SelectedItem.ToString()];
            }
            catch
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
