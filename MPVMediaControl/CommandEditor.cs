using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPVMediaControl
{
    public partial class CommandEditor : Form
    {
        private static readonly string[] _defaultCommands = { "set_property pause false", "set_property pause true", "playlist-prev weak", "playlist-next weak" };

        public CommandEditor()
        {
            InitializeComponent();
        }

        private void CommandEditor_Load(object sender, EventArgs e)
        {
            playCmdText.Text = Properties.Settings.Default.PlayCommand;
            pauseCmdText.Text = Properties.Settings.Default.PauseCommand;
            prevCmdText.Text = Properties.Settings.Default.PrevCommand;
            nextCmdText.Text = Properties.Settings.Default.NextCommand;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            playCmdText.Text = _defaultCommands[0];
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pauseCmdText.Text = _defaultCommands[1];
        }

        private void button5_Click(object sender, EventArgs e)
        {
            prevCmdText.Text = _defaultCommands[2];
        }

        private void button6_Click(object sender, EventArgs e)
        {
            nextCmdText.Text = _defaultCommands[3];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PlayCommand = playCmdText.Text;
            Properties.Settings.Default.PauseCommand = pauseCmdText.Text;
            Properties.Settings.Default.PrevCommand = prevCmdText.Text;
            Properties.Settings.Default.NextCommand = nextCmdText.Text;

            Properties.Settings.Default.Save();
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
