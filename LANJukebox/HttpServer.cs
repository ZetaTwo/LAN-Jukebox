using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace LANJukebox
{
    public delegate void FileUploaded(string file);

    class HttpServer
    {
        private HttpListener listener = new HttpListener();
        public event FileUploaded NewSong;

        private string tempDir;
        private volatile int fileNumber = 0;

        public HttpServer(string dir)
        {
            tempDir = dir;

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            Directory.CreateDirectory(tempDir);
        }

        public void Start()
        {
            listener.Prefixes.Add("http://*:3000/");

            listener.Start();
            listener.BeginGetContext(new AsyncCallback(ListenerCallback), null);
        }

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

        private void ActionSubmit(HttpListenerContext context)
        {
            if (context.Request.ContentLength64 < 1000000000)
            {
                //string fileName = "song" + (fileNumber++).ToString() + ".mp3";
                string fileName = Path.Combine(tempDir, Path.GetRandomFileName());
                try
                {
                    SaveFile(context.Request.ContentEncoding, GetBoundary(context.Request.ContentType), context.Request.InputStream, fileName);
                    NewSong(fileName);
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

        private void ServeFile(HttpListenerContext context, string filePath)
        {
            if (File.Exists(filePath))
            {
                context.Response.ContentType = GetContentType(Path.GetExtension(filePath));
                context.Response.ContentEncoding = System.Text.Encoding.UTF8;

                Stream output = context.Response.OutputStream;

                StreamReader index = File.OpenText(filePath);
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

        private static String GetBoundary(String ctype)
        {
            return "--" + ctype.Split(';')[1].Split('=')[1];
        }

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
                                File.Delete(fileName);

                                throw new Exception("Client aborted");
                            }
                        }
                    }
            }
        }

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
