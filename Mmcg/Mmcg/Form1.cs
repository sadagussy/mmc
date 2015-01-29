﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Windows.Forms.Integration;

namespace Mmcg
{
    public partial class Form1 : Form
    {
        [DllImport("mmc.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe static extern int mmc_convert(string infilename, string outilename, [Out] StringBuilder msg);

        [DllImport("mmc.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe static extern int mmc_convert_string(string mml, string outilename, [Out] StringBuilder msg);

        [DllImport("mmc.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe static extern void mmc_version([Out] StringBuilder version);

        const string Version = "MusicMemoPad 0.0.0(alpha)";
        bool textChanged = false;
        bool onceOpenedOrSaved = false;
        string currentFilename = "";

        System.Diagnostics.Process player;
        bool playing = false;

        private void close_player()
        {
            try
            {
                if (playing) player.Kill();
            }
            catch (Exception)
            {
                ;
            }

        }

        private Form2 form2;
        private HelpForm helpForm;


        private void updateComboBox(){
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("[1]" + form2.getPlayer(1));
            toolStripComboBox1.Items.Add("[2]" + form2.getPlayer(2));
            toolStripComboBox1.Items.Add("[3]" + form2.getPlayer(3));
            toolStripComboBox1.Items.Add("[4]" + form2.getPlayer(4));
        }

        private void updateStatusBar()
        {            
            int index = textBox1.SelectionStart;
            int line = textBox1.GetLineFromCharIndex(index);
            Point point = textBox1.GetPositionFromCharIndex(index);
            point.X = 0;
            int column = index - textBox1.GetCharIndexFromPosition(point);
            toolStripStatusLabel1.Text = "("+(line+1)+","+(column+1)+")";
        }

        public Form1()
        {
            InitializeComponent();
            /*
            ICSharpCode.AvalonEdit.TextEditor te = new ICSharpCode.AvalonEdit.TextEditor();
            ElementHost host = new ElementHost();
            host.Dock = DockStyle.Fill;
            host.Child = te;
            this.Controls.Add(host);
            */
            form2 = new Form2();
            form2.Owner = this;

            updateComboBox();
            toolStripComboBox1.SelectedIndex = 0;
            updateStatusBar();
        }

        private void play_music()
        {
            int result;
            StringBuilder buf = new StringBuilder(256);
            result = mmc_convert_string(textBox1.Text, "temp.mid", buf);
            textBox2.Text = buf.ToString();
            if (result != 0) return;

            int selected = toolStripComboBox1.SelectedIndex;
            string option = form2.getArguments(selected + 1);
            string program_name = form2.getPlayer(selected + 1);
            string filename = "temp.mid";
            string fullpath_filename = "\"" + Path.GetFullPath(filename) + "\"";


            try
            {
                if (playing) player.Kill();
            }
            catch (Exception)
            {
                ;
            }
            player = new System.Diagnostics.Process();
            player.StartInfo.CreateNoWindow = true;
            player.StartInfo.UseShellExecute = true;
            player.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            player.StartInfo.FileName = "\"" + program_name + "\"";
            player.StartInfo.Arguments = option + " " + fullpath_filename;
            try {
                player.Start();
            }
            catch (Exception)
            {
                textBox2.Text += "Failed starting player.\r\n";
            }


            playing = true;
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            play_music();
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }



        private void stopButton_Click(object sender, EventArgs e)
        {
            close_player();  
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            StringBuilder buf = new StringBuilder(256);
            mmc_version(buf);
            MessageBox.Show(Version + "\n" + buf.ToString() + "\nCopyright (c) 2013 sada.gussy (sada dot gussy at gmail dot com)");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            close_player();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            play_music();

        }

 
        bool confirmDiscardOkay()
        {
            if (!textChanged) return true;
            DialogResult result = MessageBox.Show("Changes will be discarded.\nAre you sure?", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result != DialogResult.OK) return false; else return true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (confirmDiscardOkay() == false) return;
            openFileDialog1.Title = "Open";            
            openFileDialog1.FileName = "Untitled.mml";
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog1.Filter = "MML file|*.mml|All files(*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (Stream fileStream = openFileDialog1.OpenFile())
                {
                    StreamReader sr = new StreamReader(fileStream, true);
                    currentFilename = openFileDialog1.FileName;
                    textBox1.Text = sr.ReadToEnd();
                    onceOpenedOrSaved = true;
                    textChanged = false;
                    this.Text = Version + " " + currentFilename;
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (onceOpenedOrSaved)
            {
                using (Stream fileStream = new FileStream(currentFilename, FileMode.Create))
                using (StreamWriter sr = new StreamWriter(fileStream))
                {
                    sr.Write(textBox1.Text);
                }
                textChanged = false;
                onceOpenedOrSaved = true;
                currentFilename = saveFileDialog1.FileName;
                this.Text = Version + " " + currentFilename;
            }
            else
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Save As";
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog1.FileName = "Untitled.mml";
            saveFileDialog1.Filter = "MML file(.mml)|*.mml|All files(*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (Stream fileStream = saveFileDialog1.OpenFile())
                using (StreamWriter sr = new StreamWriter(fileStream))
                {
                    sr.Write(textBox1.Text);
                }
                textChanged = false;
                onceOpenedOrSaved = true;
                currentFilename = saveFileDialog1.FileName;
                this.Text = Version + " " + currentFilename;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (confirmDiscardOkay() == false) return;
            textBox1.Text = "";
            currentFilename = "";
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!confirmDiscardOkay()) return;
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!confirmDiscardOkay()) e.Cancel = true;
        }

        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {
  
            form2.ShowDialog();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.CanUndo)
            {
                textBox1.Undo();
                textBox1.ClearUndo();
            }
        }
        // cut
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != "") textBox1.Cut();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != "") textBox1.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text))
            {
                textBox1.Paste();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Cut();
            Clipboard.Clear();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            close_player();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem_Click(sender, e);
        }

        private void textBox1_CursorChanged(object sender, EventArgs e)
        {

        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.Text = Version + " " + currentFilename + "*";
            textChanged = true;
        }

        private void helpToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            helpForm = new HelpForm();
            helpForm.Owner = this;
            helpForm.ShowDialog();
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            updateStatusBar();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            updateStatusBar();
        }

    }
}
