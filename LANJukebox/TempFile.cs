using System;
using System.IO;

namespace LANJukebox
{
    /// <summary>
    /// A temporary file which is deleted when the class is disposed
    /// </summary>
    sealed class TempFile : IDisposable
    {
        string path;
        public TempFile() : this(System.IO.Path.GetTempFileName()) { }

        /// <summary>
        /// Creates a new TempFile with the given path
        /// </summary>
        /// <param name="path">Path to the file</param>
        public TempFile(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            this.path = path;
        }

        /// <summary>
        /// The full path to the file.
        /// </summary>
        public string Path
        {
            get
            {
                if (path == null) throw new ObjectDisposedException(GetType().Name);
                return path;
            }
        }

        ~TempFile() { Dispose(false); }

        /// <summary>
        /// Disposes the file, deleting it.
        /// </summary>
        public void Dispose() { Dispose(true); }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            if (path != null)
            {
                try { File.Delete(path); }
                catch { } // best effort
                path = null;
            }
        }
    }
}
