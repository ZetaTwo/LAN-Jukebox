using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LANJukebox
{
    public partial class OptionsForm : Form
    {
        LANPlayer parent;
        internal AudioDevice Device
        {
            get { return (AudioDevice)comboBoxDevices.SelectedItem; }
        }

        internal int HistorySize
        {
            get { return (int)numericUpDownHistorySize.Value; }
        }

        public OptionsForm(LANPlayer _parent)
        {
            parent = _parent;
            
            InitializeComponent();

            //Populate devices
            AudioDevice[] devices = parent.Player.GetDevices();
            AudioDevice current_device = parent.Player.CurrentDevice;
            foreach (AudioDevice device in devices)
            {
                comboBoxDevices.Items.Add(device);
                if (device.Id == current_device.Id)
                {
                    comboBoxDevices.SelectedItem = device;
                }
            }

            //Set history size
            numericUpDownHistorySize.Value = parent.HistorySize;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
