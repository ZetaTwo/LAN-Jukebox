using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LANJukebox;

namespace LANJukeboxCL
{
    class LANJukeboxCL
    {
        LANPlayerCore Player = new LANPlayerCore();
        Track currentTrack;
        List<Track> Playlist = new List<Track>();

        public LANJukeboxCL()
        {
            Player.TrackAdd += new TrackEvent(TrackAdded);
            Player.TrackPlay += new TrackEvent(TrackPlay);
            Player.TrackDelete += new TrackEvent(TrackDelete);
        }

        public void Run()
        {
            Console.WriteLine("Starting LAN Jukebox");
            Console.WriteLine("Listening on port 3000");
            bool running = true;
            while (running)
            {
                switch (Console.ReadLine())
                {
                    case "exit":
                        running = false;
                        break;
                    case "playlist":
                        DisplayPlaylist();
                        break;
                    case "next":
                        Player.Next();
                        break;
                    case "prev":
                    case "previous":
                        Player.Previous();
                        break;
                    case "play":
                    case "pause":
                        Player.Audio.PlayPause();
                        break;
                    default:
                        DisplayCommands();
                        break;
                }
            }
        }

        private void DisplayPlaylist()
        {
            Console.WriteLine("Current playlist:");
            foreach (Track track in Playlist)
            {
                if (currentTrack == track)
                {
                    Console.WriteLine(track.Tags.artist + " - " + track.Tags.title + " (playing)");
                }
                else
                {
                    Console.WriteLine(track.Tags.artist + " - " + track.Tags.title);
                }
            }
        }

        private void DisplayCommands()
        {
            Console.WriteLine("Available commands");
            Console.WriteLine("exit");
            Console.WriteLine("play");
            Console.WriteLine("next");
            Console.WriteLine("previous");
            Console.WriteLine("playlist");
            Console.WriteLine("");
        }

        private void TrackAdded(Track track)
        {
            Console.WriteLine("Added: " + track.Tags.artist + " - " + track.Tags.title);

            Playlist.Add(track);
            if (Playlist.Count == 1)
            {
                Player.Next();
            }
        }

        private void TrackPlay(Track track)
        {
            Console.WriteLine("Playing: " + track.Tags.artist + " - " + track.Tags.title);
            currentTrack = track;
        }

        private void TrackDelete(Track track)
        {
            Console.WriteLine("Deleted: " + track.Tags.artist + " - " + track.Tags.title);
            Playlist.Remove(track);
        }
    }
}
