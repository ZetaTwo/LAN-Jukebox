using System.Collections.Generic;
using Lpfm.LastFmScrobbler;

namespace LANJukebox
{
    public delegate void TrackEvent(Track track);

    public class LANPlayerCore
    {
        AudioPlayer player = new AudioPlayer();
        public AudioPlayer Audio
        {
            get { return player; }
        }

        uint historySize = 10;
        public uint HistorySize
        {
            get { return historySize; }
            set { historySize = value; }
        }

        List<Track> Playlist = new List<Track>();
        Track currentTrack;

        HttpServer server = new HttpServer();

        public event TrackEvent TrackAdd;
        public event TrackEvent TrackPlay;

        public LANPlayerCore()
        {
            player.TrackEnd += new TrackEvent(TrackEnded);

            server.NewSong += new FileUploaded(TrackAdded);
            server.Start();
        }

        public void Next()
        {
            int currentTrackIndex = Playlist.IndexOf(currentTrack);
            Track nextTrack = Playlist[currentTrackIndex + 1];
            PlayTrack(nextTrack);
        }

        public void Previous()
        {
            int currentTrackIndex = Playlist.IndexOf(currentTrack);
            Track nextTrack = Playlist[currentTrackIndex - 1];
            PlayTrack(nextTrack);
        }

        private void TrackAdded(TempFile tempFile)
        {
            Track track = new Track(tempFile);
            if (track != null && track.Tags != null)
            {
                Playlist.Add(track);

                TrackAdd(track);
            }
        }

        private void TrackEnded(Track track)
        {
            Audio.Stop();
            int currentTrackIndex = Playlist.IndexOf(currentTrack);
            if (!LastTrack())
            {
                PlayTrack(Playlist[currentTrackIndex + 1]);
            }
        }

        private void PlayTrack(Track track)
        {
            if (currentTrack != null)
            {
                Audio.Stop();
            }

            currentTrack = track;

            Audio.LoadSong(currentTrack);
            Audio.PlayPause();

            while (Playlist.IndexOf(currentTrack) >= HistorySize)
            {
                DeleteTrack(0);
            }

            TrackPlay(track);
        }

        public void MoveTrack(int from, int to)
        {
            Track track = Playlist[from];
            Playlist.RemoveAt(from);
            Playlist.Insert(to, track);
        }

        public void DeleteTrack(int index)
        {
            //If it was the current track
            if (index == Playlist.IndexOf(currentTrack))
            {
                //Stop track
                player.Stop();
            }

            //Get track and remove it from playlist
            Track track = Playlist[index];
            Playlist.RemoveAt(index);

            //Select next track in list
            if (Playlist.Count > 0)
            {
                Next();
            }
            else
            {
                currentTrack = null;
            }
        }

        public bool LastTrack()
        {
            return (Playlist.IndexOf(currentTrack) == Playlist.Count - 1);
        }

        public bool FirstTrack()
        {
            return (Playlist.IndexOf(currentTrack) == 0);
        }

        /*public void LastFm()
        {
            QueuingScrobbler scrobbler = new QueuingScrobbler("API_KEY", "API_SECRET", "SESSION_KEY");
            scrobbler.
        }*/
    }
}
