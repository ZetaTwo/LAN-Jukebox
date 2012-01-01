using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LANJukebox
{
    public partial class LANPlayer : Form
    {
        AudioPlayer player = new AudioPlayer();
        internal AudioPlayer Player
        {
            get { return player; }
        }

        const string TEMP_DIR = "temp";
        HttpServer server = new HttpServer(TEMP_DIR);
        ListViewItem currentTrack;

        int historySize = 10;
        public int HistorySize
        {
            get { return historySize; }
        }

        public LANPlayer()
        {
            InitializeComponent();

            player.TrackEnd += new LANJukebox.TrackEnded(TrackEnded);

            server.NewSong += new FileUploaded(SongAdded);
            server.Start();
            toolStripStatusLabelStatus.Text = "Web interface running on port 3000.";
        }

        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            Player.SetVolume(trackBarVolume.Value);
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

        private void SongAdded(string fileName)
        {
            Song song = new Song(fileName);
            if (song != null)
            {
                ListViewItem songItem = new ListViewItem();
                songItem.Text = song.Tags.title;
                songItem.SubItems.Add(song.Tags.artist);
                songItem.SubItems.Add(song.Tags.album);

                DateTime duration = new DateTime((long)(song.Tags.duration * 10e6));
                songItem.SubItems.Add(duration.ToString("mm:ss"));

                songItem.Tag = song;

                AddSong(songItem);
            }
        }

        delegate void AddSongHandler(ListViewItem track);
        private void AddSong(ListViewItem track)
        {
            if (listViewTracks.InvokeRequired)
            {
                Invoke(new AddSongHandler(AddSong), track);
                return;
            }

            listViewTracks.Items.Add(track);
            if (listViewTracks.Items.Count == 1)
            {
                PlaySong(track);
            }

            if (listViewTracks.Items.Count > 1)
            {
                toolStripButtonNext.Enabled = true;
            }
        }

        delegate void TrackEndedHandler();
        private void TrackEnded()
        {
            if (currentTrack.ListView.InvokeRequired)
            {
                Invoke(new TrackEndedHandler(TrackEnded));
                return;
            }

            Player.Stop();
            if (!LastTrack(currentTrack.Index))
            {
                PlaySong(currentTrack.ListView.Items[currentTrack.Index + 1]);
            }
        }
        
        private void PlaySong(ListViewItem track)
        {
            if (currentTrack != null)
            {
                currentTrack.BackColor = Color.Empty;
            }
            currentTrack = track;
            currentTrack.BackColor = Color.Beige;

            Player.LoadSong((Song)currentTrack.Tag);
            Player.PlayPause();

            while (currentTrack.Index >= HistorySize)
            {
                DeleteTrack(0);
            }

            UpdatePlayerButtons();
        }

        private void UpdatePlayerButtons()
        {
            if (currentTrack != null)
            {
                toolStripButtonNext.Enabled = !LastTrack(currentTrack.Index);
                toolStripButtonPrev.Enabled = !FirstTrack(currentTrack.Index);
                toolStripButtonPlayPause.Enabled = true;
            }
            else
            {
                toolStripButtonNext.Enabled = false;
                toolStripButtonPrev.Enabled = false;
                toolStripButtonPlayPause.Enabled = false;
            }
        }

        private void MoveTrackTo(int from, int to)
        {
            ListViewItem track = listViewTracks.Items[from];
            listViewTracks.Items.RemoveAt(from);
            listViewTracks.Items.Insert(to, track);

            UpdatePlayerButtons();
        }

        private void DeleteTrack(int index)
        {
            ListViewItem track = listViewTracks.Items[index];
            if (track == currentTrack)
            {
                player.Stop();
                currentTrack = null;
            }
            listViewTracks.Items.RemoveAt(index);

            try
            {
                File.Delete(((Song)track.Tag).Tags.filename);
            }
            catch
            {

            }

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
            return (index != 0);
        }

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
                Player.CurrentDevice = options.Device;
                historySize = options.HistorySize;
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
            Player.PlayPause();
        }

        private void toolStripButtonNext_Click(object sender, EventArgs e)
        {
            Player.Stop();
            PlaySong(currentTrack.ListView.Items[currentTrack.Index + 1]);
        }

        private void toolStripButtonPrev_Click(object sender, EventArgs e)
        {
            Player.Stop();
            PlaySong(currentTrack.ListView.Items[currentTrack.Index - 1]);
        }

        private void toolStripButtonWebInterface_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://127.0.0.1:3000");
        }
    }
}
