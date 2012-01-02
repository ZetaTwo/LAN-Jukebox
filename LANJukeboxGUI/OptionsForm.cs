using System;
using System.Windows.Forms;
using LANJukebox;

namespace LANJukeboxGUI
{
    public partial class OptionsForm : Form
    {
        LANPlayer parent;
        internal AudioDevice Device
        {
            get { return (AudioDevice)comboBoxDevices.SelectedItem; }
        }

        internal uint HistorySize
        {
            get { return (uint)numericUpDownHistorySize.Value; }
        }

        string lastFmSession;
        public string LastFmSession
        {
            get { return lastFmSession; }
        }

        public OptionsForm(LANPlayer _parent)
        {
            parent = _parent;
            
            InitializeComponent();

            //Populate devices
            AudioDevice[] devices = parent.Player.Audio.GetDevices();
            AudioDevice current_device = parent.Player.Audio.CurrentDevice;
            foreach (AudioDevice device in devices)
            {
                comboBoxDevices.Items.Add(device);
                if (device.Id == current_device.Id)
                {
                    comboBoxDevices.SelectedItem = device;
                }
            }

            //Set history size
            numericUpDownHistorySize.Value = parent.Player.HistorySize;

            //Set Last.Fm auth
            if (!string.IsNullOrWhiteSpace(parent.Player.LastFmSession))
            {
                buttonLastFm.Text = "Revoke";
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonLastFm_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(parent.Player.LastFmSession))
            {
                parent.Player.RemoveLastFmAuth();
                lastFmSession = null;
                buttonLastFm.Text = "Authenticate";
            }
            else
            {
                System.Diagnostics.Process.Start(parent.Player.BeginLastFmAuth());
                TryAuth();
            }
        }

        private void TryAuth()
        {
            this.Enabled = false;
            DialogResult authResult = MessageBox.Show("Have you authenticated with Last.Fm?", "Authenticate Last.fm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (authResult == DialogResult.Yes)
            {
                try
                {
                    lastFmSession = parent.Player.EndLastFmAuth();
                    buttonLastFm.Text = "Revoke";
                }
                catch
                {
                    TryAuth();
                }
            }

            this.Enabled = true;
        }
    }
}
