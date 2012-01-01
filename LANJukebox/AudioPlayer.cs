using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;

namespace LANJukebox
{
    public delegate void TrackEnded();

    class AudioPlayer
    {
        int currentHandle = 0;
        Song currentSong = null;
        bool playing = false;
        SYNCPROC endTrack;

        public event TrackEnded TrackEnd;

        public AudioDevice CurrentDevice
        {
            get
            {
                int device_number = Bass.BASS_GetDevice();
                BASS_DEVICEINFO device = Bass.BASS_GetDeviceInfo(device_number);
                return new AudioDevice(device_number, device.name);
            }
            set
            {
                BASS_DEVICEINFO device = Bass.BASS_GetDeviceInfo(value.Id);
                if (device.IsEnabled && !device.IsInitialized)
                {
                    Bass.BASS_Init(value.Id, 44100, BASSInit.BASS_DEVICE_DEFAULT, System.IntPtr.Zero);
                }

                if (currentSong != null)
                {
                    PlayPause();
                }

                bool result = Bass.BASS_SetDevice(value.Id);

                if (currentSong != null)
                {
                    PlayPause();
                }
                
            }
        }

        public AudioPlayer()
        {
            endTrack = new SYNCPROC(EndTrackProc);
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, System.IntPtr.Zero);
        }

        ~AudioPlayer()
        {
            Bass.BASS_Free();
        }

        public void LoadSong(Song song)
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
                Bass.BASS_StreamFree(currentHandle);
                currentHandle = 0;
            }
        }

        public void EndTrackProc(int arg1, int arg2, int arg3, IntPtr arg4)
        {
            TrackEnd();
        }

        public void SetVolume(int volume)
        {
            Bass.BASS_SetVolume((float)volume / 100);
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
                    Bass.BASS_ChannelSetSync(currentHandle, BASSSync.BASS_SYNC_END, 0, endTrack, IntPtr.Zero);
                }
                Bass.BASS_ChannelPlay(currentHandle, false);
                playing = true;
            }
        }
    }
}
