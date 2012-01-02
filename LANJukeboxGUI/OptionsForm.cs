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

        }
    }
}
