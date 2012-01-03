using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LANJukebox
{
    /// <summary>
    /// Represents an audio device on the computer
    /// </summary>
    public class AudioDevice
    {
        int id;
        /// <summary>
        /// The ID of the audio device. 0 = No sound, 1 = First real device
        /// </summary>
        public int Id
        {
            get { return id; }
        }

        string name;
        /// <summary>
        /// The name of the audio device
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Instantiates an AudioDevice object
        /// </summary>
        /// <param name="_id">The ID of the device</param>
        /// <param name="_name">The name of the device</param>
        public AudioDevice(int _id, string _name)
        {
            id = _id;
            name = _name;
        }

        /// <summary>
        /// Returns the device name
        /// </summary>
        /// <returns>The device name</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
