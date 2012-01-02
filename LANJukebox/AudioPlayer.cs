using System;
using System.Collections.Generic;
using Un4seen.Bass;

namespace LANJukebox
{
    public class AudioPlayer
    {
        int currentHandle = 0;
        Track currentSong = null;
        bool playing = false;
        public bool Playing
        {
            get { return playing; }
        }

        public int Volume
        {
            get { return (int)(100 * Bass.BASS_GetVolume()); }
            set { Bass.BASS_SetVolume((float)value / 100); }
        }

        public event TrackEvent TrackEnd;
        public event TrackEvent TrackScrobble;

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

                if (currentSong != null)
                {
                    PlayPause();
                }

                bool result = Bass.BASS_SetDevice(value);

                if (currentSong != null)
                {
                    PlayPause();
                }
            }
        }

        public AudioPlayer()
        {
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, System.IntPtr.Zero);
        }

        ~AudioPlayer()
        {
            Stop();
            Bass.BASS_Free();
        }

        public void LoadSong(Track song)
        {
            Stop();
            currentSong = song;
            
        }

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

        public void EndTrackProc(int arg1, int arg2, int arg3, IntPtr arg4)
        {
            TrackEnd(currentSong);
        }

        public void ScrobbleTrackProc(int arg1, int arg2, int arg3, IntPtr arg4)
        {
            TrackScrobble(currentSong);
        }

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

        public void PlayPause()
        {
            if (currentSong == null)
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
                    currentHandle = Bass.BASS_StreamCreateFile(currentSong.Tags.filename, 0, 0, BASSFlag.BASS_DEFAULT);
                    Bass.BASS_ChannelSetSync(currentHandle, BASSSync.BASS_SYNC_END, 0, EndTrackProc, IntPtr.Zero);
                    
                    long scrobblePos = Bass.BASS_ChannelGetLength(currentHandle) / 2;
                    Bass.BASS_ChannelSetSync(currentHandle, BASSSync.BASS_SYNC_POS, scrobblePos, ScrobbleTrackProc, IntPtr.Zero);
                }
                Bass.BASS_ChannelPlay(currentHandle, false);
                playing = true;
            }
        }
    }
}
