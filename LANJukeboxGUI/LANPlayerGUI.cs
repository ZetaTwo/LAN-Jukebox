using System;
using System.Windows.Forms;
using LANJukebox;
using System.Drawing;
using EQATEC.Analytics.Monitor;

namespace LANJukeboxGUI
{
    public partial class LANPlayer : Form
    {
        LANPlayerCore player;
        public LANPlayerCore Player
        {
            get { return player; }
        }

#if !DEBUG
        IAnalyticsMonitor monitor;
#endif

        public LANPlayer()
        {
            InitializeComponent();

#if !DEBUG
            monitor = AnalyticsMonitorFactory.Create("3C33D1DC98B34193B7B7C99E484C4583");
            AppDomain.CurrentDomain.UnhandledException += (s, exep) =>
                monitor.TrackException(exep.ExceptionObject as Exception);
            monitor.Start();
#endif

            string lastFmSession = Properties.Settings.Default.LastFmSession;
            if (!string.IsNullOrWhiteSpace(lastFmSession))
            {
                player = new LANPlayerCore(lastFmSession);
            }
            else
            {
                player = new LANPlayerCore();
            }
            Player.TrackAdd += new LANJukebox.TrackEvent(AddTrack);
            Player.TrackPlay += new TrackEvent(PlayTrack);
            Player.TrackDelete += new TrackEvent(DeleteTrackHistory);
            Player.Audio.DeviceIndex = Properties.Settings.Default.AudioDevice;
            Player.HistorySize = Properties.Settings.Default.HistorySize;
            
            toolStripStatusLabelStatus.Text = "Web interface running on port 3000.";
        }

        private void UpdatePlayerButtons()
        {
            if (Player.Audio.Playing)
            {
                toolStripButtonNext.Enabled = !Player.LastTrack();
                toolStripButtonPrev.Enabled = !Player.FirstTrack();
                toolStripButtonPlayPause.Enabled = true;
            }
            else
            {
                toolStripButtonNext.Enabled = false;
                toolStripButtonPrev.Enabled = false;
                toolStripButtonPlayPause.Enabled = false;
            }
        }

        #region Track

        delegate void TrackHandler(Track track);
        private void AddTrack(Track track)
        {
            if (listViewTracks.InvokeRequired)
            {
                Invoke(new TrackHandler(AddTrack), track);
                return;
            }

            //Create list item
            ListViewItem trackItem = new ListViewItem();
            trackItem.Text = track.Tags.title;
            trackItem.SubItems.Add(track.Tags.artist);
            trackItem.SubItems.Add(track.Tags.album);

            //Format duration
            DateTime duration = new DateTime((long)(track.Tags.duration * 10e6));
            trackItem.SubItems.Add(duration.ToString("mm:ss"));

            //Add connection to track
            trackItem.Tag = track;

            //Add to list
            listViewTracks.Items.Add(trackItem);

            //If there are more tracks
            if (listViewTracks.Items.Count > 1)
            {
                toolStripButtonNext.Enabled = true;
            }
        }

        private void PlayTrack(Track track)
        {
            if (listViewTracks.InvokeRequired)
            {
                Invoke(new TrackHandler(PlayTrack), track);
                return;
            }

            foreach (ListViewItem trackItem in listViewTracks.Items)
            {
                if(trackItem.Tag == track)
                {
                    trackItem.BackColor = Color.Olive;
                } else
                {
                    trackItem.BackColor = Color.Transparent;
                }
            }
            

            UpdatePlayerButtons();
            toolStripStatusLabelStatus.Text = "Playing " + track.Tags.artist + " - " + track.Tags.title;
        }

        private void MoveTrackTo(int from, int to)
        {
            Player.MoveTrack(from, to);

            ListViewItem track = listViewTracks.Items[from];
            listViewTracks.Items.RemoveAt(from);
            listViewTracks.Items.Insert(to, track);

            UpdatePlayerButtons();
        }

        private void DeleteTrackHistory(Track track)
        {
            if (listViewTracks.InvokeRequired)
            {
                Invoke(new TrackHandler(DeleteTrackHistory), track);
                return;
            }

            foreach (ListViewItem trackItem in listViewTracks.Items)
            {
                if (trackItem.Tag == track)
                {
                    trackItem.Remove();
                    return;
                }
            }
        }

        private void DeleteTrack(int index)
        {
            ListViewItem track = listViewTracks.Items[index];
            listViewTracks.Items.RemoveAt(index);
            Player.DeleteTrack(index);

            //Select next track in list
            if (listViewTracks.Items.Count > 0)
            {
                if (index >= listViewTracks.Items.Count)
                {
                    listViewTracks.SelectedIndices.Add(listViewTracks.Items.Count - 1);
                }
                else
                {
                    listViewTracks.SelectedIndices.Add(index);
                }
            }
        }

        private bool LastTrack(int index)
        {
            return (index == listViewTracks.Items.Count - 1);
        }

        private bool FirstTrack(int index)
        {
            return (index == 0);
        }

        #endregion

        #region UIHandlers

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm options = new OptionsForm(this);
            DialogResult result = options.ShowDialog();
            if (result == DialogResult.OK)
            {
                //Set options
                Player.Audio.CurrentDevice = options.Device;
                Player.HistorySize = options.HistorySize;

                //Save options
                Properties.Settings.Default.AudioDevice = Player.Audio.DeviceIndex;
                Properties.Settings.Default.HistorySize = Player.HistorySize;
                Properties.Settings.Default.LastFmSession = options.LastFmSession;
                Properties.Settings.Default.Save();
            }
        }

        private void toolStripButtonUp_Click(object sender, EventArgs e)
        {
            int index = listViewTracks.SelectedIndices[0];
            MoveTrackTo(index, index - 1);
        }

        private void toolStripButtonDown_Click(object sender, EventArgs e)
        {
            int index = listViewTracks.SelectedIndices[0];
            MoveTrackTo(index, index + 1);
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            int index = listViewTracks.SelectedIndices[0];
            DeleteTrack(index);
            UpdatePlayerButtons();
        }

        private void toolStripButtonPlayPause_Click(object sender, EventArgs e)
        {
            Player.Audio.PlayPause();

            if (Player.Audio.Playing)
            {
                toolStripButtonPlayPause.ImageIndex = 1;
                toolStripButtonPlayPause.Text = "Pause";
            }
            else
            {
                toolStripButtonPlayPause.ImageIndex = 0;
                toolStripButtonPlayPause.Text = "Play";
            }
        }

        private void toolStripButtonNext_Click(object sender, EventArgs e)
        {
            Player.Next();
        }

        private void toolStripButtonPrev_Click(object sender, EventArgs e)
        {
            Player.Previous();
        }

        private void toolStripButtonWebInterface_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://127.0.0.1:3000");
        }

        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            Player.Audio.Volume = trackBarVolume.Value;
        }

        private void listViewTracks_SelectedIndexChanged(object sender, EventArgs e)
        {
            //If a track i selected
            if (listViewTracks.SelectedIndices.Count > 0)
            {
                //Enable the correct buttons
                int index = listViewTracks.SelectedIndices[0];
                toolStripButtonUp.Enabled = !FirstTrack(index);
                toolStripButtonDown.Enabled = !LastTrack(index);
                toolStripButtonDelete.Enabled = true;
            }
            else
            {
                toolStripButtonUp.Enabled = false;
                toolStripButtonDown.Enabled = false;
                toolStripButtonDelete.Enabled = false;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
        }

        #endregion

        private void LANPlayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Player.Audio.Stop();
#if !DEBUG
            monitor.Stop();
#endif
        }
    }
}
