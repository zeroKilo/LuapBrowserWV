using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using Microsoft.VisualBasic;

namespace LuapBrowserWV
{
    public partial class Form1 : Form
    {
        public LuaPackage luap;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            for (int i = 0; i < LuaOpcode.opcNames.Length; i++)
                comboBox1.Items.Add("[" + i.ToString("X2") + "] " + LuaOpcode.opcNames[i]);
            comboBox2.Items.Clear();
            comboBox2.Items.Add("NULL");
            comboBox2.Items.Add("BOOLEAN");
            comboBox2.Items.Add("RESERVED");
            comboBox2.Items.Add("NUMBER");
            comboBox2.Items.Add("STRING");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.luap|*.luap";
            if(d.ShowDialog() == DialogResult.OK)
            {
                luap = new LuaPackage(d.FileName);
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            if (luap == null)
                return;
            listBox1.Items.Clear();
            for (int i = 0; i < luap.entries.Count; i++)
                listBox1.Items.Add(i + " : " + luap.entries[i].script.mainFunc.source);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            hb1.ByteProvider = new DynamicByteProvider(luap.entries[n].rawData);
            rtb1.Text = luap.entries[n].script.mainFunc.Dump(0);
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("MainFunc");
            for (int i = 0; i < luap.entries[n].script.mainFunc.subFunc.Count; i++)
                toolStripComboBox1.Items.Add("SubFunc #" + (i + 1));
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void exportRawHexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if(d.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(d.FileName, luap.entries[n].rawData);
                MessageBox.Show("Done.");
            }
        }

        private void saveAsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (luap == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.luap|*.luap";
            if (d.ShowDialog() == DialogResult.OK)
            {
                luap.Save(d.FileName);
                MessageBox.Show("Done.");
            }
        }

        private void RefreshFunction()
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            if (n == -1 || m == -1) 
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            listBox2.Items.Clear();
            for (int i = 0; i < func.constants.Count; i++)
                listBox2.Items.Add((i + 1) + " : " + func.constants[i]);
            listBox3.Items.Clear();
            for (int i = 0; i < func.upVars.Count; i++)
                listBox3.Items.Add((i + 1) + " : " + func.upVars[i]);
            listBox4.Items.Clear();
            for (int i = 0; i < func.byteCode.Count; i++)
                listBox4.Items.Add("<" + (i + 1).ToString("D4") + "> : " + func.byteCode[i].ToString("X8") + " " + new LuaOpcode(func.byteCode[i]).Print(i + 1, func));
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshFunction();
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            int o = listBox4.SelectedIndex;
            if (n == -1 || m == -1 || o == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            LuaOpcode op = new LuaOpcode(func.byteCode[o]);
            comboBox1.SelectedIndex = (int)op.ID;
            textBox1.Text = func.byteCode[o].ToString("X8");
            textBox2.Text = op.A.ToString();
            textBox3.Text = op.B.ToString();
            textBox4.Text = op.C.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            int o = listBox4.SelectedIndex;
            if (n == -1 || m == -1 || o == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            func.byteCode[o] = Convert.ToUInt32(textBox1.Text.Trim(), 16);
            RefreshFunction();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            int o = listBox4.SelectedIndex;
            if (n == -1 || m == -1 || o == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            uint ID = (uint)comboBox1.SelectedIndex;
            uint A = Convert.ToUInt32(textBox2.Text.Trim());
            uint B = Convert.ToUInt32(textBox3.Text.Trim());
            uint C = Convert.ToUInt32(textBox4.Text.Trim());
            func.byteCode[o] = ID | (A << 6) | (C << 14) | (B << 23);
            RefreshFunction();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            if (n == -1 || m == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            func.byteCode.Add(0x1E);
            RefreshFunction();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            int o = listBox4.SelectedIndex;
            if (n == -1 || m == -1 || o == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            func.byteCode.RemoveAt(o);
            RefreshFunction();
        }

        private void addToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            if (n == -1 || m == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            string s = GetStringInput("");
            if (s != "")
                func.upVars.Add(s);
            RefreshFunction();
        }

        private string GetStringInput(string def)
        {
            return Interaction.InputBox("Enter String", "Enter String", def);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditUpVal();
        }

        private void listBox3_DoubleClick(object sender, EventArgs e)
        {
            EditUpVal();
        }

        private void EditUpVal()
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            int o = listBox3.SelectedIndex;
            if (n == -1 || m == -1 || o == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            string s = GetStringInput(func.upVars[o]);
            if (s != "")
                func.upVars[o] = s;
            RefreshFunction();
        }

        private void removeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            int o = listBox3.SelectedIndex;
            if (n == -1 || m == -1 || o == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            func.upVars.RemoveAt(o);
            RefreshFunction();
        }

        private void addToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            if (n == -1 || m == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            func.constants.Add(new LuaConstant(LuaConstant.TYPE.STRING, ""));
            RefreshFunction();
        }

        private void removeToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            int o = listBox2.SelectedIndex;
            if (n == -1 || m == -1 || o == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            func.constants.RemoveAt(o);
            RefreshFunction();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            int o = listBox2.SelectedIndex;
            if (n == -1 || m == -1 || o == -1)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            comboBox2.SelectedIndex = (int)func.constants[o].type;
            if (func.constants[o].type != LuaConstant.TYPE.NULL)
                textBox5.Text = func.constants[o].value.ToString();
            else
                textBox5.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = toolStripComboBox1.SelectedIndex;
            int o = listBox2.SelectedIndex;
            if (n == -1 || m == -1 || o == -1 || comboBox2.SelectedIndex == 2)
                return;
            LuaFunction func;
            if (m == 0)
                func = luap.entries[n].script.mainFunc;
            else
                func = luap.entries[n].script.mainFunc.subFunc[m - 1];
            func.constants[o].type = (LuaConstant.TYPE)comboBox2.SelectedIndex;
            switch(comboBox2.SelectedIndex)
            {
                case 0:
                case 2:
                    func.constants[o].value = null;
                    break;
                case 1:
                    func.constants[o].value = Convert.ToBoolean(textBox5.Text);
                    break;
                case 3:
                    func.constants[o].value = Convert.ToDouble(textBox5.Text);
                    break;
                case 4:
                    func.constants[o].value = textBox5.Text;
                    break;
            }
            RefreshFunction();
        }
    }
}
