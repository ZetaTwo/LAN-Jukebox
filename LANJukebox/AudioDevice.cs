using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LANJukebox
{
    class AudioDevice
    {
        int id;
        public int Id
        {
            get { return id; }
        }

        string name;
        public string Name
        {
            get { return name; }
        }

        public AudioDevice(int _id, string _name)
        {
            id = _id;
            name = _name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
