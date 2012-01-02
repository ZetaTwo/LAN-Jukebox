using Un4seen.Bass.AddOn.Tags;

namespace LANJukebox
{
    public class Track
    {
        TAG_INFO tags;
        public TAG_INFO Tags
        {
            get { return tags; }
        }

        TempFile file;

        internal Track(TempFile tempFile)
        {
            file = tempFile;
            tags = BassTags.BASS_TAG_GetFromFile(file.Path, true, true);
        }
    }
}
