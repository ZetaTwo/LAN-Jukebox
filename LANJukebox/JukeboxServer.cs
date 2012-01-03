using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace LANJukebox
{
    delegate void FileUploaded(TempFile file);

    /// <summary>
    /// Serves an interface for uploadning tracks
    /// </summary>
    class JukeboxServer
    {
        private HttpListener listener = new HttpListener();
        public event FileUploaded NewSong;


        /// <summary>
        /// Starts the web server
        /// </summary>
        public void Start()
        {
            /*HttpServer.HttpModules.ResourceFileModule module = new HttpServer.HttpModules.ResourceFileModule();
            module.AddResources("/123", Assembly.GetExecutingAssembly(), "LANJukebox." + "Web.template.index.html");
            HttpServer.HttpServer serv = new HttpServer.HttpServer();
            serv.Add(module);
            serv.ServerName = "LAN Jukebox";
            serv.Start(IPAddress.Any, 3000);*/
            
            listener.Prefixes.Add("http://*:3000/");

            listener.Start();
            listener.BeginGetContext(new AsyncCallback(ListenerCallback), null);
        }

        /// <summary>
        /// Accepts an incoming connection and serves the appropriate content
        /// </summary>
        /// <param name="result">The handle used to get the HttpListenerContext object associated with the connection.</param>
        public void ListenerCallback(IAsyncResult result)
        {
            HttpListenerContext context = listener.EndGetContext(result);

            string file = context.Request.RawUrl.Substring(1);
            string[] action = file.Split('/');
            
            if (file == "") { file = "index.html"; };
            string filePath = "Web/" + file;

            switch (action[0])
            {
                case "css":
                case "js":
                case "img":
                    ServeFile(context, filePath);
                    break;
                case "":
                    ActionIndex(context);
                    break;
                case "submit":
                    ActionSubmit(context);
                    break;
                default:
                    ServeFile(context, filePath);
                    break;
            }

            try
            {
                context.Response.Close();
            }
            catch
            {
            }

            listener.BeginGetContext(new AsyncCallback(ListenerCallback), null);
        }

        //TODO: Display current playlist
        /// <summary>
        /// Displays the index page with an upload form.
        /// </summary>
        /// <param name="context">The current connection context</param>
        private void ActionIndex(HttpListenerContext context)
        {
            /*context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;

            Stream data = new MemoryStream();
            long contentLength = 0;


            Stream output = null;

            output = context.Response.OutputStream;
            contentLength += AppendFile(data, "Web/template/top.html");
            //contentLength += AppendFile(data, "Web/template/bottom.html");
            context.Response.ContentLength64 = contentLength;
            data.CopyTo(output, 1024);
            //data.CopyTo(output);


            try
            {
                output.Close();
            }
            catch (Exception e)
            {
            }*/

            ServeFile(context, "Web/template/index.html");
        }

        /// <summary>
        /// Uploads a file to the server
        /// </summary>
        /// <param name="context">The current connection context</param>
        private void ActionSubmit(HttpListenerContext context)
        {
            if (context.Request.ContentLength64 < 1000000000)
            {
                TempFile tempFile = new TempFile();

                try
                {
                    SaveFile(context.Request.ContentEncoding, GetBoundary(context.Request.ContentType), context.Request.InputStream, tempFile.Path);
                    NewSong(tempFile);
                }
                catch
                {
                }

                context.Response.StatusCode = 303;
                Uri uri = context.Request.Url;
                context.Response.Headers.Add("Location: " + uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port + "/");
            }
            else
            {
                context.Response.StatusCode = 403;
                context.Response.StatusDescription = "Maximum file size is 100mb.";
            }
        }

        /// <summary>
        /// Serve a static embedded file.
        /// </summary>
        /// <param name="context">The current connection context</param>
        /// <param name="filePath">The path to the file.</param>
        private void ServeFile(HttpListenerContext context, string filePath)
        {
            string resource = "LANJukebox." + filePath.Replace('/', '.');
            Stream file = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (file != null)
            {
                context.Response.ContentType = GetContentType(Path.GetExtension(filePath));
                context.Response.ContentEncoding = System.Text.Encoding.UTF8;

                Stream output = context.Response.OutputStream;
                
                StreamReader index = new StreamReader(file);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(index.ReadToEnd());
                index.Close();

                context.Response.ContentLength64 = buffer.Length;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.ContentLength64 = 0;
            }
        }

        //TODO: Does not work
        private int AppendFile(Stream output, string filePath)
        {
            StreamReader index = File.OpenText(filePath);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(index.ReadToEnd());
            index.Close();

            output.Write(buffer, 0, buffer.Length);
            return buffer.Length;
        }

        /// <summary>
        /// Returns the content type from a given file extension
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <returns>The content type associated with the file extension</returns>
        private static string GetContentType(string extension)
        {
            switch (extension)
            {
                case ".css": return "text/css";
                case ".html": return "text/html";
                case ".jpg": return "image/jpeg";
                case ".js": return "application/x-javascript";
                case ".mp3": return "audio/mpeg";
                case ".png": return "image/png";
                default: return "application/octet-stream";
            }
        }

        /// <summary>
        /// Returns the post data boundary.
        /// </summary>
        /// <param name="ctype">The ctype string</param>
        /// <returns>The post data boundary</returns>
        private static String GetBoundary(String ctype)
        {
            return "--" + ctype.Split(';')[1].Split('=')[1];
        }

        /// <summary>
        /// Save the uploaded file to disk
        /// </summary>
        /// <param name="enc">The file encoding</param>
        /// <param name="boundary">The post data boundary</param>
        /// <param name="input">The uploaded data</param>
        /// <param name="fileName">The file to save to</param>
        private static void SaveFile(Encoding enc, String boundary, Stream input, string fileName)
        {
            Byte[] boundaryBytes = enc.GetBytes(boundary);
            Int32 boundaryLen = boundaryBytes.Length;

            using (FileStream output = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                Byte[] buffer = new Byte[1024];
                Int32 len = input.Read(buffer, 0, 1024);
                Int32 startPos = -1;

                // Find start boundary
                while (true)
                {
                    if (len == 0)
                    {
                        throw new Exception("Start Boundaray Not Found");
                    }

                    startPos = IndexOf(buffer, len, boundaryBytes);
                    if (startPos >= 0)
                    {
                        break;
                    }
                    else
                    {
                        Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);
                        len = input.Read(buffer, boundaryLen, 1024 - boundaryLen);
                    }
                }

                // Skip four lines (Boundary, Content-Disposition, Content-Type, and a blank)
                for (Int32 i = 0; i < 4; i++)
                {
                    while (true)
                    {
                        if (len == 0)
                        {
                            throw new Exception("Preamble not Found.");
                        }

                        startPos = Array.IndexOf(buffer, enc.GetBytes("\n")[0], startPos);
                        if (startPos >= 0)
                        {
                            startPos++;
                            break;
                        }
                        else
                        {
                            len = input.Read(buffer, 0, 1024);
                        }
                    }
                }


                Array.Copy(buffer, startPos, buffer, 0, len - startPos);
                len = len - startPos;


                while (true)
                {
                    Int32 endPos = IndexOf(buffer, len, boundaryBytes);
                    if (endPos >= 0)
                    {
                        if (endPos > 0) output.Write(buffer, 0, endPos);
                        break;
                    }
                    else if (len <= boundaryLen)
                    {
                        throw new Exception("End Boundaray Not Found");
                    }
                    else
                    {

                        output.Write(buffer, 0, len - boundaryLen);
                        Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);

                        try
                        {
                            len = input.Read(buffer, boundaryLen, 1024 - boundaryLen) + boundaryLen;
                        }
                        catch
                        {
                            //Input aborted, delete file
                            output.Close();

                            throw new Exception("Client aborted");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the index of a given boundary within a buffer
        /// </summary>
        /// <param name="buffer">The buffer to search</param>
        /// <param name="len">The length of the buffer</param>
        /// <param name="boundaryBytes">The boundary to look for</param>
        /// <returns></returns>
        private static Int32 IndexOf(Byte[] buffer, Int32 len, Byte[] boundaryBytes)
        {
            for (Int32 i = 0; i <= len - boundaryBytes.Length; i++)
            {
                Boolean match = true;
                for (Int32 j = 0; j < boundaryBytes.Length && match; j++)
                {
                    match = buffer[i + j] == boundaryBytes[j];
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
