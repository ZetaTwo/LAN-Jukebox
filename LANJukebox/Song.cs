using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using Un4seen.Bass.AddOn.Tags;

namespace LANJukebox
{
    class Song
    {
        TAG_INFO tags;
        public TAG_INFO Tags
        {
            get { return tags; }
        }

        public Song(string filename)
        {
            tags = BassTags.BASS_TAG_GetFromFile(filename, true, true);
        }
    }
}
