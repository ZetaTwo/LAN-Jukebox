using System.Collections.Generic;
using System;
using System.Threading;

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

        Lpfm.LastFmScrobbler.Scrobbler auth;
        Lpfm.LastFmScrobbler.QueuingScrobbler scrobbler;
        Lpfm.LastFmScrobbler.Track currentScrobble;
        const string LASTFM_KEY = "5256b2ac1511d9f7af4eea20975600ca";
        const string LASTFM_SECRET = "b7f519d5bbb61733432c49e53dd115d3";

        private string lastFmSession;
        public string LastFmSession
        {
            get { return lastFmSession; }
        }

        public event TrackEvent TrackAdd;
        public event TrackEvent TrackDelete;
        public event TrackEvent TrackPlay;


        public LANPlayerCore()
        {
            Init();
        }

        public LANPlayerCore(string lastfm_session)
        {
            Init();
            lastFmSession = lastfm_session;
            try
            {
                scrobbler = new Lpfm.LastFmScrobbler.QueuingScrobbler(LASTFM_KEY, LASTFM_SECRET, lastfm_session);
            }
            catch
            {
                lastFmSession = null;
            }
        }

        private void Init()
        {
            player.TrackEnd += new TrackEvent(TrackEnded);
            player.TrackScrobble += new TrackEvent(TrackScrobble);

            server.NewSong += new FileUploaded(TrackAdded);
            server.Start();
        }

        public void Next()
        {
            if (!LastTrack())
            {
                int currentTrackIndex = Playlist.IndexOf(currentTrack);
                Track nextTrack = Playlist[currentTrackIndex + 1];
                PlayTrack(nextTrack);
            }
        }

        public void Previous()
        {
            if (!FirstTrack())
            {
                int currentTrackIndex = Playlist.IndexOf(currentTrack);
                Track nextTrack = Playlist[currentTrackIndex - 1];
                PlayTrack(nextTrack);
            }
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

            Audio.LoadTrack(currentTrack);
            Audio.PlayPause();

            while (Playlist.IndexOf(currentTrack) >= HistorySize)
            {
                DeleteTrack(0);
            }

            TrackPlay(track);
            if (scrobbler != null)
            {
                currentScrobble = TrackToLastFm(track);
                scrobbler.NowPlaying(currentScrobble);
                
                Thread process = new Thread(ScrobbleProcess);
                process.Start();
            }
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

            //Get track and remove it from playlist
            Track track = Playlist[index];
            Playlist.RemoveAt(index);

            TrackDelete(track);
        }

        public bool LastTrack()
        {
            return (Playlist.IndexOf(currentTrack) == Playlist.Count - 1);
        }

        public bool FirstTrack()
        {
            return (Playlist.IndexOf(currentTrack) == 0);
        }

        public string BeginLastFmAuth()
        {
            auth = new Lpfm.LastFmScrobbler.Scrobbler(LASTFM_KEY, LASTFM_SECRET);
            return auth.GetAuthorisationUri();
        }

        public string EndLastFmAuth()
        {
            lastFmSession = auth.GetSession();
            scrobbler = new Lpfm.LastFmScrobbler.QueuingScrobbler(LASTFM_KEY, LASTFM_SECRET, lastFmSession);

            return lastFmSession;
        }

        public void RemoveLastFmAuth()
        {
            scrobbler = null;
            lastFmSession = null;
        }

        private void TrackScrobble(Track track)
        {
            if (scrobbler != null)
            {
                scrobbler.Scrobble(currentScrobble);

                Thread process = new Thread(ScrobbleProcess);
                process.Start();
            }
        }

        private void ScrobbleProcess()
        {
#if DEBUG
            scrobbler.Process(true);
#else
            scrobbler.Process();
#endif
        }

        public static Lpfm.LastFmScrobbler.Track TrackToLastFm(Track track)
        {
            Lpfm.LastFmScrobbler.Track scrobbleTrack = new Lpfm.LastFmScrobbler.Track();
            scrobbleTrack.AlbumArtist = track.Tags.albumartist;
            scrobbleTrack.AlbumName = track.Tags.album;
            scrobbleTrack.ArtistName = track.Tags.artist;
            scrobbleTrack.Duration = new TimeSpan((long)(track.Tags.duration * 10e6));
            scrobbleTrack.TrackName = track.Tags.title;
            try
            {
                scrobbleTrack.TrackNumber = int.Parse(track.Tags.track.Substring(0, 2));
            }
            catch
            {
                scrobbleTrack.TrackNumber = 0;
            }
            scrobbleTrack.WhenStartedPlaying = DateTime.Now;

            return scrobbleTrack;
        }
    }
}
