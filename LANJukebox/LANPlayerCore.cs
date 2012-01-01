using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LANJukebox
{
    class LANPlayerCore
    {
        AudioPlayer player = new AudioPlayer();
        internal AudioPlayer Player
        {
            get { return player; }
        }
    }
}
