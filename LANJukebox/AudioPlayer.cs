using System;
using System.Collections.Generic;
using Un4seen.Bass;

namespace LANJukebox
{
    /// <summary>
    /// Class responsible for loading and playing audio tracks
    /// </summary>
    public class AudioPlayer
    {
        int currentHandle = 0;
        Track currentTrack = null;
        bool playing = false;
        /// <summary>
        /// Gets the playing status of the player.
        /// </summary>
        public bool Playing
        {
            get { return playing; }
        }

        /// <summary>
        /// Gets or sets the volume level (0-100).
        /// </summary>
        public int Volume
        {
            get { return (int)(100 * Bass.BASS_GetVolume()); }
            set { Bass.BASS_SetVolume((float)value / 100); }
        }

        /// <summary>
        /// This event is fired when the current track ends.
        /// </summary>
        public event TrackEvent TrackEnd;

        /// <summary>
        /// This event is fired when the current has played halfway and should be scrobbled.
        /// </summary>
        public event TrackEvent TrackScrobble;

        /// <summary>
        /// Gets or sets the current audio device
        /// </summary>
        public AudioDevice CurrentDevice
        {
            get
            {
                int device_number = DeviceIndex;
                BASS_DEVICEINFO device = Bass.BASS_GetDeviceInfo(device_number);
                return new AudioDevice(device_number, device.name);
            }
            set { DeviceIndex = value.Id; }
        }

        /// <summary>
        /// Gets or sets the index of the current audio device
        /// </summary>
        public int DeviceIndex
        {
            get { return Bass.BASS_GetDevice(); }
            set
            {
                BASS_DEVICEINFO device = Bass.BASS_GetDeviceInfo(value);
                if (device.IsEnabled && !device.IsInitialized)
                {
                    Bass.BASS_Init(value, 44100, BASSInit.BASS_DEVICE_DEFAULT, System.IntPtr.Zero);
                }

                if (currentTrack != null)
                {
                    PlayPause();
                }

                bool result = Bass.BASS_SetDevice(value);

                if (currentTrack != null)
                {
                    PlayPause();
                }
            }
        }

        /// <summary>
        /// Instantiate an AudioPlayer and initializes the default audio device.
        /// </summary>
        public AudioPlayer()
        {
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, System.IntPtr.Zero);
        }

        ~AudioPlayer()
        {
            Stop();
            Bass.BASS_Free();
        }

        /// <summary>
        /// Loads a track as the current track.
        /// </summary>
        /// <param name="track"></param>
        public void LoadTrack(Track track)
        {
            Stop();
            currentTrack = track;
            
        }

        /// <summary>
        /// Stops playback of the current tracks and unloads the audio data.
        /// </summary>
        public void Stop()
        {
            if (playing)
            {
                playing = false;
                Bass.BASS_ChannelStop(currentHandle);
            }

            if (currentHandle != 0)
            {
                Bass.BASS_StreamFree(currentHandle);
                currentHandle = 0;
            }
        }

        /// <summary>
        /// Get all audio devices available on the computer.
        /// </summary>
        /// <returns>An array of audio devices on the computer</returns>
        public AudioDevice[] GetDevices()
        {
            BASS_DEVICEINFO[] devices = Bass.BASS_GetDeviceInfos();
            List<AudioDevice> retval = new List<AudioDevice>();
            for(int i = 0; i < devices.Length; i++)
            {
                if(devices[i].IsEnabled)
                {
                retval.Add(new AudioDevice(i, devices[i].name));
                }
            }

            return retval.ToArray();
        }

        /// <summary>
        /// Starts, pauses or resumes playback of the current song
        /// </summary>
        public void PlayPause()
        {
            if (currentTrack == null)
            {
                throw new Exception("No song loaded.");
            }

            if (playing)
            {
                Bass.BASS_ChannelPause(currentHandle);
                playing = false;
            }
            else
            {
                if (currentHandle == 0)
                {
                    currentHandle = Bass.BASS_StreamCreateFile(currentTrack.Tags.filename, 0, 0, BASSFlag.BASS_DEFAULT);
                    Bass.BASS_ChannelSetSync(currentHandle, BASSSync.BASS_SYNC_END, 0, EndTrackProc, IntPtr.Zero);
                    
                    long scrobblePos = Bass.BASS_ChannelGetLength(currentHandle) / 2;
                    Bass.BASS_ChannelSetSync(currentHandle, BASSSync.BASS_SYNC_POS, scrobblePos, ScrobbleTrackProc, IntPtr.Zero);
                }
                Bass.BASS_ChannelPlay(currentHandle, false);
                playing = true;
            }
        }

        private void EndTrackProc(int arg1, int arg2, int arg3, IntPtr arg4)
        {
            TrackEnd(currentTrack);
        }

        private void ScrobbleTrackProc(int arg1, int arg2, int arg3, IntPtr arg4)
        {
            TrackScrobble(currentTrack);
        }
    }
}
